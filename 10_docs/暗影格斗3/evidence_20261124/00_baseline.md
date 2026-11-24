# 暗影格斗3 星光复刻 — 基线取证（2026-11-24，雷电模拟器）

> 侦察兵轮次产物。范围：只读取证 + 拉底包。未装机、未卸载、未清数据、未 hook、未构建。
> 唯一被触碰的设备：`emulator-5554`（雷电）。真机 `96f0d883` 全程未连接操作。
> 硬约束来源：`10_docs/暗影格斗3/硬约束.md` A 区 4 条（第 3 条二进制零改动、第 4 条三次冷启基线）。

---

## 1. 设备与包事实（实测，2026-11-24 19:13–19:34）

| 项 | 值 | 取法 |
|---|---|---|
| 设备 serial | `emulator-5554`（transport_id=1，在线） | `adb devices -l` |
| 设备型号 / props | `model:SGT_AL10` / `product:SGT-AL10` / `device:graceltexx` | `adb devices -l` |
| Android / ABI | 14 / x86_64（primaryCpuAbi 报 arm64-v8a，靠 native bridge） | `getprop`、`dumpsys package` |
| root | `su -c id` → `uid=0(root)` | `adb shell 'su -c id'` |
| 包名 | `com.wanda.sf3.wd` | `dumpsys package` |
| versionName / versionCode | **1.45.3** / **907** | `dumpsys package` |
| minSdk / targetSdk | 26 / 30 | `dumpsys package` |
| primaryCpuAbi | arm64-v8a | `dumpsys package` |
| installer | `com.taptap` | `dumpsys package` |
| firstInstallTime / lastUpdateTime | 2026-11-24 19:06:28 / 2026-11-24 19:06:28 | `dumpsys package` |
| 设备端 base.apk | `/data/app/~~1tRXwzBaUxvEYyNmvQTs2Q==/com.wanda.sf3.wd-1v9EQGopOWpgamSH6Pa7-A==/base.apk` | `pm path` |
| 设备端 APK 大小 | 2,713,267,683 B | `stat -c %s` |
| 设备端 APK sha256 | `1b17ab1c…d166087`（全值见 §4） | `sha256sum` |
| 单 base.apk | 是（`pm path` 仅 1 行，无 split） | `pm path` |

---

## 2. 脚本改动（只改两支，均已过 preflight）

`git --no-pager diff --numstat`：`device_ready_sf3.sh +25/−10`、`fetch_official_sf3.sh +29/−5`（合计 +54/−15）。

### 2.1 `04_scripts/暗影格斗3/device_ready_sf3.sh`（49 → 63 行）

- `EXPECTED_SERIAL` 拆成两个变量：`EXPECTED_SERIAL_SET`（用户是否**显式点名**）与 `EXPECTED_SERIAL`（默认仍 `f9a116a2`）。
- **显式点名分支**：设备列表里 `grep -Fxq` 命中点名 serial ⇒ 直接用那一台，**不再要求 `COUNT==1`**（真机 + 模拟器可同时在线）。点名设备不在线 ⇒ 报错退出。
- **不点名分支**：完全保留旧行为（必须恰好一台在线 + 必须等于默认真机 `f9a116a2`，否则拒绝）。
- 结果：`EXPECTED_SERIAL=emulator-5554` 时只认模拟器；`96f0d883` 只有在被显式点名时才会成为目标。

### 2.2 `04_scripts/暗影格斗3/fetch_official_sf3.sh`（140 → 163 行）

四处改动，**全部硬校验保留**（单 base.apk、大小一致、sha256 一致、`unzip -tq`、斌哥签名拦截、写 `official_apk.json`）：

