# 旷野征途 2.4 / vc24 · Java/TapTap 侧静态分析（只读）

> 生成：2026-11-16　作者：gamma（Java/TapTap 静态分析）
> 范围：仅静态。未组包、未签名、未装机、未改 `04_scripts/**`、未改框架 Java 公共文件、未 commit。
> 工作区：`13_scratch/kyzt24_java/`（dex 副本 + 反汇编 + 类清单）。底包 `02_apks/旷野征途/旷野征途_2.4_taptap_official.apk` 只读。

## ① 结论

1. **2.4 走 A 轨（改包法）**。依据：`com.taptap.sdk.*` 在包内**明文可达**、未加密未藏 payload —— 8 个 dex 中 `classes.dex`(268 类) / `classes6.dex`(780 类) / `classes7.dex`(1530 类) 共 **2578 个 taptap 类**，三处锚点类全部命中（§③-1）。按 `skill/re-analyze/login/taptap-web-login.md` 现行口径「包内 TapSDK 可达 ⇒ 改包法」，不启用 SPI 作为主路径。
2. **需要改的是 3 处（2.2 那三处仍全部有效）+ 建议追加第 4 处 `TapSignKit.getSign` 签名回灌**。签名回灌是现行口径里「必需步骤」，且本轮静态定位到 1001 的**唯一 Java 签名来源**就是它（§③-5）。
3. **与 2.2 的最大差异不是 SDK，而是 dex 布局**：2.2 全部锚点在 `classes.dex`（2 dex 包）；2.4 是 **8 dex 包**，锚点分裂到 `classes6.dex`（initializer/gatekeeper）与 `classes7.dex`（login/kit）⇒ **P3 组包必须替换两个 dex**，不能沿用 2.2 的单 dex 假设。
4. **TapSDK 版本未变（4.10.9），类集合几乎逐类相同**：taptap 类 2.2=2579 / 2.4=2578，新增 **0**，消失 **1**（`cloudsave/ArchiveData$$ExternalSyntheticBackport0`，编译器产物）。initializer 包 2.2=75 / 2.4=75，**无新增 Gatekeeper 类**。
5. **一个 2.2 遗留误判被本轮静态否掉**：`TapTapSdkInitializer.isGatekeeperLoadedSuccessfully()Z` 在 2.2 与 2.4 **全 dex 均无 Java 调用点**（仅自身 + Kotlin metadata 字符串）⇒ 补丁 #1 属「死补丁」，它对 1001 从未起作用；21:09 那轮「三处补丁仍 1001」的原因因此是结构性的，不是补丁没生效。
6. **killer（B 轨运行时替换）在 2.4 仍然可用且不需要重做**：killer 内置证书 DER 678 B，实测 md5 `360c78f5…5a31` / sha256 `0a3b7825…048f`，与 2.4 官方签名证书**双指纹一致**；manifest 仍无 `android:name`、`appComponentFactory` 仍为 `androidx.core.app.CoreComponentFactory` ⇒ 父类 `android.app.Application` 的改法继续成立。**建议定位为回退项**（见 ④-1）。

## ② 补丁清单（基线 + 2.4 判定）

