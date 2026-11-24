# 暗影格斗3 1.45.3（TapTap / com.wanda.sf3.wd / vc907）— 星光版重建出包报告

- 时间：2026-11-24 19:59–20:5x（Windows 11 + Git Bash / MSYS）
- 角色：builder（构建与出包）。**未装机、未卸载、未清数据**。
- 输入底包：`02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk`（2,713,267,683 B / md5 `7ac7591436e92136f22b715c91b5b930`）
- 产物：`08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk`
- 前提：沿用 `13_scratch/sf3_remake/01_static.md`（本报告不改任何游戏逻辑源码）

## 0. 结论摘要（每步一条）

| # | 步骤 | 结果 | 关键数字 |
|---|---|---|---|
| 1 | 三支私有脚本 Windows 跑通（+surgical 同批） | **PASS** | 5 个 .sh 过 `preflight`；改动 263+/44− |
| 2 | killer smali 源重建（三处必改项 + 回读） | **PASS** | dex md5 `93845adfea433c3e5b16b7d61d13235a` / 13,928 B / 13 份 smali |
| 3 | `sf3_build_so.sh` 编译 so | **PASS** | md5 `da6d7821f8df70afca3d8a5ff1476a46` / 191,232 B |
| 4 | classes8.dex（星光菜单 dex） | **PASS** | md5 `e1451c5d71d082f9e733608e1dac849b` / 646,496 B / native 19 项对齐 |
| 5 | 外科组装 + 签名 | **PASS** | 成品 md5 `0bc69378712b98a3dcb62ad85aec1684` / 2,715,627,251 B |
| 6 | 报告 | 本文件 | — |

**无 FAIL 项。** 有 4 处与任务书预期的偏差，逐条在 §7 说明（都不是绕过门禁）。

## 1. 步骤与证据

### 1.1 平台垫层（步骤 1）

宿主实测：`uname -s = MINGW64_NT-10.0`、`rsync` 缺失、`apktool` 不在 PATH、NDK 根目录只有 `ndk-build.cmd`、build-tools 真实文件是 `zipalign.exe / aapt2.exe / apksigner.bat`。

改法（**只改路径解析与工具探测**，构建命令与校验条目逐条保留）：

| 脚本 | 改动 | 行数 |
|---|---|---|
| `sf3_build_so.sh` | `NIXIANG="${NIXIANG_ROOT:-mac旧值}"`，缺 `01_tools` 时回落到脚本位置上两级；NDK 三级探测（ANDROID_NDK_HOME→本仓 r27c→mac）；无 sh 版 ndk-build 时直调 `prebuilt/windows-x86_64/bin/make.exe -f build/core/build-local.mk SHELL=cmd`（同 `04_scripts/爆裂防线/blfx_build_so_win.sh` 配方）；`nproc` 优先；`md5 -q / stat -c / sha256sum` 垫片；仓库路径含中文 ⇒ ASCII 暂存（`.workbuddy/scratch/sf3_so`）构建后拷回 | +75 / −14 |
| `sf3_sign.sh` | 同上路径/垫片；build-tools 探测（本仓→%LOCALAPPDATA%→mac，认 `zipalign.exe`）；**apksigner 优先 `java -jar lib/apksigner.jar`**（实测 `apksigner.bat` 经 cmd 转发会把含空格的 JAVA_HOME 拆开：`'C:\Program' 不是内部或外部命令`）；JDK17 取 `$NIXIANG_JAVA17`→`$JAVA_HOME`→mac `java_home`；显式 `--v1/--v2/--v3-signing-enabled true`；**库断言 2 个 → 5 个**（§2） | +79 / −17 |
| `sf3_install.sh` | 路径 + `md5` 垫片（本轮不执行） | +9 / −1 |
| `surgical_sf3_1.45.3.sh` | 平台垫层（md5/fsize/winpath）；`rsync`→`sync_tree()`（tar 管道，排除表与原参数一一对应）；apktool 前置进 PATH；NDK/aapt2/JDK 探测；`NDK_HOST_TAG` 认 `windows-x86_64`；工作副本写 `local.properties`（仓库那份不动）；SDK 选「真带 platforms/ 的」；native 声明校验先剥注释（§7-2） | +100 / −12 |

机器关：