1. **点名校验**（6 行）：`EXPECTED_SERIAL` 显式给出且与 `.device_serial` 不一致 ⇒ 直接退出。防多设备下拿旧 serial 拉错设备。
2. **`adb pull` 的 MSYS 参数改写**（9 行）：Git Bash 会把 argv 里的 `/data/app/...` 改写成 `C:/Program Files/Git/data/app/...`（实测报 `adb: failed to stat remote object`）。改为 `MSYS_NO_PATHCONV=1` + 本地目标走 `cygpath -m`；无 `cygpath`（mac）时保持原样。原 `exec-out su -c cat` 回退分支保留。
3. **跨平台指纹**（9 行）：`stat -f '%z'` / `shasum -a 256` / `md5 -q` 在本机裸环境**全部不可用**（无 `md5`/`shasum`，GNU `stat` 的 `-f` 是"文件系统信息"且把错误打到 stdout）。改成三个小函数：先探 `stat -c`、`shasum`、`md5`，缺失则回退 GNU 版本。mac 侧行为不变。
4. **`-x` → `-f`**（apksigner 探测，3 行）：Windows 只有 `apksigner.bat`，MSYS 对 `.bat` 的 `[ -x ]` 恒为假 ⇒ 旧写法让**签名者读取与斌哥包拦截整段静默跳过**（首跑 `signer` 字段确实为空）。改 `[ -f ]`（与 `_common/env.sh` 同口径）后重跑，`signer` 正常落盘。

preflight 结果（两支）：`汇总: PASS=5  FAIL=0  REMIND=0  SKIP=0 / ✅ 机器关通过`。

---

## 3. 实际执行命令（逐条，按序）

```bash
# 0) 环境前提：本机 shell 无 md5/shasum/BSD stat，需加载垫片；pipeline 自身也依赖 shasum 做指纹自检
export BASH_ENV="$HOME/.nx_shims.sh"

# 1) 改完两支脚本 → 机器关
bash 04_scripts/_common/preflight.sh 04_scripts/暗影格斗3/device_ready_sf3.sh 04_scripts/暗影格斗3/fetch_official_sf3.sh

# 2) 管线现状
BASH_ENV=$HOME/.nx_shims.sh EXPECTED_SERIAL=emulator-5554 bash 04_scripts/pipeline.sh 暗影格斗3 status --brief

# 3) 回退 + 重跑 device_ready（旧 done 戳是 2026-09-19 mac 真机的）
BASH_ENV=$HOME/.nx_shims.sh EXPECTED_SERIAL=emulator-5554 bash 04_scripts/pipeline.sh 暗影格斗3 reset device_ready
BASH_ENV=$HOME/.nx_shims.sh EXPECTED_SERIAL=emulator-5554 bash 04_scripts/pipeline.sh 暗影格斗3 run device_ready

# 4) 拉官方原包（run 而非 next：next 会先去跑 fetch_binge，模拟器没装斌哥包必然失败）
BASH_ENV=$HOME/.nx_shims.sh EXPECTED_SERIAL=emulator-5554 bash 04_scripts/pipeline.sh 暗影格斗3 run fetch_official

# 5) 本地独立复核（不依赖脚本）
sha256sum '02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk'
md5sum   '02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk'
stat -c '%s' '02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk'
unzip -l '02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk' 'lib/arm64-v8a/libil2cpp.so' 'assets/bin/Data/Managed/Metadata/global-metadata.dat'
unzip -p <APK> 'lib/arm64-v8a/libil2cpp.so' | md5sum
unzip -p <APK> 'assets/bin/Data/Managed/Metadata/global-metadata.dat' | md5sum
source 04_scripts/_common/env.sh; "$NIXIANG_APKSIGNER" verify --print-certs <APK>

# 6) 设备侧独立复核
adb -s emulator-5554 shell "stat -c %s '<remote base.apk>'"
adb -s emulator-5554 shell "sha256sum '<remote base.apk>'"
```

拉取耗时：主拉取 19:29:30 → 19:30:59（≈89 s，含 preflight + 2 次 `unzip -tq` + 3 个哈希 + 设备侧 sha256）；第二次（复用已有文件，只做校验与写 json）19:32:14 → 19:32:58（44 s）。
传动方式：`adb pull` 成功，**未触发** `exec-out su -c cat` 回退。

---

## 4. 指纹对比（归档 vs 本次）

