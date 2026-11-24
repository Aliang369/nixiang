# 暗影格斗3 1.45.3 星光版复刻 — 独立验收报告（verifier）

- 时间：2026-11-24 20:5x–21:1x（Windows 11 + Git Bash / MSYS，工作区 `E:/nixiang`）
- 角色：**独立验收员**。只读取证，**未装机、未 patch、未重签、未改任何源码/脚本/包**。
- 待验产物：`08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk`
  - 实测 size = 2,715,627,251 B；md5 = `0bc69378712b98a3dcb62ad85aec1684`；sha256 = `3e814c905e822bff62beddfb5efac32fe9f6f8d811bed573efa7076f049e5590`
  - 与任务书给出的值逐字节一致。
- 底包：`02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk`（实测 2,713,267,683 B / md5 `7ac7591436e92136f22b715c91b5b930`）
- 判据：`10_docs/暗影格斗3/硬约束.md` A 区 4 条（A-1 定怪删除且不新增功能位 / A-2 过验签双轨且禁复活 SignSpoof 与 TapLoginForceWeb 白名单 / A-3 五库零改动 / A-4 连续 3 次冷启验收）。
- 工具：python 3.11（zipfile 读中央目录 + 流式哈希）、`llvm-objdump`（NDK r27c）、`baksmali-fat.jar 2.5.2`、`aapt2.exe`（build-tools android-15）、`apksigner.jar`。

---

## ① 结论总表

| # | 核对项 | 结论 | 一句话依据 |
|---|---|---|---|
| 1 | 签名（apksigner --print-certs） | **PASS** | `Verifies`；v2=true / v3=true；DN `CN=StarryMagic`；v1=false 但实测 minSdkVersion=26 ⇒ 不影响安装 |
| 2 | A3 五库零改动（包内抽 5 条算 md5 对比底包） | **PASS** | 5/5 md5 完全相同（libil2cpp `53345cdb…`、global-metadata `809ec525…`、libaccess `82ee8142…`、libcpp `7dad26fc…`、libcore `4b0b4e79…`） |
| 3 | 新 so 在包内且可用（反汇编取立即数） | **PASS**（含 1 项缺项，见 3b） | so 191,232 B / md5 `da6d7821…`，DT_NEEDED 含 `libc++_shared.so`（包内已有）；三挂点常量全部落到实处；菜单 3 串各 1、违禁串 0 |
| 3b | 12 字节序言常量 `ff8303d1 e83b00fd fd7b08a9` | **FAIL（缺项，无安全后果）** | 包内 so 全域字节搜索 0 命中；源码 `VaAtFileOffBytes()/kPrologueChangeLife` 定义后从未被调用（死代码被编译器丢弃）⇒ 产物只有 4 字节序言校验 |
| 4 | A2 双轨（dex 级） | **PASS** | classes9.dex 有 `KillerApplication193`（super = `Landroid/app/Application;`）+ 证书 base64 头 `MIIDSTCCAjGgAwIBAgIEJhYcejAN`；classes5.dex `MultiDexApplication.super` → killer；classes.dex 原 `com.taptap.sdk.action` 已 0 命中，新串 = **`com.tap12tap.sdk.action`** |
| 5 | Manifest | **PASS** | `StarryInitProvider`(exported=false, initOrder=100)、`MenuService`(exported=false, foregroundServiceType=0x40000000 + SPECIAL_USE 子类型)、`appComponentFactory=com.starry.magic.sign.StarryAppComponentFactory`、原 `Application=com.accesssdkwrapper.AccessSDKApplicationWrapper` 保留、FGS / FGS_SPECIAL_USE 权限在、组件名 diff = **+2 / −0** |
| 6 | 原包条目零破坏 | **PASS** | 条目 4963 → 4967；删除仅 `META-INF/AYGD3.RSA`、`AYGD3.SF`；新增 6 条；**4957 条共有条目 size+CRC32 全等**；另抽 20 条（含 `resources.arsc` + 10 个 assets）sha256 20/20 相同 |
| 7 | A1 功能面（菜单 3 项；定怪全包 0） | **PASS** | so 内 4 条菜单串各 1 次；`定怪` / `SF3_FEAT_FREEZE` 全包（4967 条逐条流式扫描）**0 命中**；`FREEZE` 仅 3 条官方文件命中，且均为未改动条目 |
| 8 | 装机前门禁 `pre_install_gate.sh` | **PASS** | 两次运行均 `FAIL=0`（查2 PASS 18 项 / 查6 PASS / 查7 PASS / 查9 PASS；查12 SKIP：脚本判定本包非 TapSDK 门闩类） |
| 9 | 可复现性 | **PASS（1 处标注不可字节复现）** | 见 §2.9；`classes9.dex` md5 不可字节复现（baksmali 回读树相同） |
| — | A4「连续 3 次冷启 InitSuccessCallBack + LoginSuccessCallBack」 | **UNPROVEN（静态不可判）** | 需装机后取证；本轮禁止装机 |

