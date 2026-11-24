# 暗影格斗3 — 后台 game_id 从 sf3 改为 aygd3（重出包 + 覆盖安装）

- 时间：2026-11-24 22:36–23:16（Windows 11 + Git Bash/MSYS，`E:/nixiang`）
- 设备：`emulator-5554`（真机 96f0d883 / MuMu 未触碰）
- 目标：`authGameId`（后台 `games.game_id`）由 `sf3` → `aygd3`；`.tag`（日志/菜单内部前缀）保持 `sf3` 不变。

---

## ① 改动 diff

### 1.1 `mod菜单/starry-mod-menu/app/src/main/jni/Games/Sf3Entry.cpp`（唯一代码改动）

```diff
@@ -27,8 +27,9 @@ static const char* kSf3_Features[] = {
 
 extern "C" const StarryGameMod* const GameMod_Get() {
     static const StarryGameMod mod = {
+        // tag ≠ 后台 game_id：tag 只做日志/菜单内部标识前缀（保留 sf3 便于对照历史日志），game_id 见 authGameId
         .tag = "sf3",
-        .authGameId = "sf3",            // 暗影格斗3
+        .authGameId = "aygd3",          // 后台 games.game_id（暗影格斗3 → 官方缩写 AYGD3，与官方签名文件名 AYGD3.RSA 同源）
         .featureList = kSf3_Features,
         .featureCount = (int)(sizeof(kSf3_Features) / sizeof(kSf3_Features[0])),
```

`git diff --numstat` = `2 1`（+2/-1）；jni 目录下无其它文件改动（Main.cpp / MainModern.cpp / GameConfig.h / Android.mk 未碰）。

### 1.2 菜单模板改名 + 改值

- `10_docs/暗影格斗3/sf3_menu_template.json` → `10_docs/暗影格斗3/aygd3_menu_template.json`
- `"template_id": "sf3"` → `"aygd3"`；`name/package/source/features`（三项）原样保留。

### 1.3 文档引用同步

- `10_docs/暗影格斗3/硬约束.md` A-1 行「证据/权威落点」列：`sf3_menu_template.json` → `aygd3_menu_template.json`（唯一一处现行引用）。
- `10_docs/暗影格斗3/_index.md` 由生成器重建：`python3 04_scripts/_common/gen_game_doc_index.py --game 暗影格斗3` → `- aygd3_menu_template.json（292B，2026-11-24）`。
- 历史文件按「不重写历史」保留原文：`10_docs/_state/规范变更日志.md`(2026-10-29 条目)、`10_docs/_state/history/reviews/…` 仍写旧名（历史记录，不作现行指引）。
- 机器关：`bash 04_scripts/_common/preflight.sh <Sf3Entry.cpp> <aygd3_menu_template.json>` → `汇总: PASS=4 FAIL=0 REMIND=1`，`✅ 机器关通过`。

## ② so / 包新指纹

| 层 | 值 |
|---|---|
| 新 so | `e0f31dc163d00bb7d029f75e9985b03f`，192,080 B（上一版 v2：`69e8924c…`） |
| 包内 so（新包抽出） | `e0f31dc163d00bb7d029f75e9985b03f`，192,080 B ⇒ 与 build_so 一致 |
| unsigned | md5 `adad19dacdc198d706db0b43caa127ac`，2,715,072,323 B |
| **成品包** | `08_output/暗影格斗3/暗影格斗3_1.45.3_surgical-releaseSigned.apk`，md5 **`a06cbff00dc7204574f240fa186edbcd`**，sha256 **`3f748503a8356052bd73a85a54ef1c3b3d8e8943450dd758ce299fe0592ab14c`**，size **2,715,627,251** |
| 留证旧包 | `…_surgical-releaseSigned_v2_105190f1.apk`（md5 `105190f1…`）、`…_v1_0bc69378.apk`（md5 `0bc69378…`），均未删除 |