| 项 | 归档（`official_apk.json`，2026-09 分析） | 本次（2026-11-24 从 `emulator-5554` 拉取） | 判定 |
|---|---|---|---|
| size (B) | 2,713,267,683 | 2,713,267,683 | **PASS** |
| md5 | `7ac7591436e92136f22b715c91b5b930` | `7ac7591436e92136f22b715c91b5b930` | **PASS** |
| sha256 | `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087` | `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087` | **PASS** |
| 设备端 sha256（独立取） | — | `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087` | 与本地一致 **PASS** |
| 签名者 DN | `CN=sagely, OU=atr, O=szu, L=sz, ST=gd, C=cn` | `CN=sagely, OU=atr, O=szu, L=sz, ST=gd, C=cn` | **PASS** |
| signer_sha256 | `b147f6a485dfaa6dcb9154e5e5190d342264b0568566749df0aa38cfe55e76e3` | 同值 | **PASS** |
| versionName / versionCode | 1.45.3 / 907 | 1.45.3 / 907 | **PASS（版本号未变）** |
| installer | `com.android.packageinstaller`（上次装机方式不同） | `com.taptap` | 差异项，非包内容差异 |
| zip 完整性 | `unzip -tq` 通过 | `unzip -tq` 两次通过 | **PASS** |

---

## 5. 结论

**雷电（`emulator-5554`）上安装的 `com.wanda.sf3.wd` 与 2026-09 分析的 1.45.3(vc907) 是逐字节同一份。**
size / md5 / sha256 / 签名者 / signer_sha256 / 版本号 六项全 PASS，无任何差异项（installer 字段差异只反映装机渠道，与包内容无关）。

推论（可直接用于后续工作）：

1. **版本号未变**：仍是 1.45.3 / versionCode 907。
2. **此前的静态挂点（file offset / RVA）继续有效**：`libil2cpp.so` 与 `global-metadata.dat` 与服务端字节相同（归档挂点基于此两文件的偏移），无需重新定位。
3. 硬约束 A3 的"官方零改动"基线在这台模拟器上成立：`libil2cpp.so` md5 `53345cdb…`、`global-metadata.dat` md5 `809ec525…`，与硬约束记录逐字一致。

---

## 6. 给下一步静态分析的输入清单

| 项 | 值 |
|---|---|
| APK 绝对路径（Windows） | `E:\nixiang\02_apks\暗影格斗3\暗影格斗3_1.45.3_taptap_official_from_device.apk` |
| APK 绝对路径（MSYS/Git Bash） | `/e/nixiang/02_apks/暗影格斗3/暗影格斗3_1.45.3_taptap_official_from_device.apk` |
| APK md5 | `7ac7591436e92136f22b715c91b5b930` |
| APK sha256 | `1b17ab1c1503dbf5eee3aeb1917948e1c5e80f3ce0b3c3a6c4d682966d166087` |
| APK size | 2,713,267,683 B |
| 条目 1 | `lib/arm64-v8a/libil2cpp.so` — uncompressed 127,366,992 B；md5 `53345cdb4d31dff336fdd575fb87b667`；sha256 `3f28d8a61ee3ad6f49b9271b97799c6eacee618be438ab41d095176e41893080` |
| 条目 2 | `assets/bin/Data/Managed/Metadata/global-metadata.dat` — uncompressed 26,448,516 B；md5 `809ec525f0875c2092dc7f21e73fb288`；sha256 `af8d4cc7e1e31acdcf18440f1dab21b879dc3ccc20c6deb198d35d8f29fdf214` |
| 条目时间戳（可作交叉验证） | 两文件 zip 内时间均为 2026-06-20（21:51 / 21:46） |

---

## 7. 机器状态变更与坑（Windows / MSYS）

### 7.1 本轮引入的环境 shim（需知悉，可撤销）

`04_scripts/暗影格斗3/pipeline.json` 里的路径是 mac 绝对路径 `/Users/aliang/Documents/nixiang/...`。本机的对应垫片被建在了 MSYS 根 `C:\Program Files\Git\Users\aliang\Documents\nixiang`，而 `pipeline.sh` 的 python 助手按 Windows 语义把 `/Users/...` 解析成 `E:\Users\...` ⇒ **所有 `verify` / 产物指纹 / 陈旧度判断全部失败**（首跑 device_ready 报"产物校验未通过"即此因，与脚本本身无关）。

本轮新建了 NTFS junction `E:\Users\aliang\Documents\nixiang` → `E:\nixiang`（`check_win_compat.sh` 文件头已把这类 junction 列为既定豁免机制）。撤销命令：
```
cmd /c rmdir "E:\Users\aliang\Documents\nixiang"     # 只删 junction，不删工作区
```
不建该 shim 时，任何游戏在本机跑 `pipeline ... run/verify` 都会因 mac 路径解析失败而卡住。

