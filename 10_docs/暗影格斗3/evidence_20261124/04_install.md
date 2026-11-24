# 暗影格斗3 星光版 — 装机与冷启验收报告（模拟器）

- 日期：2026-11-24 21:00–21:37（Asia/Shanghai）
- 设备：**emulator-5554 only**（雷电；x86_64 + libhoudini，Android 14/API34）
- 未触碰：真机 `96f0d883`、MuMu `127.0.0.1:16384`
- 包：`08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk`（2,715,627,251 B）
- 本地产物 md5：`0bc69378712b98a3dcb62ad85aec1684`（`md5sum` 实测，与派单一致）
- 证据根目录：`09_test/暗影格斗3/install_20261124/`、`09_test/暗影格斗3/precheck_20261124_213316/`

## 0. 结论速览

| 项 | 结果 |
|---|---|
| 安装 | **成功**（Streamed Install，见 §2.4） |
| 设备端 base.apk md5 == `0bc69378…` | **是**（逐字节相等） |
| 签名 DN | **CN=StarryMagic**（SHA-1 `82:1C:92:E4:04:AB:5F:DA:C8:DB:F1:57:C2:E4:67:E7:14:40:91:7D`） |
| 连续 3 次冷启（A-4） | **3/3 InitSuccessCallBack + 3/3 LoginSuccessCallBack（uid 3009937009）** |
| `[sf3]` 星光日志 | **3/3 出现**（`lib ready` / `初始化成功` / `MOD 已就绪 ready=1`） |
| 崩溃 | **0**（FATAL/SIGSEGV/tombstone/ANR 全无） |
| post_install_precheck | **PASS_TO_HUMAN（rc=0）** |
| 菜单功能层（伤害/速度滑块） | **UNPROVEN** —— 未做任何点按，滑块日志必然不出现 |

## 1. 装机前门禁（必须先过）

```bash
# ① 产物交叉关（等同 pipeline.json 的 pre_install_gate 阶段，Windows 路径改写）
bash 04_scripts/_common/pre_install_gate.sh \
  --cpp 'mod菜单/starry-mod-menu/app/src/main/jni/MainModern.cpp' \
  --cpp 'mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Mod.cpp' \
  --dex '07_patched/暗影格斗3/surgical/classes8.dex' \
  --so  'mod菜单/starry-mod-menu/libs_sf3/arm64-v8a/libstarry_magic.so' \
  --game 暗影格斗3
```
结果：`PASS=2 FAIL=0`
- ✓ JNI 配套：18 个动态注册方法名在 dex 中全部有声明
- ✓ 查6 轮次根因：无「连续 2 轮无效且无定案」的功能

```bash
# ② 包形态关 + so/metadata 配套关（A-3 五库零改动的机器证据）
bash 04_scripts/_common/pre_install_gate.sh \
  --apk '08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk' \
  --base-apk '02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk'
```
结果：`PASS=1 FAIL=0 SKIP=2`
- ✓ Manifest 形态：包内 `AndroidManifest.xml` 是二进制 AXML
- ✓ so/metadata：**两者均与底包一致（未替换）** ⇒ `libil2cpp.so`/`global-metadata.dat` 零改动（A-3）
- SKIP 查8（构建戳已退役）、SKIP 查12（该包不含 TapSDK 门闩类 ⇒ 非 TapTap/TapSDK 包）

> `pipeline.sh` 本轮不可用：`❌ pipeline.sh 已被改动（指纹不匹配）… 实际指纹: <计算失败>`（Windows 换行导致指纹计算失败），故按 `stages.md` 的脚本原语直接调用。未改 `pipeline.json`/`pipeline.lock`。

## 2. 卸载 + 安装

### 2.1 卸载前留证（官方包）

