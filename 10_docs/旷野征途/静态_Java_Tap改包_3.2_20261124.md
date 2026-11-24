# 旷野征途 3.2 / vc32 · Java/TapTap 侧静态分析（只读）

> 生成：2026-11-24　作者：static-java（阶段1b：Java/TapTap 静态分析）
> 范围：**纯静态**。未上机、未组包、未改 dex 字节、未改 `04_scripts/**`、未改框架 Java、未 commit。
> 底包只读：`02_apks/旷野征途/旷野征途_3.2_taptap_official.apk`（198,941,707 B，sha256 `6318a972a599cfa664c1db867b960fc1a5f9b878d9bad3c15b003d28da17df1d`）
> 工作区（本轮全部产物，可删）：`13_scratch/kyzt32_java/`（dex 副本 `../kyzt32_base/` + 反汇编 `dis1..dis7` + 类清单 `cls/`）
> 操作模板：`10_docs/旷野征途/静态_Java_Tap改包_2.4_20261116.md`

## ① 结论

1. **3.2 是 7 dex 包（classes.dex … classes7.dex），全部 DEX 037**；**TapSDK 4.10.9 的 2578 个 `com/taptap/` 类全部集中在 `classes6.dex` 一份里**，另 6 个 dex 命中 0 ⇒ **阶段3 只需替换 1 个官方 dex**（2.4 时代锚点分裂在 classes6/classes7，要换两个）。这是相对 2.4 最大的结构简化。
2. **2.4 的 4 个补丁锚点在 3.2 全部命中，方法体指令序列与 2.4 文档引文逐条一致**（`TapTapSdkInitializer` 行 718、`GatekeeperRepository` 行 487、`DefaultTapLoginFragmentProvider` 行 94（锚点 `aget` 在行 184）、`TapSignKit` 行 59）；行号、`.registers`、标签名、try 区间全部未变。**#1 依旧是死补丁**：全 7 dex 只有 2 处 invoke，都在 TapSDK 自身自调用链内，**没有任何外部调用者**。
3. **本轮最重要发现：3.2 存在一处 2.4 文档完全没覆盖的本地签名门闩** —— `com.xiefeng.game2d.PrivacyActivity.getVerifiedVersionCode(Context)J`（launcher Activity，`onCreate` 行 84 调用）：内嵌官方证书 **SHA-256**（base64 `Cjt4JZHHiVQ3RypHaKR1TmSH/SfL4Yh2l+vDjeQaBI8=`，解码 = `0a3b7825…41a048f`）→ `MessageDigest.isEqual` 比对 → 不匹配返回 `-1` → `if-gtz` 判负 → 弹「无法启动游戏 / 请从官方渠道获取完整安装包后再试。」→ 按钮/取消都 `finish()`。**重签包（CN=StarryMagic）在此必被拦，游戏进不到登录。** 这直接决定「能不能省掉签名伪装」（见 ⑤）。
4. **killer（B 轨）的静态前提在 3.2 仍全部成立**（application **无 `android:name`**、`appComponentFactory` 仍 `androidx.core.app.CoreComponentFactory`、`StarryInitProvider` 注入链与 2.2/2.4 同形），但 **killer dex 资产已不在工作区**：`04_scripts/旷野征途/assets/kyzt_killer_22.dex` 目录不存在，且 `.gitignore` 第 164 行排除 `*.dex` ⇒ **git 历史里也没有**，无法「原样复用」，只能重建或用补丁替代。
5. **硬约束：`classes6.dex` 的 `method_ids_size = 65536`（顶格，dex 上限）**，`field_ids 54924`、`string_ids 67009`、`type_ids 10509`、`proto_ids 15768`。本侧 4 个补丁都是**减少**引用（与 2.4 同向），但仍须在重汇编后复核计数——任何往该 dex「加类/加方法引用」的改动会直接撞上限。

## ② dex 布局（3.2 vs 2.4）

| # | 文件 | 字节 (3.2) | sha256 (3.2) | 类数 | taptap 类 | 2.4 对应字节 | 差异 |
|---|---|---|---|---|---|---|---|
| 1 | classes.dex | 9,303,736 | `cc1d0978efea2c05ce1a46ae6ddf6ab092f3ee59efbacbd8217421496d558040` | 8,333 | 0 | 9,210,068 | +93,668 |
| 2 | classes2.dex | 8,133,224 | `546d1d5085b29c018a11f0c27bf015ee30f3e5f03a98ea3d2ef08315e3e8e7ef` | 7,910 | 0 | 8,852,256 | −719,032 |
| 3 | classes3.dex | 7,962,256 | `a7ede4341a56a16f1cb58c9149a6452dd60b594ba0491f3576b8990da9f6a26e` | 7,697 | 0 | 8,602,228 | −639,972 |
| 4 | classes4.dex | 8,465,080 | `e4d26f6507b5fc298dd884b71ae0edac6cb02cd88495bf39ebf920d80e1f6b7c` | 8,053 | 0 | 8,510,712 | −45,632 |
| 5 | classes5.dex | 7,376,528 | `b4888be4b11c4a0447f38ff06dd3ac042cfc9933bd300498484e60a2df079ae6` | 9,658 | 0 | 7,376,684 | −156 |
| 6 | **classes6.dex** | **10,901,656** | **`06547707230a83aa3e033a8bb4c410f9f0e7a7adcf1e36d4c24f9b27744d230e`** | **8,753** | **2,578** | 9,343,240 | +1,558,416 |
| 7 | classes7.dex | 694,588 | `ea9dc8819872dba29f594028e1dcad24c221af4bc9b2f253fa1fe1c66b7c8f7b` | 674 | 0 | 11,283,180 | **−10,588,592** |
| — | classes8.dex | **不存在** | — | — | — | 868,008 | 3.2 只有 7 dex |

- 类总数：3.2 = **51,078**（2.4 = 63,035，−11,957）。3.2 的 `classes7.dex` 是 R8 溢出的碎 dex：`retrofit2/http` 25 + `retrofit2/converter` 4 + `retrofit2/internal` 1 类 + 一批单字母混淆类（`La;` … `Lzg;`），无任何 taptap 类。
- **TapSDK 分布**：`classes6.dex` 2,578；classes/2/3/4/5/7 = **0**。锚点归属全部落在 classes6：`TapTapSdkInitializer`(+8 内部类)、`GatekeeperRepository`(+3)、`DefaultTapLoginFragmentProvider`(+WhenMappings)、`TapSignKit`、`WebLoginFragment`(11)、`TapTapLoginActivity`(+fragmentProvider$1)。

TapSDK 二级包类数（3.2 / 2.4 全部相等）：

    update 295 / leaderboard 232 / compliance 227 / okhttp3 205 / login 195 / components 195
    db 190 / kit 187 / common 176 / profile 160 / retrofit2 122 / initializer 75 / cloudsave 57
    okio 44 / gid 36 / core 36 / base 35 / openlog 31 / startup 25 / themis 18 / flowlayout 14
    servicemanager 8 / initialize 7 / services 5 / relation 3