### 7.2 pipeline.json 状态变化（机器写，非手改）

- `device_ready`：done（`.device_serial` = `emulator-5554`，md5 `3e4571c1…`）。
- `fetch_official`：done，`done_at=2026-11-24 19:32:54`，artifacts md5 = `7ac75914…`。
- `fetch_binge` 自动化回退 pending（上游 `.device_serial` 变了）；同一波自动回退也把 `build_so / sign / pre_install_gate / install` 打回 pending 并清掉它们的 `inputs` 快照。这是管线的设计行为（上游产物变化 ⇒ 下游作废），但这些阶段的实际产物（`08_output/*`、`07_patched/*`）本轮**未被改动**；要恢复旧戳需重跑对应阶段或由用户决定。**未手改 pipeline.json / pipeline.lock。**
- `archive` 阶段保持原本的 failed（本轮未触碰）。
- 副作用：`pipeline.json` 被 python 以 Windows 文本模式写回，工作副本变成 **CRLF**（git 会在下次提交时归一化为 LF，diff 仍只见状态字段变更）。

### 7.3 坑清单（本机实测，供后续会话直接引用）

1. **裸 bash 无 `md5` / `shasum`，GNU `stat` 不支持 `-f`**：`~/.nx_shims.sh` 只在交互 shell 或设了 `BASH_ENV` 时生效；`pipeline.sh` 用 `bash -c` 跑阶段，函数垫片靠 `export -f` 传递，若父进程没加载则子进程也没有 ⇒ `pipeline.sh` 自己的指纹自检会直接 die（`实际指纹: <计算失败>`）。本次统一用 `BASH_ENV=$HOME/.nx_shims.sh` 前缀解决。
2. **MSYS 改写 argv 里的远端路径**：`adb pull /data/app/...` 会把远端路径改成 `C:/Program Files/Git/data/app/...`。必须 `MSYS_NO_PATHCONV=1` + 本地目标 `cygpath -m`。`adb shell "cmd '/data/...'"` 这种"整串作为一个参数"的形式不受影响。
3. **`exec-out` 回退在 Git Bash 会注入 CRLF**：实测 `exec-out su -c "cat '/system/build.prop'" > f` 比设备端大 144 B（`\r\n`）。所以 2.7 GB 包**不能依赖这条回退**（会被 sha256 抓成失败），本次主路径 `adb pull` 正常，未触发。
4. **`[ -x ]` 对 `.bat` 恒假**：apksigner 在 Win 上是 `apksigner.bat`，旧代码因此静默跳过签名者读取与斌哥包拦截。已按 `_common/env.sh` 口径改 `[ -f ]`。
5. **后台长任务会被工具调用超时连带杀掉**：首次把 `fetch_official` 放 nohup 后台，主调用超时后后台 `adb pull` 一并被杀（传输停在 1,711,669,248 B）。改为前台单次调用（本地模拟器 ≈110 MB/s，整阶段 89 s）后正常。
6. **`next` 不适合本场景**：`next` 按阶段序会先跑 `fetch_binge`（斌哥包，模拟器未安装 ⇒ 必然失败），所以两条阶段都用 `run <stage_id>` 显式执行。
7. `04_scripts/暗影格斗3/official_apk.json` 被脚本重写（这是它的设计行为）：`serial`=`emulator-5554`、`installer`=`com.taptap`、`remote_path` 换成模拟器路径、`apk_path` 变成 `C:/Program Files/Git/Users/aliang/...`（MSYS 垫片形式，可直接用）。size/md5/sha256/signer 与归档一致。

### 7.4 未取到 / 未做

- 未做任何装机、卸载、清数据、启动游戏（全程无 `install/uninstall/pm clear/am start`）。
- 未验证游戏在模拟器上能否启动（超出本轮范围；硬约束 A4 的"连续 3 次冷启"是过验签改动的验收基线，与本轮无关）。
- 未取 `lib/arm64-v8a/` 下其它 so（`libaccess.so`/`libcpp.so`/`libcore.so`）的 md5：本轮未要求，需要时用同一条 `unzip -p | md5sum` 取。