| # | 类（描述符） | 方法（描述符） | 2.2 原始指令 | 2.2 改后 | 2.4 位置 | 2.4 判定 |
|---|---|---|---|---|---|---|
| 1 | `Lcom/taptap/sdk/initializer/TapTapSdkInitializer;` | `isGatekeeperLoadedSuccessfully()Z` | `.registers 2` + `invoke-virtual getGateKeepManager` → `invoke-virtual GatekeeperManager->isGatekeeperLoadedSuccessfully()Z` → `move-result v0` → `return v0` | `.registers 2` + `const/4 v0,0x1` + `return v0` | `classes6.dex` 行 718 | **仍在、方法体逐字未变**；但**无任何调用点**（死补丁）⇒ 保留无害，不作为修复依据 |
| 2 | `Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;` | `showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V`（private final） | 取 `Err->getMsg()` → `SysExtKt->showToast` → 取 `Err->getDescription()` → `TapLogger->loge$default` → `return-void` | `.registers 5` + `return-void` | `classes6.dex` 行 487 | **仍在、方法体逐字未变**（唯一差异：`:cond_18`→`:cond_17`、`const-string/jumbo`→`const-string`，均为重编译产物）⇒ 有效，只掐提示不改结果 |
| 3 | `Lcom/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider;` | `provide(Lcom/taptap/sdk/login/impl/data/model/LoginRequest;)Landroid/app/Fragment;` | `aget v2, v3, v2` 后接 `const/4 v3,0x1` / `if-eq` 三分支（1=ClientLoginFragment、2=CloudGameFragment、3=WebLoginFragment） | 在 `aget` 之后插 `const/4 v2, 0x3` | `classes7.dex` 行 94（`aget` 在行 184） | **类文件与 2.2 官方版逐字相同**（去 `.line`/`.local` 后 diff=0），锚点在全类内唯一（count=1）⇒ 原补丁脚本可直接复用 |
| 4（新增，建议） | `Lcom/taptap/sdk/kit/internal/utils/TapSignKit;` | `getSign(Landroid/content/Context;)Ljava/lang/String;` | `getPackageManager()` → `getPackageName()` → `getPackageInfo(pkg, 0x40)` → `PackageInfo.signatures` → `firstOrNull` → `Signature->toByteArray()` → `CommonUtils->getMD5([B)` → 返回；异常走 `printStackTrace` + 返回 null | 建议 `.registers 2` + `const-string v0, "360c78f563dd471fa6df02cc68045a31"` + `return-object v0`（**待 P3 授权后实做**；本表为设计，不是已落地产物） | `classes7.dex` 行 59 | **方法体与 2.2 逐字相同**；是 1001 路径上唯一的 Java 签名来源（§③-5） |

说明：补丁 4 的常量 = 官方证书 DER 的 MD5（`apksigner verify --print-certs` 实测 `Signer #1 certificate MD5 digest: 360c78f563dd471fa6df02cc68045a31`）；`CommonUtils.getHashString` 逐 nibble `Integer.toHexString` 拼接、不补零 ⇒ 输出即该 32 位小写十六进制串，格式对齐。

## ③ 证据切片（文件:行 / 实测命令）

### ③-1 底包与布局（2.4）

| 项 | 值 | 取证 |
|---|---|---|
| APK | `02_apks/旷野征途/旷野征途_2.4_taptap_official.apk`，194,003,543 B | `ls -l`（beta 拉取产物，只读） |
| sha256 | `8d6776704bcaa6bc358e0a4e63eab16d57a38ced518db694485b32f3a17e484e` | `shasum -a 256` |
| 版本 | versionName **2.4** / versionCode **24**；minSdk 24 / targetSdk **35** | `aapt2 dump xmltree` |
| 签名 | `O=xiefeng`；SHA-256 `0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f`；MD5 `360c78f563dd471fa6df02cc68045a31` | `apksigner verify --print-certs` |
| dex | **8 个**：9,210,068 / 8,852,256 / 8,602,228 / 8,510,712 / 7,376,684 / 9,343,240 / 11,283,180 / 868,008 B | `unzip -l` |
| 类总数 | 63,035（2.2 两 dex 合计 10,696） | `baksmali list classes` 汇总 |
| libil2cpp | 65,835,528 B（2.2 = 65,520,080） | `unzip -l` |
| manifest | `<application>` **无 `android:name`**；`appComponentFactory="androidx.core.app.CoreComponentFactory"`；`TapTapLoginActivity` 仍声明（行 598） | `aapt2 dump xmltree` |

各 dex 类数 / taptap 类数（`baksmali list classes`）：

    classes.dex   8112 类, taptap 268   (common/cloudsave/base)
    classes2.dex  8341 类, taptap 0
    classes3.dex  8786 类, taptap 0
    classes4.dex  8255 类, taptap 0
    classes5.dex  9636 类, taptap 0
    classes6.dex  9479 类, taptap 780   (initializer 75 / compliance 227 / components 195 / db 190 / core 36)
    classes7.dex  9539 类, taptap 1530  (update 295 / leaderboard 232 / okhttp 205 / login 195 / kit 187)
    classes8.dex   887 类, taptap 0

锚点类归属（`grep` 各 dex 类清单）：`TapTapSdkInitializer`→classes6；`GatekeeperRepository(+4 内部类)`→classes6；`DefaultTapLoginFragmentProvider(+WhenMappings)`→classes7；`WebLoginFragment`(11 个类)→classes7；`TapSignKit`→classes7。

### ③-2 TapSDK 版本