- **TapSDK 版本 = 4.10.9（未升级）**：`const-string "4.10.9"` 命中 10 处（`TapCloudSaveInitTask:389`、`CloudSaveTracker$loggerHelper$2:89`、`ComplianceTracker$loggerHelper$2:89`、`TapSdkCommonParamsHandler:148`、`TapSdkEnvHandler:161`、`BridgeCoreServiceImpl$openlog$2:79`、`CachedPredefinedProvider:504`、`TapOpenlogHelper$openlog$2:89`、`DefaultTrackEvent:165`、`LoginRequestBuilder:590`），全 dex 未见其它 4.x/5.x 版本串。
- `initializer` 包仍 **75 类**，且**无新增 Gatekeeper 类**（41 个 `*gatekeeper*` 命中全部是已知集合：`GateKeeper`/数据类/serializer、`GatekeeperManager`(+Builder/WhenMappings/内部协程)、`GatekeeperRepository`(+Companion/initialize$1/initialize$1$networkData$1)、`GatekeeperStorage`/`PrefsGatekeeperStorage`/`GatekeeperApiService`）。其它 dex 的 `gatekeeper` 命中仅 `androidx/appcompat/widget/DropDownListView$GateKeeperDrawable`（无关）。

## ③ 补丁面复核（逐个）

| # | dex | 类（描述符） | 方法 | 3.2 行号 | 3.2 指纹（关键指令） | 2.4 → 3.2 逐字 | 判定 |
|---|---|---|---|---|---|---|---|
| 1 | classes6 | `Lcom/taptap/sdk/initializer/TapTapSdkInitializer;` | `isGatekeeperLoadedSuccessfully()Z` | 718 | `.registers 2` + `invoke-virtual getGateKeepManager` + `invoke-virtual GatekeeperManager->isGatekeeperLoadedSuccessfully()Z` + `return v0` | 指令序列一致（2.4 引文未含注解，3.2 该处多 `@Deprecated(message="请改用…getInitState()…")`，**注解差异不可判**） | **仍在，但依旧是死补丁**；全 7 dex 仅 2 处 invoke，均在 TapSDK 自链内，**0 外部调用点** ⇒ 保留无害，不得作为修复依据 |
| 2 | classes6 | `Lcom/taptap/sdk/initializer/repository/GatekeeperRepository;` | `showErrorToast(TapNetworkException;)V`（private final） | 487 | `.registers 5` + `getErr→getMsg→SysExtKt.showToast` + `getErr→getDescription` + `if-nez :cond_18` + `const-string/jumbo "当前应用初始化信息错误…"` + `TapLogger.loge$default` + `return-void` | **指令序列与字符串内容一致**；差异仅 `:cond_17`→`:cond_18`、`const-string`→`const-string/jumbo`（＝2.4 文档已记录的同类「重编译产物」，方向相反） | 有效，只掐提示不改结果 |
| 3 | classes6 | `Lcom/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider;` | `provide(LoginRequest;)Landroid/app/Fragment;` | 94（`aget v2, v3, v2` = 184） | `sget-object WhenMappings` + `ordinal()` + **`aget v2, v3, v2`（全类唯一，count=1）** + `const/4 v3,0x1`/`if-eq :cond_6f`(Client) + `0x2`/`if-eq :cond_64`(CloudGame) + `0x3`/`if-ne :cond_5e`(Web) | 结构、标签、行号（184）与 2.4 引文**逐字一致** | 有效；插入点 `aget` 后加 `const/4 v2, 0x3` 语义不变（见 ⑥ 枚举映射） |
| 4 | classes6 | `Lcom/taptap/sdk/kit/internal/utils/TapSignKit;` | `getSign(Landroid/content/Context;)Ljava/lang/String;` | 59 | `.registers 5`；`:try_start_6` → `getPackageManager`/`"context.packageManager"` → `getApplicationContext().getPackageName()`/`"context.applicationContext.packageName"` → `const/16 v2,0x40` → `getPackageInfo` → `"pm.getPackageInfo(packag\\u2026geManager.GET_SIGNATURES)"` → `PackageInfo->signatures` → `firstOrNull` → `toByteArray` → `:cond_38/:goto_39/:cond_3c` → `CommonUtils.getMD5([B)` → `:try_end_40` / `.catch Exception :catch_41` → `return-object` | **逐字一致**：标签名（`:cond_38/:goto_39/:cond_3c/:catch_41`）、try 区间（`{:try_start_6 .. :try_end_40} :catch_41`）、42 条指令、连 `const/16 v2,0x40` 与两处 Kotlin 判空串都在 | 有效；**官方 MD5 常量须在 3.2 上重算（已重算，见下）** |

### ③-1 补丁 #4 的常量：3.2 底包实测（不是沿用 2.4）

```
BT=01_tools/android-sdk/build-tools/android-15
$BT/apksigner verify --print-certs 02_apks/旷野征途/旷野征途_3.2_taptap_official.apk
  Signer #1 certificate DN: O=xiefeng
  Signer #1 certificate SHA-256 digest: 0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f
  Signer #1 certificate SHA-1 digest:   9de90671cac0eea12092a81f9f4b0e237b9bf591
  Signer #1 certificate MD5 digest:     360c78f563dd471fa6df02cc68045a31
```

**计算方法与等价性**（3.2 内实证）：`TapSignKit.getSign` 取 `PackageInfo.signatures[0].toByteArray()`（＝证书 **DER**）交 `CommonUtils.getMD5([B)`；后者 `MessageDigest.getInstance("MD5").update(der)` → `getHashString` 逐字节拆两 nibble `Integer.toHexString`（小写、nibble 为 0 时输出 `"0"`，宽度天然正确）⇒ **输出 ≡ apksigner 的 "certificate MD5 digest"** = `360c78f563dd471fa6df02cc68045a31`（与 2.4 相同，因两版同钥）。
⇒ 补丁 #4 常量不变：`const-string v0, "360c78f563dd471fa6df02cc68045a31"` + `return-object v0`，`.registers 3`（2.4 已记：`.registers 2` 会把常量写进 p0，证据难读）。

### ③-2 #1「死补丁」调用点证据（全 7 dex）

    $ grep -rn 'isGatekeeperLoadedSuccessfully()Z' dis1..dis7 | grep invoke
    dis6/…/manager/GatekeeperManager.smali:555   invoke-virtual {v0}, …GatekeeperRepository;->isGatekeeperLoadedSuccessfully()Z
    dis6/…/initializer/TapTapSdkInitializer.smali:729 invoke-virtual {v0}, …GatekeeperManager;->isGatekeeperLoadedSuccessfully()Z
    # 其余命中只有 3 处方法定义 + 3 处 Kotlin metadata 字符串（d2 数组）

⇒ 只有「包装方法调下层方法」，**没有任何业务代码调用 `TapTapSdkInitializer.isGatekeeperLoadedSuccessfully()`**，与 2.2/2.4 结论相同。

### ③-3 逐字比对的方法学与局限（必读）

2.4 的底包与反汇编工作区**已从磁盘清理**（`13_scratch/kyzt24_java` 不存在、`02_apks/旷野征途/` 只剩 3.2），So **本轮无法做 2.4↔3.2 的机器 diff**。判据改为三条独立证据叠加：
1. 2.4 文档里**逐字抄录的方法全文**（#2、#4 是全文；#1、#3 是关键段）；
2. **行号一致性**（718 / 487 / 94+184 / 59 四处全部命中 2.4 记录值）；
3. **类集合与包级类数全等**（taptap 2578=2578、initializer 75=75、login 195=195 … 25 个二级包无一变化）。

结论强度：**「语义级逐字未变（指令序列一致）」可下**；**「文件级逐字节相同」不可下**（#2 的标签编号/常量池宽度确实不同）。