```
versionName=1.45.3        versionCode=907      targetSdk=30   primaryCpuAbi=arm64-v8a
firstInstallTime=2026-11-24 19:06:28   lastUpdateTime=2026-11-24 19:06:28
signatures=PackageSignatures{6bc8365 version:2, signatures:[85df7c35]}
证书 DN（从设备同源官方包 META-INF/AYGD3.RSA 提取）：CN=sagely, OU=atr, O=szu, L=sz, ST=gd, C=cn
  SHA-1  90:78:E7:87:FB:52:46:BF:D1:47:19:33:43:CF:79:44:DA:DC:8F:D8
  SHA-256 B1:47:F6:A4:85:DF:AA:6D:CB:91:54:E5:E5:19:0D:34:22:64:B0:56:85:66:74:9D:F0:AA:38:CF:E5:5E:76:E3
pm path: /data/app/~~1tRXwzBaUxvEYyNmvQTs2Q==/com.wanda.sf3.wd-1v9EQGopOWpgamSH6Pa7-A==/base.apk
```
原始 dumpsys 全文：`09_test/暗影格斗3/install_20261124/pre/dumpsys_package_pre_uninstall.txt`

### 2.2 命令链（脚本优先，失败原文照抄）

```bash
# ① 未带卸载授权（是我第一次误跑，记录失败原文）
bash 04_scripts/暗影格斗3/sf3_install.sh
→ Failure [INSTALL_FAILED_UPDATE_INCOMPATIBLE: Existing package com.wanda.sf3.wd signatures do not match newer version; ignoring!]

# ② 用户已授权：卸载官方包 + 全新安装
MSYS2_ARG_CONV_EXCL=/data SF3_ALLOW_UNINSTALL=1 bash 04_scripts/暗影格斗3/sf3_install.sh
→ Failure [INSTALL_FAILED_UPDATE_INCOMPATIBLE: …]            # 覆盖安装按签名判死
→ adb shell pm uninstall --user 0 com.wanda.sf3.wd            # 脚本按授权执行，数据被清（已授权）
→ Failure [INSTALL_FAILED_MEDIA_UNAVAILABLE: Failed to restorecon]   # 卸载后的 pm install 失败
```

### 2.3 装不上的两个环境缺口（事实，非推测）

1. **MSYS 路径转换**：Git Bash 把 `/data/local/tmp/sf3_surgical.apk` 改写成 `C:/Program Files/Git/data/local/tmp/…` 传给 `adb.exe` ⇒
   `adb: error: failed to copy … remote secure_mkdirs failed: No such file or directory`。修法：`MSYS2_ARG_CONV_EXCL=/data`（已验证）。
   脚本本身未处理该转换，且**push 失败时脚本静默退出、不打印错误**（实测两次，日志只有两行）。
2. **`INSTALL_FAILED_MEDIA_UNAVAILABLE: Failed to restorecon`**：雷电（SELinux=Permissive，`/data/local/tmp` 上下文 `shell_data_file`）下，卸载后走 `pm install --user 0 <path>`（legacy 拷贝路径）会在 restorecon 处失败；`su restorecon` 无效（Skipping restorecon on directory）。会话式安装路径不受影响。

### 2.4 实际生效的安装命令

```bash
adb -s emulator-5554 install -r -d --user 0 \
  '/Users/aliang/Documents/nixiang/08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk'
→ Performing Incremental Install
  Exception … IllegalArgumentException: Incremental installation not allowed.   # 雷电不支持增量，自动回退
→ Performing Streamed Install
  Success                                                                     # RC=0，耗时约 30 s
```
（`/Users/aliang/…` 是本机指向 `/e/nixiang` 的符号链接，同一工作区）

### 2.5 安装后闭环校验（三项全过）

| 项 | 实测 | 判定 |
|---|---|---|
| 设备 `base.apk` md5 | `0bc69378712b98a3dcb62ad85aec1684` | ✅ 等于本地 md5 |
| firstInstallTime / lastUpdateTime | `2026-11-24 21:16:55` / `2026-11-24 21:16:55`（全新安装，非旧包） | ✅ |
| 签名 DN | `CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`；SHA-1 `82:1C:92:E4:…:91:7D` | ✅ |