**旁注（如实记录，非本任务改动）**：本次 gradle 重建导致 `classes8.dex` md5 由 `cea19d33…` 变为 `5b6e7cd4…`。字符串表比对：v3 比 v2 多 `com/starry/magic/cocos/Zzbx2Hooks`、`com/starry/magic/games/wdyz/GameHookImpl`、`com.rsg.wdyz`，少一个 R8 混淆名 `com/starry/magic/o/X0`（`mod菜单/…/games/wdyz/GameHookImpl.java` 的 mtime = 22:08，晚于我上一轮 22:07 的出包）。属共享 starry 工程被其它会话更新后被 R8 一起打进 dex；框架隔离门禁 PASS，sf3 行为不受影响（`GameHooks` 按 game_id/包名派发）。

## ③ `aygd3` 串取证

```
$ python: libs_sf3/arm64-v8a/libstarry_magic.so   ('aygd3' count = 1, offset 0x4dc3)
  .rodata[0x4dc3:0x4dc9] = b'aygd3\x00il2cpp'      （紧跟其为 libil2cpp 名字串，无拼接歧义）
$ llvm-readelf -r -W so | grep 4dc3
  000000000002e780  0000000000000403 R_AARCH64_RELATIVE   4dc3
  ⇒ StarryGameMod 静态表里 authGameId 指针确实指向 0x4dc3（即 "aygd3"）
包内复核（python zipfile 抽 lib/arm64-v8a/libstarry_magic.so）：aygd3 count = 1 at 0x4dc3；so md5 = e0f31dc1…
```

**一句话**：`aygd3` 以 1 次字节命中落在新 so 的 `.rodata` 0x4dc3，并被 `.data.rel.ro` 的重定位（0x2e780 → 0x4dc3）作为 `authGameId` 指针引用；`.tag` 仍是 `sf3`（so 内 `sf3` 串 18 次，日志前缀不变）。

## ④ 五库 md5 断言（sf3_sign.sh 原样，未放宽）

```
[sign] 5 库 md5 断言 OK：il2cpp=53345cdb4d31dff336fdd575fb87b667 metadata=809ec525f0875c2092dc7f21e73fb288
       access=82ee8142fa3a5996cb5eb7bd4a43677e cpp=7dad26fcca3bfe85df53a45a046959e9 core=4b0b4e792b2670e8f0f5bab8d47aa08b
```

→ **5/5 PASS**（A-3 游戏本体零改动成立）。

## ⑤ 覆盖安装闭环

`MSYS2_ARG_CONV_EXCL=/data adb -s emulator-5554 install -r -d --user 0 <新包>` → `Performing Streamed Install / Success`（未卸载、未清数据）

| 项 | 结果 |
|---|---|
| 设备 base.apk md5 | `a06cbff00dc7204574f240fa186edbcd`（= 新包；覆盖前为 `105190f1…`） |
| lastUpdateTime | **2026-11-24 22:49:46**（覆盖前 22:11:52，已变化） |
| firstInstallTime | 2026-11-24 21:16:55（**未变** ⇒ 覆盖非重装，数据保留） |
| 设备内 so | `…/lib/arm64/libstarry_magic.so` md5 = `e0f31dc1…` |
| 签名 | apksigner：`CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`（cert sha256 `83c5cc0b…`） |

## ⑥ 冷启复验（force-stop → logcat -c → am start -W → 观察窗 → logcat -d）

| # | 观察窗 | Init | Login(uid) | `[sf3]` | `native menu shown` | `auth game_id …` | 崩溃 | 判定 |
|---|---|---|---|---|---|---|---|---|
| 1（装完首启） | 95 s | 1 | 1 (3009937009) | **0** | 0 | 0 | 0 | 缺项（首启慢，同 v2 包同现象） |
| 2 | 95 s | 1 | 1 | 2 | 0 | 0 | 0 | Init/Login/[sf3] PASS |
| 3 | 95 s | 1 | 1 | 2 | 0 | 0 | 0 | 同上 |
| 4 | 95 s | 1 | 1 | 2 | 0 | 0 | 0 | 同上 |
| 5 | 150 s | 1 | 1 | 2 | 0 | 0 | 0 | 同上（延长窗仍无 auth 行） |
| 6 | 300 s | 1 | 1 | 2 | 0 | 0 | 0 | 同上（延长窗仍无 auth 行） |
| P（precheck 自带启动） | ~180 s | 1 | 1 | 2 | **1** | **5 次 `aygd3`** | 0 | **新增判据达成** |