**总线：8 项 PASS、1 项 FAIL（仅「12 字节序言常量未编入」这一加固项，不影响启动与三项功能）、1 项 UNPROVEN（A4 只能靠运行证据）。**

---

## ② 每项证据（命令 + 关键输出 + 哈希）

> 均在 `E:/nixiang` 下执行；`$OUT` 指成品 APK，`$BASE` 指底包。中间产物落 `13_scratch/sf3_remake/_tmp/verify/`（只新增，未改动仓库既有文件）。

### 2.0 体量与哈希

```
$ ls -la 08_output/暗影格斗3/
  2715627251  暗影格斗3_1.45.3_surgical-releaseSigned.apk
    21390875  暗影格斗3_1.45.3_surgical-releaseSigned.apk.idsig
    2715071296  暗影格斗3_1.45.3_surgical-unsigned.apk
$ md5sum "$OUT"
0bc69378712b98a3dcb62ad85aec1684
$ sha256sum "$OUT"
3e814c905e822bff62beddfb5efac32fe9f6f8d811bed573efa7076f049e5590
```

### 2.1 签名

```
$ java -jar 01_tools/android-sdk/build-tools/android-15/lib/apksigner.jar verify -v --print-certs "$OUT"
Verifies
Verified using v1 scheme (JAR signing): false
Verified using v2 scheme (APK Signature Scheme v2): true
Verified using v3 scheme (APK Signature Scheme v3): true
Verified using v3.1 scheme (APK Signature Scheme v3.1): false
Verified using v4 scheme (APK Signature Scheme v4): false
Number of signers: 1
Signer #1 certificate DN: CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN
Signer #1 certificate SHA-256 digest: 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d
Signer #1 certificate SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d
Signer #1 key algorithm: RSA / key size 2048
```

v4 那行是参数问题不是签名问题（v4 校验需显式传 idsig）：

```
$ java -jar .../apksigner.jar verify -v --print-certs --v4-signature-file "$OUT.idsig" "$OUT"
Verified using v4 scheme (APK Signature Scheme v4): true
```

minSdk 实测 26（见 2.5），v1=false 是 apksigner 对 minSdk>=24 的常规结果，不阻塞安装。SHA-1 `821c92e4…` 与硬约束 D-8 记录的星空证书一致。

### 2.2 A3 五库零改动（自己算，不看脚本打印）

python `zipfile` 流式 md5，底包与成品包分别计算：

| 条目 | 底包 md5 | 成品 md5 | 结论 |
|---|---|---|---|
| `lib/arm64-v8a/libil2cpp.so` | `53345cdb4d31dff336fdd575fb87b667` | 同 | **SAME** |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | `809ec525f0875c2092dc7f21e73fb288` | 同 | **SAME** |
| `lib/arm64-v8a/libaccess.so` | `82ee8142fa3a5996cb5eb7bd4a43677e` | 同 | **SAME** |
| `lib/arm64-v8a/libcpp.so` | `7dad26fcca3bfe85df53a45a046959e9` | 同 | **SAME** |
| `lib/arm64-v8a/libcore.so` | `4b0b4e792b2670e8f0f5bab8d47aa08b` | 同 | **SAME** |

前两条与硬约束 A-3 记录值逐字符相同。复现：`unzip -p "$APK" <条目> | md5sum`。

### 2.3 新 so：在包内、可用、常量落到实处