```
pm path: /data/app/~~nkZNCb3xgPtOA-taDgcgZA==/com.wanda.sf3.wd-Baqnzi0f4FzfFQcMnfzY7Q==/base.apk
versionName=1.45.3  versionCode=907  primaryCpuAbi=arm64-v8a
```
证据：`install_20261124/verify/{pm_path.txt,dumpsys_after.txt,dumpsys_head.txt,device_md5.txt,df.txt,device_cert.txt,device_STARRY.RSA,adb_install.log}`
成功戳：`04_scripts/暗影格斗3/.install_ok` = `0bc69378712b98a3dcb62ad85aec1684 2026-11-24 21:18:02`（闭环三项通过后才写）

**双轨验签运行时轨道已生效（附带强证据）**：
- 预检日志：`StarryAppFactory: SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared`
- 游戏自身 SDK 上报里读到的是官方指纹：AppsFlyer `"sig":"B147F6A485DFAA6DCB9154E5E5190D342264B0568566749DF0AA38CFE55E76E3"`（＝官方证书 SHA-256）

## 3. 连续 3 次冷启（A-4 验收基线）

每次流程：`am force-stop` → 等 3 s → `logcat -c` → `am start -W -n com.wanda.sf3.wd/com.nekki.unityplugins.NekkiNativeActivity` → 等 **95 s** → `logcat -d` 切片。

| 次 | 目录 | LaunchState / TotalTime | 观察窗 | InitSuccessCallBack | LoginSuccessCallBack | `[sf3]` 日志 | 崩溃 | 证据 |
|---|---|---|---|---|---|---|---|---|
| 1 | `w95_1` | COLD / 1533 ms | 97 s | ✅ channel=hoolai; channelId=10247; gameId=3015 | ✅ uid=3009937009 + accessToken(JWT) | ✅ lib ready / 初始化成功 / MOD 已就绪 ready=1 | 无 | logcat.txt, pid.txt(14768), timing.txt, pm_path/dumpsys_head/df |
| 2 | `w95_2` | COLD / 1936 ms | 97 s | ✅ 同上 | ✅ uid=3009937009 + accessToken | ✅ 同上 | 无 | 同上，pid=15540 |
| 3 | `w95_3` | COLD / 1642 ms | 97 s | ✅ 同上 | ✅ uid=3009937009 + accessToken | ✅ 同上 | 无 | 同上，pid=16351 |

三次均**逐次**成立，不是合并结论。third 次的原始关键行（其它两次同形）：
```
21:30:53.906 I MenuService: overlayVisible=true attached=true
21:30:54.989 I Mod_Menu: [sf3] lib ready (dual-path, lazy hooks)
21:30:54.990 I StarryMagic: [sf3] 初始化成功
21:30:54.990 I StarryMagic: MOD 已就绪 tag=sf3 ready=1
21:31:00.357 I Unity: -->[AccessSDKCallBacks] InitSuccessCallBack … channel: hoolai; channelId: 10247; gameId: 3015;
21:31:06.134 I Unity: -->[AccessSDKCallBacks] LoginSuccessCallBack … uid: 3009937009; accessToken: eyJhbGciOiJIUzI1NiIs…
```

### 3.1 短窗口（60–65 s）三轮的补充事实（重要，别当失败看）

| 次 | 目录 | 窗口 | Init | Login | `[sf3]` | 说明 |
|---|---|---|---|---|---|---|
| c1 | `1` | 52 s | ✅ | ✗ | **✗（0 条）** | 首次安装后第一启动：`libstarry_magic.so` 已被映射进内存（`/proc/<pid>/maps` 有 r--p/rw-p 段），但当窗口内**没有**任何 `[sf3]` 输出 |
| c2 | `2` | 67 s | ✅ | ✗ | ✅ 2 条 | 登录在窗口之后才发生 |
| c3 | `3` | 66 s | ✅ | ✅ | ✅ 2 条 | 登录距 Init 约 40.5 s |