- **判据 1–4（3/3 Init + 3/3 Login + 3/3 `[sf3]` + 0 崩溃）**：冷启 #2/#3/#4 连续三次达成；#1 是覆盖安装后第一次启动（95 s 内无 `[sf3]`，与上一版 v2 包同现象）。
- **新增判据（`auth game_id synced from native: aygd3`，不能再是 sf3）**：在 precheck 自带冷启日志中命中 **5 次 `…: aygd3`** 且 `MenuService: auth game_id=aygd3`，**`…: sf3` 命中 0 次**（全证据 grep）。
  - 为什么 `am start` 的 95/150/300 s 窗口里没有这行：该行由 `MenuService/NativeMenuController` 在**菜单真正弹出**时调用 `ModBridge.syncAuthGameIdFromNative()` 打出；真冷进程在本机 5 分钟内都没走到 `native menu shown`（`MenuService: overlayVisible=true` 有，`native menu shown` 无）。**同一现象在上一版（sf3）包里也一样**（v2 的 4 次 `am start` 冷启 `auth game_id` 计数 = 0），因此不是本次改动引入的回归，属启动路径/时机属性。
- 证据目录：`09_test/暗影格斗3/install_20261124_aygd3/`（`cold1..cold6_{amstart,logcat}.txt`）、`09_test/暗影格斗3/precheck_20261124_225709/`。

## ⑦ 装机预检

`bash 04_scripts/_common/post_install_precheck.sh --game 暗影格斗3 --serial emulator-5554` → `EXIT=0`

```
- 包名取自菜单模板: aygd3_menu_template.json → com.wanda.sf3.wd     ← 改名后的模板被正确解析
✓ 菜单 so 加载标记: StarryAppFactory: SignatureKiller mounted: PackageInfo.CREATOR replaced, PM/Parcel caches cleared
✓ 菜单弹出标记: MenuService: native menu shown mode=ACTIVITY
✓ 授权通过标记: ModBridge: auth game_id synced from native: aygd3
✓ PASS_TO_HUMAN —— so 加载/菜单弹出/空转 3 分钟无崩溃，装机层证据齐
```

## ⑧ 风险提示（必须给用户看）

1. **新 game_id 需要后台先建 `aygd3` 并把授权发给用户**。预检的「授权通过标记」只证明 native→Java 的 game_id 同步成功（`aygd3` 已生效），**不证明**后台授权链通过；若后端没有 `aygd3` 或未给该用户授权，菜单的授权/登录门可能不放行（表现为菜单能出但功能被拦/提示未授权）。
2. 若后端暂不能加 `aygd3`：回滚只需把 `Sf3Entry.cpp` 的 `authGameId` 改回 `sf3` 重出包，或用留证的 `…_v2_105190f1.apk` 覆盖安装（同签名，可覆盖）。
3. `.tag` 仍是 `sf3`，日志里 `[sf3]` 前缀与历史日志可比对；**看到 `[sf3]` 不代表 game_id 还是 sf3**，判定请只看 `auth game_id` 行。
4. 功能本身（伤害倍率/上帝模式/战斗速度）仍未验收，等用户实测（L3 终审）。

## ⑨ 过程中的处置（如实记录）

- 首次 `surgical` 报 `ERROR: lock /tmp/sf3_surgical.lockdir held`：锁目录 mtime 22:05、内含 pid 22944，`ps -p 22944` 无该进程 ⇒ 判定陈旧锁，按显式路径删除 `/tmp/sf3_surgical.lockdir` 后重跑成功（未改脚本）。
- 我的首次 `monkey` 冷启失败：本机镜像无 `/system/bin/monkey`（`inaccessible or not found`），随后回归 `am start` 标准路径（precheck 自身也是这个降级策略）。