```
lib/arm64-v8a/libstarry_magic.so  size=191232  md5=da6d7821f8df70afca3d8a5ff1476a46
lib/arm64-v8a/libc++_shared.so    size=1794776 md5=ab695376e7624c39a72aac8ad5210ba1
classes8.dex                      size=646496  md5=e1451c5d71d082f9e733608e1dac849b
classes9.dex                      size=13928   md5=fbf5aa529880aa7b970d186fdbaffaf6
```

可用性：

```
$ llvm-objdump -p libstarry_magic.so | grep NEEDED
  NEEDED liblog.so / libandroid.so / libc.so / libm.so / libc++_shared.so / libdl.so
  SONAME libstarry_magic.so
$ llvm-nm --defined-only libstarry_magic.so
  no symbols        # 已 strip；-D --defined-only 20 个动态符号
```

`libc++_shared.so` 是 DT_NEEDED 且已在包内（新增条目），加载链不缺库。

**反汇编取证**（`llvm-objdump -d --print-imm-hex`，解析 32,718 条指令；命中地址为 so 内 VMA）：

| 常量 | 出现形式 | 命中地址 |
|---|---|---|
| `kFileValueGet = 0x5F3AAE8` | `mov w1,#0xaae8` + `movk w1,#0x5f3,lsl#16` | **0x14ab0 / 0x14ac0** |
| `kInsValueGet = 0xA9BE57FE` | `mov w2,#0x57fe` + `movk w2,#0xa9be,lsl#16` | **0x14ab4 / 0x14ac4** |
| `kFileValueSet = 0x5F3AB5C` | `mov w1,#0xab5c` + `movk w1,#0x5f3,lsl#16` | **0x14b40 / 0x14b50** |
| `kInsValueSet = 0xFC1D0FE8` | `mov w2,#0xfe8` + `movk w2,#0xfc1d,lsl#16` | **0x14b44 / 0x14b54** |
| `kFileChangeLife = 0x5F51968` | 64 位字面量（HookSlot 描述符） | **vaddr 0x2e3a0**（file 0x2d3a0） |
| `kInsChangeLife = 0xD10383FF` | 同上，紧跟其后的 4 字节字段 | **vaddr 0x2e3a8** |
| `kFileSetTimeScale = 0x6BB5C6C` | 同上（第二槽） | **vaddr 0x2e3e8**（file 0x2d3e8） |
| `kInsSetTimeScale = 0xFC1E0FE8` | 同上 | **vaddr 0x2e3f0** |

- 描述符槽位确被代码取用：`0x14d64: adr x0, 0x2e398`（伤害槽 name 字段起点）、`0x14d84: adr x0, 0x2e3e0`（速度槽起点），两处后接 `bl` 进入安装函数；同区还带 argc 字段 5 与 1（file 0x2d3c8 / 0x2d410），与 `kDamageSlots`/`kSpeedSlots` 的 5 / 1 对应。
- 段映射：ELF 头实测 `.data.rel.ro` addr=0x2e350 / off=0x2d350（file≠vaddr），故上表两列都写。

**3b 缺项（FAIL）**：`kPrologueChangeLife[12] = ff8303d1 e83b00fd fd7b08a9` 在 so 全域 **0 命中**；`0xA9BE57FE` 的字节序列有 4 处命中，但那是**指令自身编码**（`stp x30,x21,[sp,#-0x20]!`）——假阳性，不能当常量证据。根因（源码级实锤）：`Sf3Mod.cpp:229 VaAtFileOffBytes()` 与 `:153 kPrologueChangeLife` 只有定义、**无任何调用点**（`grep -n "VaAtFileOffBytes|kPrologueChangeLife"` 只有 153 / 229 两行），clang 把未引用的 static 函数与数组整体丢弃。实际生效的是 `HookSlot.expectInsn` 的 4 字节首字校验（上表已验证）。

### 2.4 A2 双轨（baksmali 反编译取证）