## ④ 本地签名/完整性校验点扫描（含本轮最重要的新发现）

### ④-1 【新发现】PrivacyActivity 的启动期签名门闩

| 项 | 值 |
|---|---|
| 类 | `Lcom/xiefeng/game2d/PrivacyActivity;`（`classes6.dex`，≈1,750 行；`AndroidManifest` 的 **launchable-activity**） |
| 方法 | `public static getVerifiedVersionCode(Landroid/content/Context;)J`（行 884） |
| 校验物 | 包名白名单 `"com.xf.kyzt"` → `getPackageInfo`（SDK≥28 用 `0x8000000` + `signingInfo.getApkContentsSigners()`，否则 `0x40` + `signatures`）→ 要求 **恰好 1 个签名** → `MessageDigest("SHA-256")` → `MessageDigest.isEqual(实际, OFFICIAL_SIGNER_SHA256)` |
| 期望值 | `<clinit>` 行 96：`const-string "Cjt4JZHHiVQ3RypHaKR1TmSH/SfL4Yh2l+vDjeQaBI8="` → Base64.decode。**已解码验证 = `0a3b782591c7895437472a4768a4754e6487fd27cbe1887697ebc38de41a048f`（32 B，与底包证书 SHA-256 逐字节相同）** |
| 后果 | 匹配 → 返回 `getLongVersionCode()`；不匹配/异常/包名不符 → 返回 `-1` |
| 消费点 | `onCreate` 行 84：`invoke-static getVerifiedVersionCode` → `cmp-long` → `if-gtz` ⇒ **≤0 时**弹 `AlertDialog`「无法启动游戏」/「请从官方渠道获取完整安装包后再试。」，正/负按钮与 `OnCancelListener` 全部 `finish()`（lambda$onCreate$1/$2） |
| 影响 | **任何重签包（`CN=StarryMagic…`）启动即卡在该对话框，进不到隐私页/Unity/登录**。这是「改包法」在 3.2 的前置拦路石 |

判定口径（诚实版）：**3.2 存在该门闩 = 实测事实**；**它是否 2.4 就有 = 未证**（无 2.4 dex）。两条旁证都不足以定论：① 2.4 成品（Starry 证书）确实能启动 ⇒ 要么门闩当时不存在，要么当时靠 killer 覆盖了 `getPackageManager` 读数；② 2.4 文档的等价扫描（见 ④-3）有方法缺陷，命中 0 属假阴性，不能当「2.4 没有」。

### ④-2 全 dex 扫描结果

| 扫描 | 命令要点 | 结果 |
|---|---|---|
| 读 `PackageInfo->signatures` 的类 | `grep -rln 'PackageInfo;->signatures' dis1..dis7` | **43 个文件**：androidx 3、广告/归因 SDK 38（cj/mobile、com.byazt、com.anythink、com.kwad、com.huawei、com.jd.ad、com.smartdigimkt、ms/bz/bd… 全是各家 SDK 自校验/oaid）、`com/taptap/…/TapSignKit`、`com/xiefeng/game2d/PrivacyActivity` |
| TapSDK 侧唯一签名读取者 | 同上 + `grep -rln 'PackageInfo;->signatures' dis6/com/taptap` | **仍只有 `TapSignKit`**（与 2.4 一致） |
| 同时含 `getPackageInfo` 与包名字符串的类 | `comm -12`（`getPackageInfo` 275 个文件 × `com/xf/kyzt` 22 个文件） | 斜杠形式 **交集 0**；但点号形式 `"com.xf.kyzt"` 命中 `PrivacyActivity`（上文）⇒ **「0 个」是假阴性** |
| `MessageDigest.isEqual` | `grep -rn 'MessageDigest;->isEqual'` | 4 处：`dis3/com/byazt/ve/{f,qu,s}`（穿山甲自家 SDK 校验，与包证书无关）+ **`PrivacyActivity:1002`（唯一针对本包证书的）** |
| 官方证书常量扩散 | `grep -rn 'Cjt4JZHHiVQ3RypHaKR1TmSH' / '0a3b7825'` | **仅 PrivacyActivity 一条**，无第二处副本 |
| 新 Gatekeeper 类 | 41 个 `*gatekeeper*` 类名逐条核对 | 无新增（见 ②） |
| `com/taptap/sdk/initializer` 类集合 | 75 类，子包 api 25 / data 21 / manager 10 / repository 8 / service 1 / 其余 10 | 与 2.4 同量同形（**无 2.4 清单可做集合差，故「无新增」只到「包级计数一致」强度**） |
| 游戏自有 Java | `com/xf/kyzt/**` = 22 类（`R`/`R$*`/`BuildConfig`）+ `com/xiefeng/**` = 31 类（`debuglogexport` 2 + `game2d` 29：PrivacyActivity + 9 lambda + 2 内部类、TosinAdBridge 全家） | 2.4 文档只扫了 `com.xf.kyzt`，**`com.xiefeng.game2d` 整个包是模板漏项** |
| `TapTapKitInitProvider` | 读方法表（`onCreate`/query/insert/update/delete/getType） | 无 `getPackageInfo`/`Signature` 命中，provider 不参与签名判定 |

### ④-3 2.4 文档那条结论为什么必须撤回

2.4 文档 ③-6 写「同时含 `getPackageInfo` 与包名字符串 `com/xf/kyzt` 的类 = 0 个 ⇒ 无『包名+签名』本地自校验类」。**用斜杠形式 `com/xf/kyzt` 搜 Java 代码永远搜不到包名字符串**（dex 里 Java 的包名常量必然带点号）。本轮用同一斜杠法扫 3.2 同样得 0，而人工检索点号形式立刻命中 `PrivacyActivity` ⇒ **该结论在 2.4 上也是不可靠的**，不能作为「3.2 新增」或「2.4 没有」的依据。

## ⑤ killer（B 轨签名伪装）复用判定

| 条件 | 3.2 实测 | 判定 |
|---|---|---|
| manifest `<application>` 有无 `android:name` | **无**（该节点 511–522 行只有 theme/label/icon/allowBackup/hardwareAccelerated/largeHeap/supportsRtl/extractNativeLibs/usesCleartextTraffic/appComponentFactory/enableOnBackInvokedCallback；3 处 `android:name` 全是 `meta-data` 的 unity.* 键） | 与 2.4 同 ⇒ 不需要挂 Application 子类，**「父类必须是 android.app.Application」这条约束本轮不触发** |
| `appComponentFactory` | `androidx.core.app.CoreComponentFactory`（行 521） | 与 2.4 同（未被换过），`CoreComponentFactory.instantiateApplication` 走反射 newInstance，不约束具体类 |
| `StarryInitProvider` 注入是否仍成立 | 注入点在 manifest `<provider>`，与 application 有无 `android:name` 无关；框架源码 `mod菜单/starry-mod-menu/app/src/main/java/com/starry/magic/StarryInitProvider.java:57-79`：`onCreate` → 取 `getApplicationContext()` → `SignatureKillerBootstrap.install(application)`（`sign`/SignatureKillerBootstrap.java:33-38 只做 `Class.forName("bin.mt.signature.KillerApplication193")` 触发 `<clinit>`） | **链成立**，且不依赖被替换的 dex 序号（新 dex 名 = 下一个空位） |
| killer dex 资产 | `04_scripts/旷野征途/assets/` **目录不存在**；`git check-ignore -v` 命中 `.gitignore:164:*.dex` ⇒ 2.4 用的 `kyzt_killer_22.dex`（13,692 B，sha256 `ac77dfc3…2150d`）**既不在工作区也不在 git** | **不能「原样复用」**：须重建（MT ApkSignatureKillerEx 重新导出）或改走补丁路线 |