`const-string "4.10.9"` 命中 5 处：`classes.dex: cloudsave/internal/TapCloudSaveInitTask.smali:389`、`cloudsave/internal/track/CloudSaveTracker$loggerHelper$2.smali:89`；`classes6.dex: core/unity/BridgeCoreServiceImpl$openlog$2.smali:79`、`compliance/internal/ComplianceTracker$loggerHelper$2.smali:89`、`components/androidx/webview/jsbridge/handler/TapSdkCommonParamsHandler.smali:148`。⇒ **仍 4.10.9（与 2.2 同版本，未升级）**。

类集合差（`comm`，2.2 = classes.dex + classes2.dex 全量）：taptap 侧新增 **0**、消失 **1**（`Lcom/taptap/sdk/cloudsave/ArchiveData$$ExternalSyntheticBackport0;`）；`com/taptap/sdk/initializer/**` 75 → 75。

### ③-3 补丁 #1/#2 的 2.2↔2.4 对照切片

`TapTapSdkInitializer.smali`（去 `.line`/`.local` 后 `diff` = **0 行**）：

    .method public final isGatekeeperLoadedSuccessfully()Z
        .registers 2
        invoke-virtual {p0}, ...->getGateKeepManager()Lcom/taptap/sdk/initializer/manager/GatekeeperManager;
        move-result-object v0
        invoke-virtual {v0}, ...GatekeeperManager;->isGatekeeperLoadedSuccessfully()Z
        move-result v0
        return v0
    .end method

`GatekeeperRepository.smali`：仅 3 处差异，全部为编号/常量池宽度 —— `:cond_18`→`:cond_17`、`:goto_31`→`:goto_2f`、`const-string/jumbo "当前应用还未初始化"`→`const-string …`。`showErrorToast` 方法体其余指令逐字相同。

**调用点证据（关键）**：`grep -rn isGatekeeperLoadedSuccessfully dis24_c*/ | grep -v 上述三类` ⇒ **0 命中**；2.2 侧（`dis22_official/` + `dis22_official_c2/`）同样 **0 命中**。命中行只有 3 个定义处 + Kotlin metadata 的 `d2` 字符串数组（`TapTapSdkInitializer.smali:52`、`GatekeeperRepository.smali:54`）。

### ③-4 补丁 #3 的 2.2↔2.4 对照切片