```
$ java -jar 01_tools/smali/baksmali-fat.jar d classes9.dex -o bs9
bs9/bin/mt/signature/KillerApplication193.smali（+ 11 个内部类 $a..$l）

$ grep -h "super" bs9/bin/mt/signature/KillerApplication193.smali
.class public Lbin/mt/signature/KillerApplication193;
.super Landroid/app/Application;

$ LC_ALL=C grep -a -o -F "MIIDSTCCAjGgAwIBAgIEJhYcejAN" classes9.dex | wc -l
1                      # 官方证书 base64 头命中
$ grep -rh "com.wanda.sf3.wd" bs9/ | head -1
    const-string v1, "com.wanda.sf3.wd"
```

挂载点：

```
$ grep -h "super" bs5/androidx/multidex/MultiDexApplication.smali
.super Lbin/mt/signature/KillerApplication193;      # 底包原值：Landroid/app/Application;
```

调用链静态自洽（**推断**，非运行证据）：Manifest 的 `application android:name=com.accesssdkwrapper.AccessSDKApplicationWrapper`（在 classes5.dex）→ 其 `.super Landroidx/multidex/MultiDexApplication;` → `attachBaseContext` 走 `invoke-super MultiDexApplication.attachBaseContext` → `MultiDex.install()` → 加载 classes2..9（含 classes9 的 killer）。minSdk 26，ART 原生多 dex。

登录/渠道改包法：

```
$ unzip -p "$BASE" classes.dex | LC_ALL=C grep -a -c "com.taptap.sdk.action"
1                      # 底包有
$ LC_ALL=C grep -a -o -F "com.taptap.sdk.action" classes.dex | wc -l
0                      # 成品已清零
$ LC_ALL=C grep -a -o -F "com.tap12tap.sdk.action" classes.dex | wc -l
1                      # 新串（原文）
```

新 action 原文 = **`com.tap12tap.sdk.action`**，位置 `com/taptap/sdk/login/internal/handlers/client/IntentHelper.smali:73`，语义是 `getAction(LoginRequest)` 中国内分支（`getRegionType()==0`）：

```
if-nez p1, :cond_e
const-string p1, "com.tap12tap.sdk.action"      # 原 "com.taptap.sdk.action"
:cond_e  const-string p1, "com.taptap.global.sdk.action"   # 海外分支未动
```

**dex 级全量 diff**（补 surgical 未覆盖的「只改了该改的吗」）：

```
$ diff -rq bs0_base bs0     # 底包 vs 成品 classes.dex，各 4837 个 smali
Files .../client/IntentHelper.smali differ            <- 唯一差异
$ diff -rq bs5_base bs5     # 各 5366 个 smali
Files .../androidx/multidex/MultiDexApplication.smali differ   <- 唯一差异
```

A-2「禁止复活」抽查：classes8.dex 内 `SignSpoof*` 类 **0 个**；`TapLoginForceWeb.smali` 存在（框架公共类），但整个 classes8.dex 内 `com.wanda.sf3.wd` **0 命中** ⇒ 无本游戏白名单条目。

### 2.5 Manifest（aapt2 xmltree，与底包逐属性比）

```
$ aapt2 dump xmltree --file AndroidManifest.xml "$OUT"  > manifest_out.txt
$ aapt2 dump xmltree --file AndroidManifest.xml "$BASE" > manifest_base.txt
```

| 项 | 底包 | 成品 | 判定 |
|---|---|---|---|
| `application android:name` | `com.accesssdkwrapper.AccessSDKApplicationWrapper` | 同 | **保留** |
| `application appComponentFactory` | `androidx.core.app.CoreComponentFactory` | `com.starry.magic.sign.StarryAppComponentFactory` | 有意替换：该类常量 `KILLER_CLASS = bin.mt.signature.KillerApplication193`、`ORIGINAL_FACTORY = androidx.core.app.CoreComponentFactory`，属双轨挂载件，不是 SignSpoof |
| `provider com.starry.magic.StarryInitProvider` | 无 | 有：enabled=true / exported=false / authorities=com.wanda.sf3.wd.starryinit / initOrder=100 | 新增 |
| `service com.starry.magic.menu.MenuService` | 无 | 有：enabled=true / exported=false / stopWithTask=false / foregroundServiceType=0x40000000，并挂 `<property android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE value="MOD menu overlay">` | 新增 |
| `FOREGROUND_SERVICE` / `FOREGROUND_SERVICE_SPECIAL_USE` | 无 | 有 | 新增（MenuService 必需） |