```
$ bash 04_scripts/_common/preflight.sh 04_scripts/暗影格斗3/{sf3_build_so,sf3_sign,sf3_install,sf3_build_killer_smali}.sh
汇总: PASS=7  FAIL=0  REMIND=0  SKIP=0
✅ 机器关通过。
$ bash 04_scripts/_common/preflight.sh 04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh
汇总: PASS=4  FAIL=0  REMIND=0  SKIP=0
✅ 机器关通过。
```

> `~/.bashrc` 只影响交互式 shell；脚本直跑时 `NIXIANG_ROOT` 不存在，故每个脚本都加了「缺 `$NIXIANG/01_tools` ⇒ 以脚本位置上两级为准」的回落，mac 侧行为不变。（本机另有 `/Users/aliang/Documents/nixiang -> /e/nixiang` 软链，所以 mac 旧路径在本机也解析到同一工作区，容易误判「路径没生效」。）

### 1.2 killer smali 源（步骤 2）

- 模板：`04_scripts/追迹迷城/assets/zjmc_killer_smali`（13 份 smali，MT ApkSignatureKillerEx 形态，带 ``PKG`` / ``CERT_B64_WRAPPED`` 占位符）→ 入库副本 `04_scripts/暗影格斗3/assets/sf3_killer_smali/`（文本，13 个文件 79 KB）。
- 新脚本 `04_scripts/暗影格斗3/sf3_build_killer_smali.sh`（125 行）现场烘焙 → `07_patched/暗影格斗3/surgical/killer_smali/bin/mt/signature/*.smali`。
- 证书**由本包现算**：`META-INF/AYGD3.RSA`（1300 B PKCS#7）→ `openssl pkcs7 -print_certs` → `openssl x509 -outform DER` → base64（64 列，换行写成 `\n` 转义）→ 硬断言 → 替换占位符。
- 三项必改项「源文件 + baksmali 回读」双证：

```
[1/4] 取官方证书（META-INF/AYGD3.RSA → PKCS#7 → x509 DER → base64）
  DER 845 B / md5 c9950f9c3790803c7ccf42470142afe7（= 记录值）
  base64 1128 字符 / 18 行（单常量 1146 字符 << 65535 上限，无需拆分）
[2/4] baked: pkg=com.wanda.sf3.wd cert=1164 字符
[3/4] ① .super = Landroid/app/Application;  OK（无别家游戏壳父类）
      ② 目标包名 = com.wanda.sf3.wd  OK
      ③ 官方证书 base64 已烘焙（头 MIIDSTCCAjGgAwIBAgIEJhYcejAN）OK
      ④ 无别家游戏包名/壳类残留  OK
[4/4] 回读 ①：.super Landroid/app/Application;
      回读 ②：const-string v1, "com.wanda.sf3.wd"
      回读 ③：classes9.dex 内 grep -a 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' 命中
[killer] dex md5 = 93845adfea433c3e5b16b7d61d13235a / size = 13928 B
```

模板原本 `.super` 就是 `Landroid/app/Application;`（不是别家壳），`PkjNluikmwxGUJv` 已不在；这三点都写成断言，防以后换模板静默回退。

### 1.3 编译 so（步骤 3）

```
[build_so] NDK = /e/nixiang/01_tools/android-sdk/ndk/android-ndk-r27c (win)
[build_so] 路径非 ASCII → ASCII 暂存构建：/e/nixiang/.workbuddy/scratch/sf3_so
... Compile++ Sf3Entry / Sf3Mod / MainModern / Il2CppApi / Utils + xdl/*.c + hmac_sha256.c ...
[arm64-v8a] SharedLibrary  : libstarry_magic.so
[build_so] done → .../libs_sf3/arm64-v8a/libstarry_magic.so (da6d7821f8df70afca3d8a5ff1476a46)
[build_so] size = 191232
```

脚本自带串校验全过（`sf3` 标签、4 个必需串、4 个违规串为 0）；§4 是成品包内独立重算。

### 1.4 classes8.dex（步骤 4）

```
[1/6] local.properties sdk.dir → C:/Users/Administrator/AppData/Local/Android/Sdk
BUILD SUCCESSFUL in 7s / 50 actionable tasks: 50 executed
[2/6] -rw-r--r-- 646496 classes8.dex
  dex↔ModBridge native 声明校验通过（19 项）
  classes8.dex → 07_patched/暗影格斗3/surgical/classes8.dex (e1451c5d71d082f9e733608e1dac849b)
```