结论：`[sf3]` 首启不出现在 50 s 窗口内＝**首启更慢**（dexopt/资源解包），不是 hook 缺失；窗口 ≥95 s 时 3/3 稳定出现。

## 4. 装机后预检

```bash
bash 04_scripts/_common/post_install_precheck.sh --game 暗影格斗3 --serial emulator-5554
```
```
✓ 包已安装: package:/data/app/~~nkZNCb3xgPtOA-taDgcgZA==/…/base.apk
- 已干净启动（force-stop 后 monkey 冷启）
✓ 菜单 so 加载标记: StarryAppFactory: SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared
✓ 菜单弹出标记:   MenuService: native menu shown mode=ACTIVITY
✓ 授权通过标记:   ModBridge: auth game_id synced from native: sf3
✓ PASS_TO_HUMAN —— so 加载/菜单弹出/空转 3 分钟无崩溃，装机层证据齐
RC=0
```
证据：`09_test/暗影格斗3/precheck_20261124_213316/`（`logcat.txt` 1.06 MB + `monkey.log`）

## 5. 剩余 UNPROVEN（不得写成"已通过"）

1. **菜单三项功能的行为有效性**：本轮零点按（禁 `input tap`），因此 `[sf3] damage slider=` / `speed slider=` 一条都没有；伤害倍率 1–100 / 上帝模式 / 战斗速度 1–10 的 L3 只能用用户实测证明。
2. **菜单实际弹出/可操作性**：机器只拿到 `native menu shown mode=ACTIVITY`（宿主 overlay 已挂）+ 悬浮球宿主在跑；用户是否能看见/滑动，未证。
3. **登录态持久性**：uid 3009937009 是本次新装后首次登录（`firstInstallTime=21:16:55`）；重启后是否仍免登录未测。
4. **热更/资源下载完成度**：三次冷启都在 95 s 内改完日志窗口，未验证进入战斗、未验证 `libil2cpp` 侧 hook 在真实战斗帧上的命中（`[sf3]` 目前只有 init 三条）。
5. **脚本侧可复现性缺口**：`sf3_install.sh` 在 Windows 需 `MSYS2_ARG_CONV_EXCL=/data`，且卸载后的 `pm install` 在本模拟器必失败（`INSTALL_FAILED_MEDIA_UNAVAILABLE`）。→ 建议后续给脚本加 `SF3_INSTALL_MODE=session`（`adb install`）与路径转换豁免，须另开一轮，本轮未改脚本。

## 6. 给用户的最小操作请求卡（一张，只一组）

> **① 请你做什么**：打开「暗影格斗3」，等它过完启动页/热更，**看到游戏主界面**（能看到角色/主城，不是登录页、不是黑屏）。
> **② 成功标志**：主界面出现；右上角或悬浮球上有**星光菜单入口**（悬浮球/小图标）。
> **③ 我接着看哪条日志**：`adb -s emulator-5554 logcat -d | grep -E '\[sf3\]|LoginSuccessCallBack|native menu shown'` —— 我要看是否新增 `native menu shown` 之后的 `[sf3]` 行与 `LoginSuccessCallBack`。
> 期间**不用点菜单、不用改滑块**；这轮只确认「主界面 + 菜单入口可见」。你回一句「正常」或「没看到入口」即可。

## 7. 只碰 emulator-5554 的自证

所有设备命令均为 `adb -s emulator-5554 …`（含 `install/logcat/am/pm/dumpsys/maps`）；对 `96f0d883` 与 `127.0.0.1:16384` 零操作。未改 `pipeline.json`/`pipeline.lock`；未卸载/清理除 `com.wanda.sf3.wd` 以外的任何包（仅删除本机 `/data/local/tmp` 下自建的 probe/certcheck 临时文件）。