组件名集合 diff（提取全部 `android:name`）：

```
base names: 322   out names: 327
ADDED: android.app.PROPERTY_SPECIAL_USE_FGS_SUBTYPE
       android.permission.FOREGROUND_SERVICE
       android.permission.FOREGROUND_SERVICE_SPECIAL_USE
       com.starry.magic.StarryInitProvider
       com.starry.magic.menu.MenuService
REMOVED: (空)
```

⇒ 新增只有 2 组件 + 2 权限 + 1 属性，**无其它可疑组件，原组件一个没丢**。其余关键值：versionCode=907 / versionName=1.45.3 / minSdkVersion=26 / targetSdkVersion=30（与底包一致）。包内 AndroidManifest.xml 头 4 字节 = `03 00 08 00`（真 AXML，门禁查7 同判 PASS）。

### 2.6 原包条目零破坏（自己算，不看 4957 这句打印）

```
base entries: 4963   out entries: 4967
removed: META-INF/AYGD3.RSA , META-INF/AYGD3.SF
added  : META-INF/STARRY.RSA , META-INF/STARRY.SF , classes8.dex , classes9.dex ,
         lib/arm64-v8a/libc++_shared.so , lib/arm64-v8a/libstarry_magic.so
size/CRC changed count: 4
  AndroidManifest.xml   76092 -> 77308    crc f04db08b -> 9e4e8307
  META-INF/MANIFEST.MF 587143 -> 587517   crc 3a04f531 -> ede091da
  classes.dex          6642216 -> 6642216 crc 554a22ac -> af127cce
  classes5.dex         7041292 -> 7041340 crc 6129c817 -> 7a4e55c7
dup filenames: 0 / 0
```

⇒ **共有 4957 条条目除上表 4 条外 size 与 CRC32 全部相同**，无隐藏改写。

20 条 sha256 抽样（seed 20261124 固定，含 resources.arsc + 10 个 assets）：

```
SAME  resources.arsc                                     7f337a2679218f15
SAME  assets/bin/Data/de5199f135a226946997ed225960fa6d   12c39a2103111b0c
SAME  assets/Data/de1d3d85c1061b2710301250406c8bc0.bin   a5364f7c3dcf3403
... （共 20 条）
sampled 20 entries, mismatch=0
```

### 2.7 A1 功能面（菜单 3 项 / 定怪全包清零）

so 内串计数（`grep -a -o -F`）：

```
Category_功能             1
Slider_伤害倍率:1:100:1   1
Toggle_上帝模式           1
Slider_战斗速度:1:10:1    1
定怪 0 | SF3_FEAT_FREEZE 0 | FREEZE 0 | 无限体力 0 | 无限货币 0
Slider_攻击倍率 0 | Toggle_百倍伤害 0 | Toggle_免广告 0
```

与源码 `Sf3Entry.cpp:21-26 kSf3_Features`（占位 + 3 功能）逐字一致。

**全包流式扫描**（4967 条逐条读，9.0 s）：

```
## 定怪            : 0 entries with hits
## SF3_FEAT_FREEZE : 0 entries with hits
## FREEZE          : 3 entries with hits
   1  classes6.dex                                     -> 上下文 FREEZE_METHOD（官方 SDK）
   1  assets/bin/Data/f4df970a9802ae14fb318ed972c1b739 -> "5-HITS COMBO WILL FREEZE YOUR ENEMY"
   1  assets/Data/64a44ba282e21f7567b3d378dfada486.bin -> "ICON_FREEZE"
```

3 处 FREEZE 全部是官方资源/官方 SDK 数据，且这 3 条在 2.6 的 CRC 对比里属「未改动条目」⇒ 不是我们的代码、也不是本轮注入。

**关于 item 7 的 dex 侧期望（口径需修正）**：classes8.dex 内 `Category_功能` / `Slider_伤害倍率:1:100:1` / `Toggle_上帝模式` / `Slider_战斗速度:1:10:1` 均为 **0 次**，也搜不到 sf3 标识，`com.wanda.sf3.wd` 同样 0 次。这是**设计如此**：菜单项由 native 侧 `GameEntry.featureList = kSf3_Features`（指针）提供，dex 只放框架。所以「dex 与 so 一致」只能落在「so 有 3 项 + dex 无越界功能串」，不能要求 dex 里也有这 3 个串。据此判 PASS。