**B 轨还差一环证据（未证）**：2.2/2.4 只证明了 killer 让 **TapSDK 的 `getSign`** 读到官方 DER；**没有任何证据表明它能覆盖 `Activity.getPackageManager()`→`signingInfo.getApkContentsSigners()`（API≥28 路径）**。3.2 的 `PrivacyActivity` 恰好走 `signingInfo` 分支 ⇒ **不能假设 killer 能过这道门**。

## ⑥ 登录路由现状（网页登录 SPI 是否还能用）

| 项 | 3.2 实测 | 结论 |
|---|---|---|
| `TapTapLoginActivity` | `classes6.dex`，515 行；`<init>` 行 103–114 建 `new DefaultTapLoginFragmentProvider` + `fragmentProvider$1` 回调 → `iput fragmentProvider` | 与 2.4 同形 |
| `provide()` 调用点 | 2 处：行 184（`replaceContentByLoginType`，行 178 起）与行 468 | 与 2.4 记录的两个 site 一致 |
| `TapLoginFragmentProvider` 实现类 | 全 7 dex **只有 1 个**：`DefaultTapLoginFragmentProvider` | 注入点仍唯一，补丁 #3 不需换点 |
| 分支映射 | `WhenMappings.<clinit>`：`CLIENT→1`、`CLOUD_GAME→2`、`WEB→3`；`LoginTypes` 字段定义序 CLIENT/WEB/CLOUD_GAME（ordinal 顺序无关，映射按 `ordinal()` 动态写表） | **`const/4 v2,0x3` 仍精确指向 `WebLoginFragment`**，补丁 #3 语义不变 |
| 新方法 | `TapTapLoginActivity.fallbackToWeb(LoginRequest;)V`（行 303） | 只增加 Web 兜底入口，不改 `provide` 结构（未证 2.4 是否已有，不影响补丁有效性） |
| `WebLoginFragment` | 11 个类仍在（`WebLoginFragment`/`$Companion`/`$WhenMappings`/2 lambda/内部协程…） | 网页登录 UI 链完整 |
| 框架 SPI（`Games/kyzt/GameHookImpl`） | 框架侧 Java 未改、`TapLoginForceWeb` 仍在（`StarryApp.java:109-119` `onCreated(a)`） | **SPI 仍可用**；但 SPI 解决的是「强制走网页」，**不能替代补丁 #3 之外的门闩问题**，也绕不过 ④-1 的启动门闩 |

## ⑦ 阶段3 必改清单（可直接施工）

| 序 | 目标 dex | 类 | 方法 | 改法 | 依据 |
|---|---|---|---|---|---|
| 1 | **classes6.dex** | `com.taptap.sdk.initializer.repository.GatekeeperRepository` | `showErrorToast(TapNetworkException;)V` | 整段替换为 `.registers 5` + `return-void` | ③ #2 |
| 2 | **classes6.dex** | `com.taptap.sdk.login.impl.ui.fragment.DefaultTapLoginFragmentProvider` | `provide(LoginRequest;)Landroid/app/Fragment;` | 在行 184 `aget v2, v3, v2` 之后插 `const/4 v2, 0x3`（分支目标整体后移） | ③ #3、⑥ |
| 3 | **classes6.dex** | `com.taptap.sdk.kit.internal.utils.TapSignKit` | `getSign(Context;)Ljava/lang/String;` | 整段替换为 `.registers 3` + `const-string v0,"360c78f563dd471fa6df02cc68045a31"` + `return-object v0`（原 try 块与 `.catch` 一并删，避免悬空标签） | ③ #4、③-1 |
| 4 | **classes6.dex** | **`com.xiefeng.game2d.PrivacyActivity`** | 二选一：**(a)** `getVerifiedVersionCode(Context;)J` 恒返回合法 versionCode（`.registers 2` + `const-wide/16 v0,0x20` + `return-wide v0`）；**(b)** `<clinit>` 的 base64 常量改成**我方签名证书**的 SHA-256（证书 `CN=StarryMagic…` = `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d`） | **(a) 与签名无关、最稳；(b) 保留原语义但要跟签名绑定** | ④-1 |
| 5（可选） | classes6.dex | `com.taptap.sdk.initializer.TapTapSdkInitializer` | `isGatekeeperLoadedSuccessfully()Z` | 死补丁，**建议不打包**（省 3 条指令，且避免误以为它是门闩） | ③ #1、③-2 |
| 6（可选） | 新 **classes8.dex** | 星光框架 + `GameHookImpl` SPI | — | 3.2 只有 7 dex ⇒ 框架 dex 序号 = **8** | ② |
| 7（可选/B 轨） | 新 **classes9.dex** | MT 签名杀手 | — | **须先重建 dex**（原资产已丢）；且须先证它能覆盖 `signingInfo` 路径，否则仍要 #4 | ⑤ |

施工硬约束：
- **只替换 `classes6.dex`**；classes/2/3/4/5/7 逐字节不动（含 `com.xf.kyzt` 22 个 R 类）。2.4 的 `surgical_kyzt_2.4.sh` 硬编码 `classes6/classes7` 双 dex 与 `classes9/classes10` 序号，**不能直接复用**，需 3.2 变体（单官方 dex + 框架 classes8 + 杀手 classes9）。
- `classes6.dex` `method_ids = 65536`（顶格）：四个补丁都少引用，属安全方向；**重汇编后必须重跑 `dexdump -f` 复核 `method_ids_size ≤ 65536`**，且 `smali assemble -a 24`（2.4 已踩坑：默认 `-a 15` 会降到 DEX 035，底包是 037）。
- 装机门禁「查12」判据是「含 `GatekeeperRepository` 字面量的同名 dex 与底包不得逐字节相同」（`04_scripts/_common/pre_install_gate.sh:572 GATE = b'com/taptap/sdk/initializer/repository/GatekeeperRepository'`）⇒ 3.2 该字面量在 classes6.dex，改它即 PASS，不会因锚点合并而 SKIP。
- manifest **不需要动**（无 `android:name`；`TapTapLoginActivity` 与 `StarryInitProvider` 的 provider 位已够用）。

## ⑧ 未证项（不得当结论使用）

1. **2.4↔3.2 无机器 diff**：2.4 底包与 `13_scratch/kyzt24_java` 均已清理，逐字比对只能靠 2.4 文档引文 + 行号 + 包级类数（③-3）。要升级为字节级证据，需重新取得 2.4 官方包（`sha256 8d677670…e484e`）。
2. **`PrivacyActivity` 门闩是否 2.4 已存在**：未证（④-1 末尾）。这决定它是「3.2 新风险」还是「2.4 一直靠 killer 兜着」。
3. **killer 能否覆盖 `signingInfo.getApkContentsSigners()`**：无任何实测；2.2/2.4 只验过 `getSign`。也未证 killer 与 `PrivacyActivity` 的时序（provider 早于 Activity，理论上够早，但未跑）。
4. **native 侧**：`libtapsdkcore.so`（4,226,392 B，与 2.4 同尺寸）是否另有独立证书校验——本轮未查，属 native 子代理范围。同理 `libil2cpp.so`（66,268,112 B）内的 C# 反射是否调用 `isGatekeeperLoadedSuccessfully` 未查（Java 侧 0 调用已证）。
5. **只打 #4 是否足以过 1001**：仍未实测（静态只证 Java 侧签名唯一来源是 `getSign`）。
6. **`initializer` 包「无新增 Gatekeeper 类」**只到「包级计数 75=75 + 类名逐条比对无陌生项」强度，无 2.4 类清单可做集合差。
7. 本轮未跑 `post_install_precheck`/`pre_install_gate`（无产物、未装机）。