- 两个 R8 规则文件都含 `-keepnames class com.starry.magic.games.** { *; }`（`app/proguard-rules.pro`、`app/inject-dex-rules.pro`），**未改**。
- `bash 04_scripts/_common/check_framework_isolation.sh` → `[check_framework_isolation] PASS`（7/7 项）。
- `定怪` 串在 classes8.dex 内出现 **0** 次（§4）。

### 1.5 外科组装 + 签名（步骤 5）

```
[2.5/6] classes.dex(patched) → ... (24a552f3f752a7f34ca64d54c071c3f4)
[2.6/6] classes5.dex(patched) → ... (e30e50b3eff7c1a2acc6f45e2ce29057)
        classes9.dex(killer)   → ... (fbf5aa529880aa7b970d186fdbaffaf6)
[3/6]   binary AndroidManifest 77308 B
[4/6]   manifest ok (新增 20 项 / 丢弃 1 项=旧 factory，顺序无关)
[5/6]   surgical zip verified: 4957 个原始条目字节不变 + classes8.dex + starry so
        normalized: zip64 extras stripped/vneed fixed=727, classic EOCD ok
[6/6]   unsigned_md5=8348bf178265691bbd76fadec56718a1  unsigned_size=2715071296
[sign]  build-tools = 01_tools/android-sdk/build-tools/android-15 (zipalign.exe + lib/apksigner.jar)
[sign]  5 库 md5 断言 OK
[sign]  done → 08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk
[sign]  md5 = 0bc69378712b98a3dcb62ad85aec1684
[sign]  sha256 = 3e814c905e822bff62beddfb5efac32fe9f6f8d811bed573efa7076f049e5590
[sign]  size = 2715627251
```

签名验证原文：

```
$ java -jar 01_tools/android-sdk/build-tools/android-15/lib/apksigner.jar verify -v --print-certs <APK>
Verifies
Verified using v1 scheme (JAR signing): false
Verified using v2 scheme (APK Signature Scheme v2): true
Verified using v3 scheme (APK Signature Scheme v3): true
Number of signers: 1
Signer #1 certificate DN: CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN
Signer #1 certificate SHA-256 digest: 83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d
Signer #1 certificate SHA-1 digest: 821c92e404ab5fdac8dbf157c2e467e71440917d
Signer #1 certificate MD5 digest: 161fca32a36d3df274458682a44cb774
Signer #1 key algorithm: RSA / key size 2048
```

v1 那行是 `false`，但 v1 产物确实在包里（`META-INF/MANIFEST.MF` 587,517 B、`META-INF/STARRY.SF` 587,644 B、`META-INF/STARRY.RSA` 1,379 B）；底包实测 `minSdkVersion=26`、`targetSdkVersion=30` ⇒ 装机只走 v2/v3（Android 8.0 起可装），这是 apksigner 对 minSdk≥24 的常规行为，不是签名失败。

成品 zip 结构：条目 **4967**（原包 4963 − 3 个旧签名文件 + 4 个新增(classes8 / classes9 / libstarry_magic.so / libc++_shared.so) + 3 个 v1 签名产物）；`unzip -l` 与 python `zipfile` 都能打开，首条目 `AndroidManifest.xml`、末条目 `META-INF/MANIFEST.MF`。

## 2. 5 个官方库 md5 复核（成品包内实测）

| 条目 | md5 | 期望 | 结果 |
|---|---|---|---|
| `lib/arm64-v8a/libil2cpp.so` | `53345cdb4d31dff336fdd575fb87b667` | 硬约束 A-3 记录值 | PASS |
| `assets/bin/Data/Managed/Metadata/global-metadata.dat` | `809ec525f0875c2092dc7f21e73fb288` | 硬约束 A-3 记录值 | PASS |
| `lib/arm64-v8a/libaccess.so` | `82ee8142fa3a5996cb5eb7bd4a43677e` | 2026-11-24 底包重算 | PASS |
| `lib/arm64-v8a/libcpp.so` | `7dad26fcca3bfe85df53a45a046959e9` | 同上 | PASS |
| `lib/arm64-v8a/libcore.so` | `4b0b4e792b2670e8f0f5bab8d47aa08b` | 同上 | PASS |

复现：`unzip -p <APK> <条目> | md5sum`。这 5 条在 `sf3_sign.sh` 里都是 `[[ != ]]` 硬断言（不过即 exit 1）；后 3 条是本轮补进去的（§7-1）。

## 3. killer 证书指纹对照（本包现算）