### 2.8 装机前门禁（照抄）

命令一（管线 pre_install_gate 阶段的等价参数，路径本地化）：

```
bash 04_scripts/_common/pre_install_gate.sh \
  --cpp "mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp" \
  --cpp "mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp" \
  --dex 07_patched/暗影格斗3/surgical/classes8.dex \
  --so mod菜单/starry-mod-menu/libs_sf3/arm64-v8a/libstarry_magic.so
```

```
PASS  [JNI配套] 18 个动态注册方法名在 dex 中全部有声明
汇总: PASS=1  FAIL=0  WARN=0  SKIP=0
装机前门禁通过
```

命令二（补查 6/7/9/12）：

```
bash 04_scripts/_common/pre_install_gate.sh \
  --apk "08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk" \
  --base-apk "02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk" \
  --game 暗影格斗3
```

```
PASS  [Manifest形态] 包内 AndroidManifest.xml 是二进制 AXML
---- 查6 轮次根因：轮次台账.tsv 判定列=6，带判定行数=4，阈值=2 轮 ----
PASS  [查6 轮次根因] 无「连续 2 轮无效且无定案」的功能（检查 0 个达标功能）
SKIP  [构建戳] 已退役
PASS  [so/metadata] so 与 metadata 均与底包一致（未替换：3f28d8a61ee3… / af8d4cc7e1e3…）
     （该包不含 TapSDK 门闩类 ⇒ 非 TapTap/TapSDK 包，跳过）
SKIP  [TapTap方案] 非 TapTap/TapSDK 包
汇总: PASS=2  FAIL=0  WARN=0  SKIP=2
装机前门禁通过
```

> 复现注意：`bash 04_scripts/pipeline.sh 暗影格斗3 gate <stage_id>` 只跑「轮次根因闸门」，不跑产物门禁；产物门禁走 `pre_install_gate.sh`（`pipeline.json` 的 pre_install_gate 阶段命令是 mac 路径，本机需替换）。
> **查12 被 SKIP 必须说明**：脚本 ①「manifest 挂 StarryAppComponentFactory ⇒ FAIL」的判据包在「是 TapTap 包」的判断里（先查 `.../initializer/repository/GatekeeperRepository`），本包无该类 ⇒ 整条跳过。我因此手工核了 ①：manifest 里确实挂着 StarryAppComponentFactory，但它是双轨挂载件（常量见 2.5），**不是** A-2 禁用的 SignSpoofAppComponentFactory（全包 SignSpoof 类 0 个）。属「机器关判据落后于口径」，见 ③-5。

### 2.9 可复现性

| 结论 | 复现命令 | 可字节复现 |
|---|---|---|
| 成品 md5/sha256 | `md5sum "$OUT"` / `sha256sum "$OUT"` | 是 |
| 5 库 md5 | `unzip -p "$APK" <条目> | md5sum` | 是 |
| 条目清单/增删/CRC | `13_scratch/sf3_remake/_tmp/listzip.py`（python zipfile infolist） | 是 |
| 20 条 sha256 抽样 | `13_scratch/sf3_remake/_tmp/sample20.py`（seed 20261124） | 是 |
| so 常量命中 | `llvm-objdump -d --print-imm-hex` + `_tmp/findimm3.py` | 是（同一 so 字节） |
| dex 级 diff | `baksmali-fat.jar d classes.dex -o bs0` + `diff -rq bs0_base bs0` | 是（同一 dex 字节） |
| **classes9.dex md5 `fbf5aa52…`** | 重新 assemble 同一 smali 树 | **不可**（build 自述「31 字节漂移，baksmali 回读树相同」，我复核 13 份 smali 与源码一致；该 md5 只作本包实测值记录） |

---

## ③ 与 02_build.md 自述不一致之处