## 附：可复现命令与输入指纹

输入指纹：
- 底包 `旷野征途_3.2_taptap_official.apk` → `6318a972a599cfa664c1db867b960fc1a5f9b878d9bad3c15b003d28da17df1d`（198,941,707 B）
- `classes6.dex` → `06547707230a83aa3e033a8bb4c410f9f0e7a7adcf1e36d4c24f9b27744d230e`（10,901,656 B）
- 四个锚点 smali（baksmali 2.5.2 产物，可作为阶段3 的输入指纹）：
  - `DefaultTapLoginFragmentProvider.smali` 8,779 B / 246 行 / `3a42fe30532eadae94cc01290850db5eb720bdf912c848867795c12588c9b45d`
  - `TapSignKit.smali` 4,086 B / 145 行 / `42ad1f9c3397c5cde42304069b4a05a2fb02f21cbfae3ffaf46554dab1b32aa0`
  - `TapTapSdkInitializer.smali` 30,374 B / 750 行 / `c574cb3e6437a517e452b9641ef0523746949eacfb0989f065c030af1e53dcd8`
  - `GatekeeperRepository.smali` 29,422 B / 739 行 / `b646c9893f4cbecabdee60ea5b90df19841cc0ae8c39ca7b95fbffa9b9df3361`

命令（全部在本轮工作区实跑）：

    # 0) dex 落地与布局
    unzip -l "02_apks/旷野征途/旷野征途_3.2_taptap_official.apk" | grep -E 'classes.*\.dex|libtapsdk|libil2cpp'
    ls -l 13_scratch/kyzt32_base/*.dex
    BT=01_tools/android-sdk/build-tools/android-15
    $BT/dexdump.exe -f 13_scratch/kyzt32_base/classes6.dex | grep -E 'file_size|_ids_size|class_defs_size|DEX version'
    sha256sum 13_scratch/kyzt32_base/*.dex

    # 1) 类清单与包级计数（权威来自 class_defs）
    for f in 13_scratch/kyzt32_base/*.dex; do b=$(basename $f .dex); \
      01_tools/smali/baksmali list classes $f > 13_scratch/kyzt32_java/cls/$b.txt; done
    grep -c '^Lcom/taptap/' 13_scratch/kyzt32_java/cls/classes6.txt          # 2578
    grep '^Lcom/taptap/sdk/' 13_scratch/kyzt32_java/cls/classes6.txt | \
      sed -E 's#^Lcom/taptap/sdk/([^/]+)/.*#\1#' | sort | uniq -c | sort -rn

    # 2) 反汇编（7 dex 全量，单 dex ~5 s）
    01_tools/smali/baksmali d 13_scratch/kyzt32_base/classes6.dex -o 13_scratch/kyzt32_java/dis6 --jobs 6
    # 其余：classes.dex -> dis1, classesN.dex -> disN

    # 3) 锚点切片
    D=13_scratch/kyzt32_java/dis6
    sed -n '718,733p' $D/com/taptap/sdk/initializer/TapTapSdkInitializer.smali
    sed -n '487,529p' $D/com/taptap/sdk/initializer/repository/GatekeeperRepository.smali
    sed -n '94,246p'  $D/com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali
    sed -n '59,145p'  $D/com/taptap/sdk/kit/internal/utils/TapSignKit.smali
    sed -n '884,1058p' $D/com/xiefeng/game2d/PrivacyActivity.smali    # 本轮新发现的门闩

    # 4) 签名指纹（3.2 底包实测）
    $BT/apksigner.bat verify --print-certs "02_apks/旷野征途/旷野征途_3.2_taptap_official.apk"

    # 5) 校验点扫描
    grep -rn 'isGatekeeperLoadedSuccessfully()Z' 13_scratch/kyzt32_java/dis* | grep invoke
    grep -rln 'PackageInfo;->signatures' 13_scratch/kyzt32_java/dis*
    grep -rn 'MessageDigest;->isEqual'  13_scratch/kyzt32_java/dis*
    grep -rn 'TapSignKit;->getSign'     13_scratch/kyzt32_java/dis*

    # 6) manifest（沿用 lead 的 aapt2 产物，未重跑）
    grep -n -A12 'E: application' 13_scratch/kyzt32_base/manifest_aapt2_xmltree.txt


---

# 第二轮追加（2026-11-24）：补丁 #5 反推验证 + 签名读取面穷举 + 阶段3 施工清单

> lead 决策（本轮不再讨论）：① 不重建 killer dex；② 采纳 #5(a)（`getVerifiedVersionCode` 恒返 versionCode，不改 `<clinit>` 的 base64）；③ #1 不打；④ 阶段3 只替换 `classes6.dex`，框架/杀手 dex 序号待 native 结果定。
> 标注约定：**【实测】** = 由本轮反汇编/扫描直接读出；**【推断】** = 有证据链的推理；**【未证】** = 缺证据。

## ⑨ A. 补丁 #5 反推验证（`getVerifiedVersionCode`）

### ⑨-1 全部调用点（dex 级，全 7 dex）

```
$ grep -rn 'getVerifiedVersionCode' dis1 dis2 dis3 dis4 dis5 dis6 dis7
dis6/com/xiefeng/game2d/PrivacyActivity.smali:884  .method public static getVerifiedVersionCode(Landroid/content/Context;)J
dis6/com/xiefeng/game2d/PrivacyActivity.smali:1461 invoke-static {p0}, …->getVerifiedVersionCode(Landroid/content/Context;)J
```

| # | 调用者（dex / 类 / 方法） | 行 | 用途 | 证据 |
|---|---|---|---|---|
| 1 | `classes6` / `com.xiefeng.game2d.PrivacyActivity` / `protected onCreate(Landroid/os/Bundle;)V` | 1461（`onCreate` 内第 84 行源码位置） | 启动门闩：返回值只用于 `cmp-long` + `if-gtz` 判正负 | 【实测】全 dex 仅此 1 处 invoke |

**没有第二处调用者**【实测】：`grep -rn` 全 7 dex 只回 1 定义 + 1 调用；`acceptAndLaunch` 内 `signature|getVerifiedVersionCode` 命中 **0**；`launchUnity` 只 `new Intent(Context, X)→startActivity→finish`。

### ⑨-2 `onCreate` 分支反推（伪代码，逐指令对应）

```
onCreate(bundle b):                                     # 【实测】smali 行 1415-1732
  super.onCreate(b)
  if (!isTaskRoot() && action==ACTION_MAIN && hasCategory(CATEGORY_LAUNCHER)):
        finish(); return                                # 行 78-80：非根任务重复入口直接退（与签名无关）

  long vc = PrivacyActivity.getVerifiedVersionCode(this)  # ★行 84 / 全局行 1461
  cmp  = (vc <= 0) ? -1 : 0
  String sExit = "退出"                                  # 行 88 前
  if (vc <= 0):                                         # if-gtz ⇒ vc<=0 才进
        new AlertDialog.Builder(this)
          .setTitle("无法启动游戏")
          .setMessage("请从官方渠道获取完整安装包后再试。")
          .setPositiveButton("退出", → finish())          # 行 1491-1496：ExternalSyntheticLambda1 → lambda$onCreate$1
          .setOnCancelListener(→ finish())                # 行 1500-1505：ExternalSyntheticLambda2 → lambda$onCreate$2
          # 注意：该弹窗**没有** setNegativeButton（全类只有 3 处 setPositiveButton：行 191/1496/1674）
          .show()
        return                                          # ★ 到此为止，不进 Unity

  # ---- 正常路径（vc > 0）----
  try:
     privacyContent   = readDocument("Legal/PrivacyPolicy.txt")
     agreementContent = readDocument("Legal/UserProtectionAgreement.txt")
  catch IOException:
     dialog("协议内容无法读取","请重新安装完整游戏后再试。") → finish(); return   # 行 1664-1684（setPositiveButton 1674 + setOnCancelListener 1683，另一条独立失败分支，与本补丁无关）
  readOnly = getIntent().getBooleanExtra("legal_read_only", false)
  if (readOnly): document = ("agreement"==getStringExtra("legal_document")) ? 协议 : 政策
  if (!readOnly && getAcceptedPolicyVersion(this) == 0xf):   # POLICY_VERSION=15，来自 prefs/consent 文件
        launchUnity(); return                             # 已同意过 ⇒ 直接进 Unity
  buildPage(scroll)                                     # 显示隐私页；用户点同意 → acceptAndLaunch()
```

**结论 1【推断，证据强】**：令 `getVerifiedVersionCode` 恒返正数（32），`if-gtz` 必不跳，直接落到 `:cond_5b` 正常路径 ⇒ 弹窗与 `finish` 分支彻底不可达；后续只剩「读协议→（已同意则进 Unity 否则显示隐私页）」两条正常分支。

**结论 2【实测】**：**不存在隐藏的第二条消费链**——
- `onCreate` 内的 `iput`/`sput` 全集只有 `privacyContent / agreementContent / readOnly / document×2 / introScroll`【实测，逐行 grep】；
- 类内**没有 long 字段**（字段表 = 11 个 static final + 6 个实例字段，无一 long）⇒`vc` 无处落地；
- `cmp-long` 之后 `v1` 立即被复用覆盖 ⇒ 值本身丢弃；
- 全类**无网络引用**（`okhttp|HttpURLConnection|Ljava/net/URL|retrofit` 命中 0；唯一的 `Log.w` 在异常分支）⇒ **不参与任何服务端上报**【实测】。

**结论 3【推断】**：`OFFICIAL_PACKAGE = "com.xf.kyzt"` 是第二条弱校验（包名 `equals`），但我们不改包名 ⇒ 该分支恒通过；唯一会失败的是证书摘要比较。故 #5(a) 不需要动 `OFFICIAL_PACKAGE`。

### ⑨-3 第二处版本/签名交叉校验：无

| 检查 | 结果 |
|---|---|
| `OFFICIAL_SIGNER_SHA256` 引用点 | 3 行：字段声明(19)、`<clinit>` 写入(105)、`getVerifiedVersionCode` 读取(1000) ⇒ **读点只有 1 个**【实测】 |
| base64 常量是否被别的方法复用 | 否（`grep -rn 'Cjt4JZHHiVQ3RypHaKR1TmSH'` 全 dex 仅 1 命中；`grep -rn '0a3b7825'` 0 命中）【实测】 |
| 版本号与签名是否在别处交叉 | 类内无第二处 `MessageDigest`、无 `PackageManager.checkSignatures`；全包唯一含「常量比较 + isEqual」的游戏自有代码就是这一处【实测】 |
| 是否用 `versionCode` 做降级检测（用旧版覆盖） | 否：返回值不落地、不比较、不落 prefs【实测，见 ⑨-2 结论 2】 |

⇒ **#5(a) 的爆炸半径 = 1 个方法、1 个分支**，符合 lead 的「逐点 patch」取舍。

## ⑩ B. 全 dex 签名读取面穷举（决定「不注入 killer」是否安全）

扫描口径【实测】：7 dex 反汇编全量，12 个模式（`getPackageInfo` / `PackageInfo->signatures` / `signingInfo` / `getApkContentsSigners` / `getSigningCertificateHistory` / `hasMultipleSigners` / `Signature->toByteArray` / `toCharsetString` / `toHashString` / `MessageDigest->isEqual` / `PackageManager->checkSignatures` / `Arrays.equals([B[B)`），命中 **571 行 / 302 文件 / 44 个「读签名」类**；其中带签名 flag（`0x40`/`0x8000000`）的 `getPackageInfo` **48 处 / 43 类**。

### ⑩-1 按后果分组（这张表就是「不注入 killer 是否安全」的答案）

| 组 | 类（数量） | 读什么 | 校验失败后果 | 已覆盖 | 风险 |
|---|---|---|---|---|---|
| **G1 TapSDK** | `com.taptap.sdk.kit.internal.utils.TapSignKit`（1） | 本包证书 DER → MD5（`getSign`） | Tap 后端 `(包名,sign)` 判 `INVALID_CLIENT` ⇒ 初始化 1001，登录不可用 | **#4** | 中（登录功能） |
| **G2 游戏自身** | `com.xiefeng.game2d.PrivacyActivity`（1） | 本包证书 **SHA-256** vs 内嵌官方值 | 启动弹「无法启动游戏」→ finish | **#5** | 高（阻断启动） |
| **G3 androidx** | `PackageInfoCompat`(+Api28Impl)、`FontProvider`、`DefaultEmojiCompatConfig*Helper`(+_API28)（5） | **别的包**（字体/emoji provider 包）的证书 | 字体/emoji 回退默认实现，游戏不受影响；`isEqual` 比较对象是字体 provider 的证书，不是本包 | 不需要 | 无 |
| **G4 OAID/设备标识** | `cj.mobile._cc/_hc`、`cj.mobile.wm.adIdUtils.oaid.impl.OppoImpl`、`czhj.devicehelper.cnoaid.impl.p`、`gzuliyujiang.oaid.impl.OppoImpl`、`jiaads.advista.oaid.impl.OppoImpl`、`kwad…OppoDeviceIDHelper`（7） | **OPPO/heytap 等系统包**的证书（用于判断 OAID 服务是否可信） | 取不到 OAID ⇒ 回落其它设备指纹 | 不需要 | 无 |
| **G5 广告/归因 SDK 自采集** | `com.byazt`(6)、`com.anythink`(4)、`com.kwad`(4)、`com.smartdigimkt`(4)、`com.adgain`(2)、`com.alliance.ssp`(1)、`ms.bz.bd`(2)、`com.jd.ad`(1)、`com.ja.adx.qiming`(2)、`com.huanxiao`(1)、`com.huawei`(2)、`czhj.sdk.common.utils.DeviceUtils`(1) —— 共 30 | **本包**证书 → MD5/SHA1/SHA256，喂设备指纹、归因、广告上报 | **只计算、无期望值比较**：这 30 个类里 `MessageDigest.isEqual`/`Arrays.equals` 命中 **0**（唯一例外 `com.huawei` 的 hash 常量 `3517262215d8…`/`b92825c2…` 按命名与用途**推断**为 HMS 自家证书指纹【推断】，比对对象是华为自家包/归因，不是本包）；失败最坏是异常被 catch → 该项字段为空 | 不需要 | 低（不阻断游戏，只可能让个别广告/归因字段变化） |
| 合计 | 44 类 | — | — | — | — |

**一句话判定【推断】**：**Java 侧「本包签名」的硬门闩只有两处（G1 登录、G2 启动），已分别被 #4/#5 覆盖**；其余 42 类要么读别人的证书（G3/G4），要么只把本包签名当设备指纹素材（G5，无期望值比较）。⇒ **不注入 killer 在 Java 侧是安全的**（native 侧见 ⑪）。

### ⑩-2 44 个读取者类全名单（供阶段3 复核/回归时对照）

```
dis1  androidx/core/content/pm/PackageInfoCompat                 androidx/core/content/pm/PackageInfoCompat$Api28Impl
dis1  androidx/core/provider/FontProvider                        androidx/emoji2/text/DefaultEmojiCompatConfig$DefaultEmojiCompatConfigHelper(+_API28)
dis1  cj/mobile/_cc   cj/mobile/_hc   cj/mobile/wm/adIdUtils/oaid/impl/OppoImpl
dis1  com/byazt/kri/s   com/byazt/kx/c
dis2  com/adgain/sdk/a/b4   com/adgain/sdk/a/t8   com/alliance/ssp/ad/x/p
dis2  com/anythink/core/common/u/ab   com/anythink/core/common/u/l   com/anythink/odopt/a/a/o   com/anythink/odopt/a/a/q
dis2  com/byazt/az/hk   com/byazt/ib/np   com/byazt/jy/by
dis3  com/byazt/mz/u   com/czhj/devicehelper/cnoaid/impl/p   com/czhj/sdk/common/utils/DeviceUtils   com/github/gzuliyujiang/oaid/impl/OppoImpl
dis4  com/huanxiao/sdk/o/m0   com/huawei/hms/ads/uiengineloader/aa   com/huawei/openalliance/ad/utils/h
dis4  com/ja/adx/qiming/internal/bb   com/ja/adx/qiming/internal/p6   com/jd/ad/sdk/jad_an/jad_cp   com/jiaads/advista/oaid/impl/OppoImpl
dis5  com/kwad/sdk/core/e/a/j   com/kwad/sdk/liteapi/oaid/helpers/OppoDeviceIDHelper
dis5  com/kwad/sdk/liteapi/report/LiteSignatureUtils   com/kwad/sdk/utils/bo   com/kwad/sdk/utils/g
dis6  com/smartdigimkt/k/o   com/smartdigimkt/k/q   com/smartdigimkt/r4/c   com/smartdigimkt/r4/o
dis6  com/taptap/sdk/kit/internal/utils/TapSignKit            ★G1
dis6  com/xiefeng/game2d/PrivacyActivity                      ★G2
dis6  ms/bz/bd/c/Pgl/e1   ms/bz/bd/c/Pgl/k
```

扫描产物（可复用）：`13_scratch/kyzt32_java/sigscan.json`（571 行结构化命中：dex/类/方法/行/模式/邻近常量）+ 脚本 `sigscan.py / sigscan2.py / sigscan3.py`。

## ⑪ C. 残余风险交办（Java 侧看不到的校验面）

### ⑪-1 native（建议转 native 子代理）

扫描方法【实测】：从 APK 解出 `lib/arm64-v8a/*.so` 到 `13_scratch/kyzt32_java/libs/`，用 ASCII 串扫描（本机 Git Bash **无 `strings`**，改用 Node 按可打印字符成串）：

| so | 大小 (B) | 签名相关串命中 | 判断 |
|---|---|---|---|
| `libtapsdkcore.so` | 4,226,392 | `signature`/`certificate` 107 条**全部是 OpenSSL X.509/TLS 语义**（`set_default_verify_paths`、`CERTIFICATE REQUEST`、`verify_result`、`Certificate Sign`…）；`getPackageSignature`/`PackageInfo;->signatures`/`getApkContentsSigners`/`checkSignatures`/`com.xf.kyzt` **0 命中** | 【未证】未发现 APK 签名校验；**不等于不存在**（该 so 可能用非字符串形式注册 JNI / 混淆常量） |
| `libcloudsave_sdk.so` | 4,336,464 | 命中集与 `libtapsdkcore.so` **完全同形**（同一份 OpenSSL 被静态链入），107 条 | 同上，未见 APK 签名语义 |
| `libunity.so` | 25,197,712 | `Certificate verification`/`MBEDTLS_*`/AWS SigV4 —— mbedTLS 与 Unity 内部 | 与本包证书无关 |
| `libthemis.so` | 853,816 | 仅 `Ldalvik/annotation/Signature;` | 未见 |
| `libmain.so` | 6,736 | 0 | 未见 |
| `libil2cpp.so` | 66,268,112 | `Signature`=1 / `certificate`=1（ASCII 池很小，il2cpp 元数据是压缩/二进制） | **必须查**：C# 侧可能经 JNI 调 `getVerifiedVersionCode` / `isGatekeeperLoadedSuccessfully` |

给 native 子代理的具体检索项（按优先级）：
1. `libil2cpp.so` 的元数据字符串里搜 `getVerifiedVersionCode`、`PrivacyActivity`、`isGatekeeperLoadedSuccessfully`、`TapSignKit`、`getSign` —— 判定 C# 是否直接反射调这些 Java 方法（若调，则 #4/#5 补丁对 C# 路径同样生效，因为改的是 Java 方法体，不影响反射）。
2. `libtapsdkcore.so` 是否自带 `JNI_OnLoad` 注册的 `com/taptap/**` native 方法（`RegisterNatives` 表）；若 TapSDK 的初始化校验有 native 分支，Java 侧补丁会被绕过。
3. `libonlinebattle_sdk.so`(6,192,312)、`libcloudsave_sdk.so` 是否读 `/proc/self/maps`、`PackageInfo` 或 `certificate` 做完整性判定。

### ⑪-2 C# / IL2CPP 侧（暂无 dump，无法列类名）

`06_dumped/旷野征途/3.2/` 目前只有空目录 `out/`（无 `dump.cs`/`global-metadata.dat`）【实测】⇒ **本侧无法给出可疑类名**。dump 完成后的检索清单（建议直接执行）：
`rg -n 'getVerifiedVersionCode|PrivacyActivity|isGatekeeperLoadedSuccessfully|TapTapSdk|GetPackageInfo|getPackageInfo|Signature|KeyStore|versionCode' 06_dumped/旷野征途/3.2/dump.cs`（按 §5 切片读，勿整读）。

## ⑫ D. 阶段3 dex patch 清单（最终形态，可直接施工）

**唯一目标 dex：`classes6.dex`**（10,901,656 B，sha256 `06547707230a83aa3e033a8bb4c410f9f0e7a7adcf1e36d4c24f9b27744d230e`；`method_ids 65536` 顶格 / `field_ids 54924` / `string_ids 67009` / `type_ids 10509` / `proto_ids 15768`）。其余 6 个官方 dex 逐字节不动。

| # | 类（文件） | 方法 | 定位（baksmali 行） | 改动（指令级） | 引用净变化 |
|---|---|---|---|---|---|
| P1 | `com/taptap/sdk/initializer/repository/GatekeeperRepository.smali` | `showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V` | 487–528 | 整个方法体（`.registers 5` 之后到 `return-void`）压成 `return-void`，保留 `.registers 5` 与 `.line` 无关行 | 引用**净减少**（`SysExtKt.showToast`、`TapLogger.loge$default` 若他处不再引用则一并消失；具体条数以 `dexdump -f` 实测为准） |
| P2 | `com/taptap/sdk/login/impl/ui/fragment/DefaultTapLoginFragmentProvider.smali` | `provide(LoginRequest;)Landroid/app/Fragment;` | 插入点 = 第 **184** 行 `aget v2, v3, v2` 之后 | 只插 1 条 `const/4 v2, 0x3`；分支标签由 smali 自动重排 | 0 |
| P3 | `com/taptap/sdk/kit/internal/utils/TapSignKit.smali` | `getSign(Landroid/content/Context;)Ljava/lang/String;` | 59–145 | 整段替换（含删 `:try_start_6`/`:try_end_40`/`.catch`）为 `.registers 3` + `const-string v0, "360c78f563dd471fa6df02cc68045a31"` + `return-object v0` | 引用**净减少**（getPackageManager/getApplicationContext/getPackageName/getPackageInfo/firstOrNull/toByteArray/getMD5/printStackTrace 等）；**+1 新 string 常量** |
| P4 | `com/xiefeng/game2d/PrivacyActivity.smali` | `getVerifiedVersionCode(Landroid/content/Context;)J` | 884–1058 | 整段替换为 `.registers 3` + `const-wide/16 v0, 0x20` + `return-wide v0`（**0x20 = 32 = vc32**） | 引用**净减少**（getPackageManager/getPackageInfo/getApkContentsSigners/getLongVersionCode/MessageDigest.getInstance/digest/isEqual 等；`SHA-256`、`com.xf.kyzt` 等字符串仍被 <clinit>/其它方法引用，不减） |

成品 smali（逐字可用）：

```smali
# P1 —— GatekeeperRepository.showErrorToast
.method private final showErrorToast(Lcom/taptap/sdk/common/network/throwable/TapNetworkException;)V
    .registers 5

    return-void
.end method

# P2 —— DefaultTapLoginFragmentProvider.provide（仅在第 184 行 aget 之后插入这一行）
    aget v2, v3, v2
    const/4 v2, 0x3        # ← 新增：恒走 WEB

# P3 —— TapSignKit.getSign
.method public final getSign(Landroid/content/Context;)Ljava/lang/String;
    .registers 3

    const-string v0, "360c78f563dd471fa6df02cc68045a31"

    return-object v0
.end method

# P4 —— PrivacyActivity.getVerifiedVersionCode
.method public static getVerifiedVersionCode(Landroid/content/Context;)J
    .registers 3

    const-wide/16 v0, 0x20

    return-wide v0
.end method
```

寄存器/陷阱说明：
- **P4 必须用 `.registers 3`**：静态方法 1 个入参 ⇒ `.registers 2` 时 `p0 == v1`，而 `const-wide/16 v0` 是 **wide 写入（v0+v1）**，会覆盖入参（语义仍对，但会被审阅者当 bug）。`.registers 3` ⇒ `p0 = v2`，v0/v1 专用于返回值。
- **P3 必须用 `.registers 3`**（2.4 已记录同一坑）：`.registers 2` 会让 `const-string v0` 回显成 `p0`，证据难读。
- **P2 不需要改 `if-eq` 目标**：`const/4 v2,0x3` 后三个比较全部不跳，直落 `sget-object WebLoginFragment`；分支目标整体后移由汇编器处理。
- **不要动 `<clinit>` 的 base64**（lead 决策 ②）：保持「匹配官方证书」语义不变，避免被将来的点复用成错误语义。
- **method_ids 顶格**：P1–P4 全是**减少** method/field 引用（P3 新增 1 个 string、P4 不新增 string），不存在加类/加方法引用的操作 ⇒ 安全；但**重汇编后必须复核**（见验证）。

阶段3 施工与验证命令（口径同 2.4，dex 序号按 3.2 调整）：

```bash
BT=01_tools/android-sdk/build-tools/android-15
# 1) 反汇编 classes6.dex（只读底包副本）
01_tools/smali/baksmali d 13_scratch/kyzt32_base/classes6.dex -o <work> --jobs 6
# 2) 打 P1–P4（改 smali 副本，不动底包）
# 3) 汇编（-a 24 必需；默认 -a 15 会产出 DEX 035，与底包 037 不一致 —— 2.4 已踩坑）
01_tools/smali/smali assemble -a 24 <work> -o out/classes6_patched.dex
# 4) 结构校验（三条硬门）
$BT/dexdump.exe -f out/classes6_patched.dex | grep -E '_ids_size|class_defs_size|DEX version'   # method_ids<=65536, version 037
$BT/dexdump.exe -c out/classes6_patched.dex | head -2                                          # Checksum verified
01_tools/smali/baksmali list classes out/classes6_patched.dex | sort > /tmp/new.txt
sort 13_scratch/kyzt32_java/cls/classes6.txt > /tmp/base.txt; diff /tmp/base.txt /tmp/new.txt   # 类集合必须 0 差异（8753）
# 5) 目标方法逐条比对（4 处，diff 只应出现在这四个方法内）
01_tools/smali/baksmali d out/classes6_patched.dex -o <dis_new> --jobs 6
diff -r <dis_new> 13_scratch/kyzt32_java/dis6 | grep -v '^Only in' | head -50
```

组包侧（交给阶段3，不在本侧执行）：
- 只替换 `classes6.dex`；框架 dex 若加 → **`classes8.dex`**（3.2 只有 7 dex）；killer 不做 ⇒ 无 `classes9.dex`。
- 装机门禁「查12」：`pre_install_gate.sh:572 GATE = b'com/taptap/sdk/initializer/repository/GatekeeperRepository'`，3.2 该字面量在 `classes6.dex` ⇒ 改它即 PASS。
- 回滚：官方 `classes6.dex` 原样保留，单 dex 替换即回到官方行为。
- 与 2.4 的差异（必读）：2.4 施工脚本 `surgical_kyzt_2.4.sh` 硬编码 `classes6/classes7` 双官方 dex + `classes9/classes10`（框架/杀手）序号，**不能直接复用**，需 3.2 变体。

## ⑬ 第二轮未证项

1. **#5 是否「必须」**：3.2 存在门闩是实测；但「重签包在不打 #5 时一定被拦」是**推断**（依据：`MessageDigest.isEqual` 对 DER SHA-256 精确比较 + 提前返回 -1）。未上机验证（本轮禁上机）。
2. **2.4 是否已有同一门闩**：仍无 2.4 dex 可比（见 ③-3）。
3. **native 侧是否有独立证书校验**：`libtapsdkcore.so` 未发现相关字符串，但**字符串缺失 ≠ 不存在**（⑪-1）。
4. **C# 是否经 JNI 调 `getVerifiedVersionCode` / `isGatekeeperLoadedSuccessfully`**：需 `06_dumped/旷野征途/3.2/dump.cs`，目前**尚未 dump**。
5. **G5 广告 SDK「只计算不上报比较」**的结论是**模式判定**（44 类中仅 2 处含期望值比较），未逐个 30 类逐行读完；若某个 SDK 在服务端做签名白名单校验（客户端无感），本轮静态看不到。
6. **P3 的 MD5 常量在服务端是否还匹配客户端的其它算法**（如某些接口用 SHA-1）：本轮只覆盖 `getSign` 唯一调用点，其余保持原样。