| 项 | 值 | 对照 |
|---|---|---|
| 来源 | `META-INF/AYGD3.RSA`（1300 B PKCS#7） | 官方底包原样条目 |
| 证书 DER | **845 B** | 与 `01_static.md` D3「DER 845 B」一致 |
| DER md5 | `c9950f9c3790803c7ccf42470142afe7` | 与硬约束 D-8 记录一致 |
| DER SHA-1 | `9078e787fb5246bfd147193343cf7944dadc8fd8` | 同上 |
| DER SHA-256 | `b147f6a485dfaa6dcb9154e5e5190d342264b0568566749df0aa38cfe55e76e3` | 本轮新算，脚本断言值 |
| Subject | `C=cn, ST=gd, L=sz, O=szu, OU=atr, CN=sagely` | 与硬约束 D-8「CN=sagely」一致 |
| base64 | 1128 字符 / 18 行（每 64 列，`\n` 转义） | 头 `MIIDSTCCAjGgAwIBAgIEJhYcejAN` 命中 |

dex 侧：`LC_ALL=C grep -a 'MIIDSTCCAjGgAwIBAgIEJhYcejAN' classes9.dex` 命中 —— surgical `[2.6/6]` 里是硬断言。

## 4. 菜单串校验（成品包内 so / dex 重算，不是脚本自述）

```
# 包内 lib/arm64-v8a/libstarry_magic.so（191232 B / md5 da6d7821f8df70afca3d8a5ff1476a46）
Category_功能             1
Slider_伤害倍率:1:100:1   1
Toggle_上帝模式           1
Slider_战斗速度:1:10:1    1
# 违规串（必须全 0）
定怪 0 | Slider_攻击倍率 0 | Slider_战斗加速 0 | Toggle_百倍伤害 0 | Toggle_免广告 0 | 无限体力 0 | 无限货币 0
# 包内 classes8.dex（646496 B / md5 e1451c5d71d082f9e733608e1dac849b）
定怪 0
```

菜单只有 3 个功能位（伤害倍率 / 上帝模式 / 战斗速度），与硬约束 A-1 一致；**定怪在 so 与 dex 里都是 0 命中**。

## 5. 机器关输出

| 门禁 | 结果 |
|---|---|
| `preflight.sh`（4 个改过的 .sh） | PASS=7 FAIL=0 REMIND=0 |
| `preflight.sh`（surgical，改后重跑） | PASS=4 FAIL=0 REMIND=0 |
| `check_framework_isolation.sh` | `[check_framework_isolation] PASS`（7/7 项） |
| `sf3_build_so.sh` 自带串校验 | 4 必需串命中 / 4 违规串 0 |
| `sf3_build_killer_smali.sh` 断言 | ① 父类 ② 包名 ③ 证书 ④ 无他包 + baksmali 回读 3 项 全 OK |
| surgical 内部断言 | manifest 结构化比对（+20/−1）、4957 条目 sha256 不变、classes.dex/classes5.dex/classes9.dex 交叉校验、so 字节一致 |
| `sf3_sign.sh` 内部断言 | 5 库 md5、so md5 与构建产物一致、classes8/classes9 在包、apksigner verify |

## 6. 本轮改了什么（git 视图，只列本游戏 pathspec）

```
$ git diff --numstat -- 04_scripts/暗影格斗3/<本轮改动>
75  14  04_scripts/暗影格斗3/sf3_build_so.sh
9   1   04_scripts/暗影格斗3/sf3_install.sh
79  17  04_scripts/暗影格斗3/sf3_sign.sh
100 12  04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh
```

新增（未跟踪）：`04_scripts/暗影格斗3/sf3_build_killer_smali.sh`、`04_scripts/暗影格斗3/assets/sf3_killer_smali/**`（13 份 smali，79 KB）。

**没有改**：`mod菜单/starry-mod-menu/app/src/main/jni/` 下一切（`Main.cpp`/`MainModern.cpp`/`GameConfig.h`/`Android.mk`/`Android_sf3.mk`/`Application.mk`）、`Games/Sf3Mod.cpp`、`Games/Sf3Entry.cpp`、两个 R8 规则文件、`pipeline.json`/`pipeline.lock`、仓库 `local.properties`（保持 mac 值）。

> 同目录另外 4 个改动**不是我做的**（19:20–19:32 前序轮次留下）：`device_ready_sf3.sh`、`fetch_official_sf3.sh`、`official_apk.json`、`pipeline.json`。本轮未碰。

## 7. 与任务书预期的偏差（4 处，都不是放宽门禁）