1. **条目增删构成表述**：build 写「4963 − 3 个旧签名文件 + 4 新增 + 3 个 v1 产物 = 4967」。实测 **−2（AYGD3.RSA、AYGD3.SF）+6（STARRY.RSA、STARRY.SF、classes8.dex、classes9.dex、libc++_shared.so、libstarry_magic.so）**，`META-INF/MANIFEST.MF` 是原地重写（size/CRC 变了）不是新增。总数 4967 一致，仅构成描述不准。
2. **12 字节序言常量**：build 没提「`VaAtFileOffBytes`/`kPrologueChangeLife` 已是死代码、产物里没有这条强校验」。`01_static.md` 与源码注释都当它在生效。这是本轮唯一实质缺项（不影响启动与三项功能，但少了完整序言防漂移这层保护）。
3. **v4 签名**：build 只报 v1/v2/v3，未提 `.idsig`。实测该包**带 v4 签名**（传 `--v4-signature-file` 后 =true）。交付目录不应把 `.idsig` 当包内容分发。
4. **门禁查2 计数口径**：build 说「19 项 native 对齐」（surgical 的 dex↔ModBridge 校验），我实跑 `pre_install_gate` 查2 打印「**18 个**动态注册方法名在 dex 中全部有声明」。两者都 PASS，来源不同（前者抓 ModBridge 声明，后者抓源码正则）。建议文档注明工具。
5. **`pre_install_gate` 查12 与硬约束 A-2 的口径冲突（机器关落后）**：查12① 把「manifest 挂 StarryAppComponentFactory」判 FAIL，但 A-2 现行方案正是用它做 killer 挂载；本次未拦是因查12 先判「非 TapSDK 包」整条 SKIP。**这不是本包的问题**，但换到真有 TapSDK 门闩类的包会被误拦——建议把 ① 匹配串改为 SignSpoofAppComponentFactory（只记录，未改）。
6. **build §4 口径**：其表只列 so 的串计数与 classes8 的「定怪 0」，未夸大；但任务书 item 7 期望「classes8.dex 里 3 项配置」，实测 dex 侧是 0 次（在 so 里）。已在 2.7 纠正。

---

## ④ 装机前必须补的项

1. **A4 三次冷启**（唯一 UNPROVEN 项，必须装机后做）：连续 3 次冷启都要 `InitSuccessCallBack`，且 `LoginSuccessCallBack` 拿到 uid/accessToken。缺这项就不能声称「可交付」。
2. **设备归属确认**：`04_scripts/暗影格斗3/.device_serial` 与 `pipeline.json` 记 `f9a116a2`，本机 bashrc 的 `ANDROID_SERIAL` 指向 MuMu `127.0.0.1:16384`。装机前必须点名装哪台（本轮未做任何 adb 探测）。
3. **覆盖安装预期**：设备上大概率是官方签名包 ⇒ `INSTALL_FAILED_UPDATE_INCOMPATIBLE`；卸载重装不可逆，需用户当轮明确同意，不由「出包完成」推定。
4. **假成功校验**：装机后以设备 base.apk 的 md5 == `0bc69378712b98a3dcb62ad85aec1684` 为准，不看命令行回显。
5. **（可选、非阻塞）恢复序言强校验**：把 `VaAtFileOffBytes("ChangeLife", kFileChangeLife, kPrologueChangeLife, 12)` 接进 `SyncHooks` 的伤害槽安装路径（或删掉死代码声明以免文档与产物不符）。**需重编 so + 重签 ⇒ 另开一轮并重过门禁**，本轮按纪律不 patch。
6. **（可选）** 交付时不要把 `.idsig` 当包内容分发；它只是 v4 校验副产物。

---

## ⑤ 声明

- **本轮未改任何文件、未装机**：只新增了 `13_scratch/sf3_remake/_tmp/verify/`（解包取证副本与反汇编）、`13_scratch/sf3_remake/_tmp/*.py`（取证脚本），以及本报告 `13_scratch/sf3_remake/03_verify.md`。
- 未执行任何 adb 命令；未 patch / 重签 APK；未改 `sf3_sign.sh`、`surgical_sf3_1.45.3.sh` 或任何断言；未改 `mod菜单/**` 源码。
- 未整读大文件：`libil2cpp.so`、`so_dis.txt`、dump 类文件均按切片或脚本化检索；全部哈希为流式计算。
- 证据分级：凡写「实测」均为本轮命令输出；多 dex 加载链标为**推断**；A4 标 **UNPROVEN**。
