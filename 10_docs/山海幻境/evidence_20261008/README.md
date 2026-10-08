# 山海幻境 4.2 dex 换新 — 决定性证据（2026-10-08）

> 只列决定性证据行与最短复现命令；完整 logcat 不入库（原始落 `09_test/`，收尾后清理）。
> 本轮唯一目的：把 `02_apks/山海幻境_4.2.apk` 里旧版源码编的 `classes4.dex` 换成
> `mod菜单/starry-mod-menu` HEAD 重编的新 dex，并确认**战斗代码相关功能无误**。

## 产物指纹

| 项 | 值 |
|---|---|
| 正式包 | `08_output/山海幻境/山海幻境_4.2_menu-latest-releaseSigned.apk` |
| 大小 / md5 | 1,236,763,989 B / `d2fb22317170e1dacab75dbdffacb950` |
| 证书 SHA-256 | `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d` |
| 底包 | `02_apks/山海幻境_4.2.apk` 1,236,747,605 B md5 `47eebe7668a6ef133c0621839cc67520` |
| `classes4.dex` 旧 → 新 | 454,892 B `23baec22b821b7bc407723f868a188ce` → 484,180 B `1b55d842b65834f917e9054b5fe4853d` |
| 包内 so（未改） | `lib/arm64-v8a/libstarry_magic.so` 4,225,312 B md5 `3ca6608284c3cf751a3dadfafe50cb77` |

## 证据 1：只有 classes4.dex 变了（L1，出包脚本 [5/7] + 独立复核）

```
[5/7] 仅 classes4.dex 变更，so 与其余 34230 个条目逐字节一致
classes.dex    SAME  md5 66059493a9b35c9ddcfe6f0154f334bf
classes2.dex   SAME  md5 c6d64452a6a399b7f4a3f833ec69505f
classes3.dex   SAME  md5 1a7d8288ed5d0ac1d2f89d9244fb3970
libstarry_magic.so SAME  md5 3ca6608284c3cf751a3dadfafe50cb77
classes4.dex   DIFF  454892 → 484180
```
最短复现：`bash 04_scripts/山海幻境/shhj_menu_latest_4.2.sh`（退出码 0 即过）。

## 证据 2：战斗 Java 面逐指令未变（L1）

`cocos/ShhjHooks` 归一化（`.registers`↔`.locals`、label/寄存器重编号）后逐行 diff：

```
ShhjHooks 归一化逐指令相同（98 行）
```

配套：`Main.cpp` 的 20 条 `regOne` 与新旧 dex native 声明 **缺失 0 / 签名不符 0**；
`pre_install_gate.sh` → `✓ PASS [JNI配套] 20 个动态注册方法名在 dex 中全部有声明`。

## 证据 3：真机战斗挂点全部命中（L2，`f9a116a2`）

来源 `09_test/山海幻境/precheck_20261008_141139/logcat.txt`（pid 22864）：

```
StarryMagic: [桥接] RegisterNatives 20/20 完成（跳过 dex 未声明 0 个，不影响 JNI_OnLoad）
Mod_Menu:    [Shhj] hooked se::evalString
Mod_Menu:    [Shhj] early se hook ok iter=306
Mod_Menu:    [Shhj] se::eval #5 len=1864784 file=src/cocos2d-jsb.js
Mod_Menu:    [Shhj] spliced GXR/GXD/GX5 preamble
Mod_Menu:    [Shhj] spliced getHurt dmg5 (spaced)
Mod_Menu:    [Shhj] spliced curHp (spaced)
Mod_Menu:    [Shhj] spliced createNum (spaced)
Mod_Menu:    [Shhj] spliced tHurt inv (spaced)
Mod_Menu:    [Shhj] splice #1 len 10278614→10280111 file=assets/main/index.jsc
StarryMagic: [Shhj] 初始化成功
```

即游戏主 JS（10.2 MB `assets/main/index.jsc`）**真的被改写 +1497 字节**，四处战斗挂点全中。

## 证据 4：菜单链路 + 用户三功能开关写入 native（L2）

```
MenuService: native menu shown mode=ACTIVITY
ModBridge:   auth game_id synced from native: shhj
NativeMenu:  renew ok (expand)
Mod_Menu:    [Shhj] feature #1=ON inv=1 dmg100=1 dmg5=1     ← 用户手开
Mod_Menu:    [Shhj] feature #2=ON inv=1 dmg100=1 dmg5=1
Mod_Menu:    [Shhj] feature #3=ON inv=1 dmg100=1 dmg5=1
```
三个开关（①无敌 ②百倍伤害 ③五倍伤害（BOSS））都成功写进 native 标志位。
游戏进程 14:11:39 起、14:18:06 止（约 6.5 分钟，`VmRSS≈1.7 GB / 91 线程`），**带三个功能开启无崩溃**。

## 证据 5：预检闸门 PASS

```
✓ PASS_TO_HUMAN —— so 加载/菜单弹出/空转 3 分钟无崩溃，装机层证据齐
```
复现：`bash 04_scripts/_common/post_install_precheck.sh --game 山海幻境 --serial f9a116a2 --idle-min 3`

## NOT PROVEN（不得当成已验证）

1. **三项战斗功能的数值效果**未取得实测：无敌是否真免伤、百倍/五倍倍率是否落在预期数值、
   五倍对 BOSS 的判定（`ActivityWorldBoss||guildBoss||daoyun||waterFireBoss`）是否符合预期
   —— 用户本轮只开了开关，未提供战斗内数值反馈。
2. 服务端接受度与排行结算未测。
3. 旧 dex 的 `DeviceFingerprint` v5 单路径缺陷在本机（`f9a116a2`）**未复现**，
   换新 dex 是消除隐患而非修复已观测故障。

## 本轮踩到的一个坑（已就地规避，供后续优化会话合并进 `_common`）

裸 `SIGSEGV` 会命中 adb shell 辅助进程的 `E libsigchain: Setting SIGSEGV to SIG_DFL`：
实测 3 行分属 3 个不同 pid（22182/22286/22387），**每个 pid 在整份 logcat 里只有这 1 行**，
而游戏 pid 21193 同一时刻仍在正常输出 ⇒ 假崩溃。已在本游戏 `预检标记.txt` 收窄为
`FATAL EXCEPTION|Fatal signal|DEBUGGERD|crash_dump|am_crash` + `crash_ignore|…|libsigchain`。

另：`04_scripts/_common/flight_recorder.sh` `stop` 分支有两处小瑕疵（本轮实测）——
① `NM="$(grep -c '^MARK|' … || echo 0)"` 在空 `marks.log` 下得到 `"0\n0"`，
   第 218 行 `[ "$NM" -gt 0 ]` 报 `integer expression expected`；
② `kill "$SUP"` 后 worker（`__worker` 子进程）未随之退出，需人工补 kill。