`DefaultTapLoginFragmentProvider.smali`：两版本 `diff`（去 `.line`/`.local`/`.param`）= **0 行**，文件字节数 8,539 = 8,539。关键段（2.4 `classes7.dex:184`，上下文 `classes7.dex:94-250`）：

    .method public provide(Lcom/taptap/sdk/login/impl/data/model/LoginRequest;)Landroid/app/Fragment;
        .registers 25
        ...
        sget-object v3, ...DefaultTapLoginFragmentProvider$WhenMappings;->$EnumSwitchMapping$0:[I
        invoke-virtual {v2}, ...LoginTypes;->ordinal()I
        move-result v2
        aget v2, v3, v2              # ← 行 184：全类唯一，可作插入锚点
        const/4 v3, 0x1
        if-eq v2, v3, :cond_6f      # → ClientLoginFragment
        const/4 v3, 0x2
        if-eq v2, v3, :cond_64      # → CloudGameFragment
        const/4 v3, 0x3
        if-ne v2, v3, :cond_5e      # → WebLoginFragment（:cond_5e 是 NoWhenBranchMatched）
        ...

### ③-5 签名链路（1001 的成因链，逐跳可查）

1. `GatekeeperRepository$initialize$1$networkData$1.smali:232`
   `invoke-virtual {v4, v5}, Lcom/taptap/sdk/kit/internal/utils/TapSignKit;->getSign(Landroid/content/Context;)Ljava/lang/String;` —— **全 dex 唯一调用点**（`grep -rn "TapSignKit;->getSign"` 仅此 1 行）。
2. 同文件 `:239` `new-instance GateKeeperRequest` + `invoke-direct {v1, v5, v3, v4}`，`:204` 处 `const-string v5,"android"`，`v3 = context.getPackageName()` ⇒ 请求三元组 = (platform="android", packageName, sign)。
3. `GatekeeperApiService->fetchGatekeeper`（`:248`）发往 Tap 后端。
4. 失败分支 `GatekeeperRepository$initialize$1.smali`：`:303` 比较 `ApiErr$INVALID_CLIENT` → `:337` `const/16 v2, 0x3e9` → `new-instance TapInitState$Failed` + `tryEmit`；同支先调 `access$showErrorToast`。
5. `TapInitErrorCode.smali:33` `.field public static final CONFIG_ERROR:I = 0x3e9` ⇒ **1001 = CONFIG_ERROR**，与 2.2 实测 `Init callback failed: code=1001` 同码。
6. `TapSignKit.getSign`：`:94` `PackageManager->getPackageInfo(pkg, 0x40)`（GET_SIGNATURES）→ `:130` `CommonUtils->getMD5([B)` → `getHashString` 逐 nibble `toHexString`（小写、无补零）。

结论链：**服务端按 (包名, 签名 MD5) 判 INVALID_CLIENT ⇒ 只要 getSign 返回官方 MD5，该请求即过**。killer 之所以能修 1001，正是因为它在 PackageManager 层回灌官方 DER，使 `getPackageInfo` 返回官方 `Signature`，从而 `getSign` 输出官方 MD5——**两条路作用于同一跳**。

### ③-6 新增校验点扫描（2.4）

| 扫描 | 结果 |
|---|---|
| `com.taptap.sdk.initializer` 包类集合 | 75 = 75，**新增 0**（`comm -13`） |
| 全 8 dex 中调用 `PackageManager.getPackageInfo` 的类 | 414 个文件；`com/taptap/` 仅 4 个：`TapSignKit`(取 signatures)、`DeviceUtils`、`SysExtKt`、`TapIdentifierUtil`（后三者非签名路径，见下条） |
| 其中 `PackageInfo->signatures` 的 TapSDK 侧唯一读取者 | `TapSignKit`（`com/taptap/**` 内唯一） |
| 同时含 `getPackageInfo` 与包名字符串 `com/xf/kyzt` 的类 | **0 个** ⇒ 无「包名+签名」本地自校验类 |
| 游戏自身 Java | `com/xf/kyzt/**` 只有 `R`/`R$*`/`BuildConfig`（22 类），无校验逻辑 |
| `libtapsdkcore.so`（4,226,392 B）字符串 | `strings` 未见 `TapSignKit`/`getSign`/`getPackageInfo`/`Lcom/taptap` 等 Java 侧描述符（见 ④-2，属**弱证据**） |
| 登录类差异 | `TapTapLoginActivity`/`LoginTypes`/`ClientLoginFragment` 与 2.2 逐字相同；`WebLoginFragment` 132 行 diff、`fragmentProvider$1` 12 行 diff，**逐条核对均为寄存器分配/`:cond_` 重编号/`mul-double`→`mul-double/2addr` 类重编译产物，无逻辑变化** |

### ③-7 登录调用链（2.4）

`TapTapLoginActivity.<init>`（`classes7.dex` 行 ~97-110）`new-instance DefaultTapLoginFragmentProvider` + `fragmentProvider$1` 作 callback → 存入字段 `fragmentProvider:Lcom/taptap/sdk/login/impl/ui/fragment/TapLoginFragmentProvider;` → 两处消费：`replaceContentByLoginType`（行 181-184）与另一 site（行 468），均为 `invoke-interface {v0, p1}, TapLoginFragmentProvider;->provide(...)`。

⇒ provider 在包内**只有 1 个实现类**（`grep -rl "implements ...TapLoginFragmentProvider;"` = 1），字段赋值只此一处 ⇒ **`provide()` 仍是唯一注入点，不需要换点**。

## ④ 待验证项与风险

1. **「只 patch getSign」是否足以过 1001 —— 未实测（本轮只读）**。静态已证 Java 侧签名只走 getSign；但 native `libtapsdkcore.so` 是否另有独立校验**无法静态排除**（见 2）。缓解顺序建议：**先出「三处 + getSign」包实测**；若仍 1001，再叠 killer（2.2 已三次冷启验证）。
2. **native 证据缺口**：`libtapsdkcore.so` 的 `strings` 无任何 `Lcom/taptap/` / `Landroid/` 描述符，说明该 so 可能整体混淆或用非字符串形式注册 JNI ⇒ 只能写「未发现」，「不存在」未证。
3. **补丁 #1 是死补丁**：2.2/2.4 均 0 调用点。保留无害（体积 3 条指令），但**不能**拿它当「已做门闩」的依据；若 P3 想最小化改动，可只留 #2/#3/#4。
4. **dex 布局风险**：2.4 是 8 dex 包，P3 必须同时替换 `classes6.dex` 与 `classes7.dex`；框架菜单 dex 只能作 `classes9.dex`、killer（若启用）作 `classes10.dex`。2.2 的 `surgical_kyzt_2.2.sh` 硬编码 `classes3/classes4`，直接复用会撞号。
5. **查12 门禁在 2.4 仍可判**：`pre_install_gate.sh` 查12 按「含 `GatekeeperRepository` 字面量的 dex」逐字节比对（脚本内 `tap_hits` 按名匹配），2.4 命中 `classes6.dex` ⇒ 只要该 dex 被改即 PASS，不会因锚点搬家而误判 SKIP。
6. **未做**：`06_dumped/旷野征途/2.4/`（IL2CPP 侧）不在本轮；Unity/C# 侧是否反射调用 `isGatekeeperLoadedSuccessfully` 未证（Java 侧 0 调用，C# 侧未查 dump）。
7. 工具链口径：反汇编用 `01_tools/smali/baksmali 2.5.2`（`baksmali list classes` 取类清单、`baksmali d` 全量反汇编）；类集合差用 `comm`；签名指纹用 `apksigner --print-certs`；manifest 用 `aapt2 dump xmltree`。

## ⑤ 组包输入清单（给 P3）

| 项 | 值 / 说明 |
|---|---|
| 底包 | `02_apks/旷野征途/旷野征途_2.4_taptap_official.apk`（sha256 `8d677670…e484e`，194,003,543 B） |
| 需替换的 dex ① | **`classes6.dex`** ← 打补丁 #1、#2（`TapTapSdkInitializer`、`GatekeeperRepository`）；重汇编源 = `13_scratch/kyzt24_java/dis24_c6`（baksmali 产物，9,479 类） |
| 需替换的 dex ② | **`classes7.dex`** ← 打补丁 #3（`DefaultTapLoginFragmentProvider`）+ #4（`TapSignKit`，若采纳）；重汇编源 = `13_scratch/kyzt24_java/dis24_c7`（9,539 类） |
| 保持原样 | classes.dex / classes2 / classes3 / classes4 / classes5 / classes8 **逐字节不动**（含 `com.xf.kyzt` 的 22 个 R 类） |
| 框架 dex | 星光菜单 + SPI（`com.starry.magic.games.kyzt.GameHookImpl`）→ 新名 **`classes9.dex`** |
| killer dex（可选/回退） | `04_scripts/旷野征途/assets/kyzt_killer_22.dex`（sha256 `ac77dfc3…150d`，13,692 B）**原样可用**，→ `classes10.dex`；无需重编（证书/md5 与 2.4 一致、依赖仅自身 `$a..$l` + `sun.misc.Unsafe` + `android.app.Application`） |
| manifest | **不需改**（无 `android:name`；`appComponentFactory` 保持官方 `CoreComponentFactory`；`TapTapLoginActivity` 已声明） |
| so | 本侧不涉及；`libstarry_magic.so` 仍按 native 侧 2.4 构建产物 |
| 门禁预期 | 查12：`classes6.dex` 与底包不同 ⇒ PASS；dex 序号须连续 1..10 无缺口 |
| 回滚 | 官方包原样保留；只替换两个 dex 即可回滚到官方 TapSDK 行为 |

## 附：可复现命令（挑选）

    # 类清单（权威，来自 class_defs）
    01_tools/smali/baksmali list classes 13_scratch/kyzt24_java/apk_dex/classes6.dex | grep -c '^Lcom/taptap/'
    # 反汇编（本工作区产物）
    01_tools/smali/baksmali d 13_scratch/kyzt24_java/apk_dex/classes6.dex -o 13_scratch/kyzt24_java/dis24_c6 --jobs 6
    # 锚点切片
    sed -n '718,732p' 13_scratch/kyzt24_java/dis24_c6/com/taptap/sdk/initializer/TapTapSdkInitializer.smali
    sed -n '487,530p' 13_scratch/kyzt24_java/dis24_c6/com/taptap/sdk/initializer/repository/GatekeeperRepository.smali
    sed -n '94,250p'  13_scratch/kyzt24_java/dis24_c7/com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali
    # 签名指纹
    ~/Library/Android/sdk/build-tools/36.0.0/apksigner verify --print-certs "02_apks/旷野征途/旷野征途_2.4_taptap_official.apk"


---

# P3a 补丁落地与校验（2026-11-16 13:40，授权：lead「改包法 dex 补丁落地（2.4）」）

> 只做 smali 往返 + 三处补丁 + 机器校验；**未组包、未签名、未装机、未改 04_scripts/**。
> 全部只动 smali 工作副本（13_scratch/kyzt24_java/work_c6、work_c7），原始反汇编目录 dis24_c6/dis24_c7 保持只读作基线。

## 1. 产物

| dex | 路径 | 字节数 | sha256 |
|---|---|---|---|
| 底包 classes6.dex（原始） | 13_scratch/kyzt24_java/apk_dex/classes6.dex | 9,343,240 | a5647aa3793bdd48f41d316941a7777e09628d65fab7532a3dd9c5575d18b310 |
| **补丁 classes6** | 13_scratch/kyzt24_java/out/classes6_patched.dex | **9,448,484** | **802a9f69b35b4e29fda5ae030ade26666142be6236558a9d83a5171ff7fbcd3d** |
| 底包 classes7.dex（原始） | 13_scratch/kyzt24_java/apk_dex/classes7.dex | 11,283,180 | 34fa0684be18beb3f3a82e188dcd6ba1bf5d693b881fb760ff8653f3b2447e06 |
| **补丁 classes7** | 13_scratch/kyzt24_java/out/classes7_patched.dex | **11,388,272** | **ade872beea1c847fb04e279b26af7a804fda0590895971f0a1a0b482e7440d09** |

补丁脚本：13_scratch/kyzt24_java/patch24.py（幂等，二次运行打印「已打过，跳过」）。
汇编命令：01_tools/smali/smali assemble -a 24 <work_dir> -o out/classesN_patched.dex（**-a 24 必需**，见 §4-1）。

## 2. 三处补丁：原方法全文 → 改后全文

### #2 GatekeeperRepository.showErrorToast(TapNetworkException)V（classes6.dex，原行 487-528）

原全文（.registers 5 + 12 条指令 + .line 296/299/297）：

    .method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
        .registers 5

        .line 296
        invoke-virtual {p1}, ...TapNetworkException;->getErr()...$Err;
        move-result-object v0
        invoke-virtual {v0}, ...$Err;->getMsg()Ljava/lang/String;
        move-result-object v0
        invoke-static {v0}, ...SysExtKt;->showToast(Ljava/lang/String;)Lkotlinx/coroutines/Job;

        .line 299
        invoke-virtual {p1}, ...->getErr()...$Err;
        move-result-object p1
        invoke-virtual {p1}, ...$Err;->getDescription()Ljava/lang/String;
        move-result-object p1
        if-nez p1, :cond_17
        const-string p1, "当前应用初始化信息错误, 请在 TapTap 开发者中心检查[当前应用包名]与[调用初始化接口设置的 clientId 、clientToken]是否匹配"
        :cond_17
        const/4 v0, 0x4
        const-string v1, "TapInitializer"
        const/4 v2, 0x0

        .line 297
        invoke-static {v1, p1, v2, v0, v2}, ...TapLogger;->loge$default(...)V
        return-void
    .end method

改后全文：

    .method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
        .registers 5

        return-void
    .end method

字节码实证（dexdump -d out/classes6_patched.dex）：

    87a790: |[87a790] com.taptap.sdk.initializer.repository.GatekeeperRepository.showErrorToast:(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
    87a7a0: 0e00  |0000: return-void
            catches       : (none)

### #3 DefaultTapLoginFragmentProvider.provide(LoginRequest)Fragment（classes7.dex，原行 94，锚点行 184）

原（锚点上下文）：

        invoke-virtual {v2}, ...LoginTypes;->ordinal()I
        move-result v2
        aget v2, v3, v2          # 行 184，全类唯一（count=1）
        const/4 v3, 0x1
        if-eq v2, v3, :cond_6f  # 1 -> ClientLoginFragment
        const/4 v3, 0x2
        ...

改后（仅插入 1 条指令，其余一行未动）：

        aget v2, v3, v2

        const/4 v2, 0x3

        const/4 v3, 0x1
        if-eq v2, v3, :cond_70

字节码实证（补丁 dex -d，与底包逐条对齐）：

    原  classes7.dex    : 0048 aget v2,v3,v2 | 004a const/4 v3,#1 | 004b if-eq v2,v3,006f | 004d const/4 v3,#2 | 004e if-eq v2,v3,0064 | 0050 const/4 v3,#3 | 0051 if-ne v2,v3,005e
    改  classes7_patched: 0048 aget v2,v3,v2 | 004a const/4 v2,#3 | 004b const/4 v3,#1 | 004c if-eq v2,v3,0070 | 004e const/4 v3,#2 | 004f if-eq v2,v3,0065 | 0051 const/4 v3,#3 | 0052 if-ne v2,v3,005f

=> v2 恒为 3：if-eq 1 / if-eq 2 均不跳、if-ne 3 不跳 => 直落 0054 sget-object WebLoginFragment（分支目标整体后移 1 个 code unit，语义未变）。

### #4 TapSignKit.getSign(Context)String（classes7.dex，原行 59-145）

**先核结构（按 lead 要求，不照抄 2.2）**：返回值 Ljava/lang/String;；.registers 5；**1 个 try 块**（.catch Ljava/lang/Exception; {:try_start_6 .. :try_end_40} :catch_41）；无 .local/.param 依赖；方法体内标签 :cond_38/:goto_39/:cond_3c/:catch_41 只在本方法内使用，**无外部跳入**（全类仅此 1 处 getSign 定义，类内其余方法不引用这些标签）。

原全文（42 条指令，摘关键行；完整见 dis24_c7/com/taptap/sdk/kit/internal/utils/TapSignKit.smali:59-145）：

    .method public final getSign(Landroid/content/Context;)Ljava/lang/String;
        .registers 5
        const-string v0, "context"
        invoke-static {p1, v0}, Lkotlin/jvm/internal/Intrinsics;->checkNotNullParameter(Ljava/lang/Object;Ljava/lang/String;)V
        const/4 v0, 0x0
        :try_start_6
        invoke-virtual {p1}, Landroid/content/Context;->getPackageManager()Landroid/content/pm/PackageManager;
        ...
        invoke-virtual {v1, p1, v2}, Landroid/content/pm/PackageManager;->getPackageInfo(Ljava/lang/String;I)Landroid/content/pm/PackageInfo;   # 0x40 = GET_SIGNATURES
        iget-object p1, p1, Landroid/content/pm/PackageInfo;->signatures:[Landroid/content/pm/Signature;
        ...
        invoke-virtual {p1}, Landroid/content/pm/Signature;->toByteArray()[B
        ...
        invoke-static {p1}, Lcom/taptap/sdk/kit/internal/utils/CommonUtils;->getMD5([B)Ljava/lang/String;
        :try_end_40
        .catch Ljava/lang/Exception; {:try_start_6 .. :try_end_40} :catch_41
        return-object p1
        :catch_41
        invoke-virtual {p1}, Ljava/lang/Exception;->printStackTrace()V
        return-object v0
    .end method

改后全文（整段替换；整段替换后不再需要 try 块，故 .catch 与 :try_* 一并移除，避免悬空标签）：

    .method public final getSign(Landroid/content/Context;)Ljava/lang/String;
        .registers 3

        const-string v0, "360c78f563dd471fa6df02cc68045a31"

        return-object v0
    .end method

寄存器说明：.registers 3 + 2 个 ins（this + Context）=> p0=v1、p1=v2，**v0 是本地寄存器**，不覆盖入参。（首版用 .registers 2，虽正确但常量写进 p0、baksmali 回显成 const-string p0，易误读，已改 .registers 3。）
字节码实证：

    6054d4: 1a00 341f   |0000: const-string v0, "360c78f563dd471fa6df02cc68045a31" // string@1f34
    6054d8: 1100        |0002: return-object v0
            catches       : (none)

## 3. 机器校验回显

### 3-1 结构完整性（dexdump）

    dexdump -c out/classes6_patched.dex  -> Checksum verified
    dexdump -c out/classes7_patched.dex  -> Checksum verified
    dexdump -f ... -> 两包均可解析，DEX version '037'（与底包一致）

| 计数项 | classes6 原 -> 改 | classes7 原 -> 改 |
|---|---|---|
| file_size | 9,343,240 -> **9,448,484** | 11,283,180 -> **11,388,272** |
| string_ids | 55,137 -> 55,137 | 72,520 -> **72,518**（-2） |
| type_ids | 11,398 -> 11,398 | 11,252 -> 11,252 |
| proto_ids | 14,637 -> 14,637 | 14,696 -> 14,696 |
| field_ids | 46,602 -> 46,602 | 65,534 -> 65,534 |
| method_ids | 65,523 -> **65,522**（-1） | 65,489 -> **65,488**（-1） |
| class_defs | 9,479 -> 9,479 | 9,539 -> 9,539 |

差异来源（均为**减少**，因补丁体不再引用）：
- c6 -1 method：showErrorToast 原体里的 TapLogger->loge$default 在 c6 内不再被其他类引用。
- c7 -2 string / -1 method：getSign 原体独有的 "context.packageManager"、"pm.getPackageInfo(packag…geManager.GET_SIGNATURES)" 两串与他处不引用的 CommonUtils->getMD5 引用消失。
- 类清单：baksmali list classes 排序后 **diff = 0 行**（c6 9,479 / c7 9,539 全等）；仅 class_defs **顺序**不同（dexlib2 写入顺序，无语义影响）。

### 3-2 往返反汇编比对（唯一强校验）

baksmali d out/classesN_patched.dex -> 与基线 dis24_cN 逐文件 diff -rq：

| dex | 差异文件总数 | 目标类（预期） | 仅「静态字段默认值被省写」 | try 块合并 | **其它** |
|---|---|---|---|---|---|
| classes6 | 26 | 1（GatekeeperRepository） | 24 | 1（smartdigimkt/o4/a） | **0** |
| classes7 | 35 | 2（DefaultTapLoginFragmentProvider、TapSignKit） | 30 | 3（tuoxin/_a0、tuoxin/_ef、ubix/ssp/ad/k8） | **0** |

两类非目标差异的性质（均已机检）：

1. **静态字段默认值被省写**（52 文件）：diff 只有 .field … = false / = null / = 0x0 / = 0x0L 被去掉。实测取值集合 = {false×85, null×19, 0x0×11, 0x0L×9} —— **全部是类型默认值**，缺省时字段仍默认初始化为同值 => 语义等价。
2. **try 块合并**（4 文件）：连续且同一 handler 的多个 try 区间合并成一个（例：{:try_start_1..try_end_30} + {:try_start_30..try_end_46} -> {:try_start_1..try_end_46}，边界紧邻、无指令落在间隙）。机检：剥离 :try_start_*/:try_end_*/.catch* 行后逐字节 diff，tuoxin/_ef、ubix/ssp/ad/k8 = **0 行差异**；smartdigimkt/o4/a、tuoxin/_a0 的残余差异**全部仍是 .catchall 行**（首轮过滤器未含 catchall）=> 指令流完全一致，仅 handler 表合并。

**先例支持**：2.2 同样是 baksmali->smali 全量往返，实测 dis22_official vs dis22_patched 差异文件 = **27 个**（而只改 3 个方法），且设备三次冷启通过 => 这两类往返痕迹是既有手法固有产物，不是本次新增风险。

### 3-3 目标方法逐字比对

- c6 GatekeeperRepository：全类 diff 仅 showErrorToast 方法体（原 12 条指令 -> return-void），其余 20 个方法零差异。
- c7 DefaultTapLoginFragmentProvider：全类 diff 仅 provide 内新增的 const/4 v2, 0x3（其余方法零差异）。
- c7 TapSignKit：全类 diff 仅 getSign 方法体（42 条 -> 2 条），其余方法零差异。

## 4. 过程中的两个坑（供 P3b/P4 复用避雷）

1. **smali assemble 默认 -a 15 => 产出 DEX 035**，会把底包 037 降级。首版即踩：dexdump -f 显示 DEX version '035'。**必须 -a 24**（-> 037）。2.2 当时产 035 也能跑，但 2.4 dex 是 037 产物，不赌，一律对齐。
2. **.registers 2 + const-string v0 会被回显成 p0**：.registers 是「总寄存器数」，含 2 个 ins 时全部寄存器都是参数，v0 == p0。语义正确但证据难读，故 getSign 用 .registers 3。

## 5. 未做 / 交给下游

- **未组包、未签名、未装机**（按授权）。dex 序号编排、manifest、so 由 P3b 处理；本侧只交付两个 dex 与校验。
- **上限提示**：c6 method_ids = 65,522 / 65,536（余 14）、c7 field_ids = 65,534 / 65,536（余 2）。本侧补丁是**减少**引用，安全；但 P3b 若往这两个 dex 里**加**类或方法引用会直接撞上限 => 新类只应进新 dex（classes9/classes10）。
- **仍未证**：只打 getSign 是否足以过 1001（native 侧未证，见 ④-1）；killer 在 P3b 手上作并行保险。