1. **`sf3_sign.sh` 原本只有 2 条库断言**（libil2cpp + global-metadata），任务书说「5 个」。按硬约束 A-3 把 `libaccess/libcpp/libcore` 补成硬断言，md5 取自底包实测（§2）。属于**加严**，现有断言一条没动。
2. **surgical 的 dex↔ModBridge native 校验原本 3/22 假失败**。原因：`ModBridge.java:702-703` 有一行**注释**记录「2026-11-21 OTA 下线」，里面写着被删掉的 `nativeVerifyOtaSig / nativeApplyPlayPatch / nativeGetBuildStamp`；旧写法 `grep -oE '\bnative[A-Za-z0-9_]+'` 把注释当声明。改法：先用 python 剥 `/* */` 与 `//` 注释再取声明，判据不变（19 项全命中）。**不动 ModBridge.java**（框架文件禁改）。
   中途再踩一个坑：python 在 Windows 上 `print` 会把 `\n` 写成 CRLF ⇒ 变量尾部带 `\r`，grep 全不命中（第二次跑出 18/19 假失败）。已加 `| tr -d '\r'`。
3. **`01_tools/android-sdk` 没有 `platforms/`**（只有 build-tools + ndk）⇒ `compileSdk 35` 会失败。surgical 改成「选真带 `platforms/` 的 SDK」，本机落到 `%LOCALAPPDATA%/Android/Sdk`（含 android-35/36）；仓库 `local.properties` 不动，只改工作副本。
4. **classes9.dex 的 md5 不可字节复现**：同一份 smali 树连续 assemble 得到 `93845adfea...` / `fbf5aa52...` / `3e394d20...` 三个 md5，差异共 31 字节（dex 头 checksum/signature 段 + 少数偏移），**baksmali 回读的 smali 树逐文件完全相同**。与 `星空落地` §9「classes8.dex md5 非同源可复现」同一现象 ⇒ 以内容断言为门禁，不拿 md5 当复现判据。

## 8. 未装机声明 + 装机前置条件

- **本轮没有装机、没有卸载、没有清数据、没有任何 adb 调用**。只读底包，只写 `07_patched/` 与 `08_output/`。
- 装机（`bash 04_scripts/暗影格斗3/sf3_install.sh`）需要 lead 另行向用户取授权，且必须满足：
  1. **用户明确同意覆盖安装**（设备上大概率是官方签名包 ⇒ `INSTALL_FAILED_UPDATE_INCOMPATIBLE`）；或明确同意「卸载官方包、接受存档/数据丢失」后才用 `SF3_ALLOW_UNINSTALL=1`；
  2. 设备在线且目标明确：`04_scripts/暗影格斗3/.device_serial` 与 `pipeline.json` 记的是 `f9a116a2`，而本机 bashrc 的 `ANDROID_SERIAL` 指向 MuMu `127.0.0.1:16384` —— **装机前必须先确认装哪台**；
  3. 装机后按硬约束 A-4 验收：**连续 3 次冷启**都 `InitSuccessCallBack`、`LoginSuccessCallBack` 拿到 uid/accessToken；再跑 `post_install_precheck.sh --game 暗影格斗3`。
- 已知安装风险（`星空落地` §9）：**「install 假成功」**——以设备 `base.apk` md5 == `0bc69378712b98a3dcb62ad85aec1684` 为准，不看命令行回显。

## 9. 复现链（一条命令一层）

```bash
cd /e/nixiang
bash 04_scripts/暗影格斗3/sf3_build_so.sh             # → libs_sf3/arm64-v8a/libstarry_magic.so        (da6d7821...)
bash 04_scripts/暗影格斗3/sf3_build_killer_smali.sh   # → 07_patched/暗影格斗3/surgical/killer_smali   (baked, 回读三证)
bash 04_scripts/暗影格斗3/surgical_sf3_1.45.3.sh      # → 08_output/.../..._surgical-unsigned.apk     (8348bf17...)
bash 04_scripts/暗影格斗3/sf3_sign.sh                 # → 08_output/.../..._surgical-releaseSigned.apk (0bc69378...)
```

中间产物（留证，均不入 git）：`07_patched/暗影格斗3/surgical/{AndroidManifest.xml,classes.dex,classes5.dex,classes8.dex,classes9.dex,killer_smali/}`；ASCII 暂存目录 `.workbuddy/scratch/sf3_so`（可删，下次构建自动重建）。
