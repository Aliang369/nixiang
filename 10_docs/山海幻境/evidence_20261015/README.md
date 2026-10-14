# 山海幻境 4.3/vc69 → 星空版（2026-10-15）决定性证据

> 本轮任务：用户称真机已装 taptap 渠道官方原版最新 APK，要求「更新/替换成星空版，按之前的分析文档重新复刻功能」。
> 结论：**官方 4.3 已替换为星空包并装机成功；装机层与 JS 拼接证据齐**；三项功能的**数值效果**仍需用户 L3 实测（见文末 NOT PROVEN）。

## 1. 样本与产物指纹

| 项 | 值 |
|---|---|
| 官方底包（设备只读提取） | `02_apks/山海幻境/山海幻境_4.3_official_from_device.apk` |
| 官方指纹 | 1,296,756,453 B · md5 `9adc1108637c17326a5845d90f067c3c` · sha256 `f475b7570df15abe85b7af0f87b3c4d32ec8c1a6341c22e02ff824f914d89da6` |
| 官方签名 | md5 `86d1959875555021642fcf3b0d9c161d`（与 3.7/3.8/4.0/4.2 一致）· `CN=shhjchx` |
| 官方引擎 | `lib/arm64-v8a/libcocos2djs.so` 29,176,056 B · md5 `09e19fcd4e4141913d17ffb4836c9634`（**与 4.0/4.2 逐字节相同**） |
| 官方结构 | 3 dex（`classes.dex`/`classes2.dex`/`classes3.dex`）· 9 个 `.jsc` · 无 `libSDKloading.so`/`libtoolchecker.so` |
| 星空包（唯一产物） | `08_output/山海幻境/山海幻境_4.3_starry-releaseSigned.apk` |
| 星空指纹 | md5 `a014ea1f51b0617836a5d0db06139805` · sha256 `bc6302b818db02b5509b2b5278b8f21e8851c18e965455fa49b8d56eb43971ae` |
| 包内 starry so | md5 `8bbc9d43098cde53b8b474ffbbf1491c`（构建产物同值） |
| 签名 | `CN=StarryMagic` · SHA-256 `83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d` · v1/v2/v3 + zipalign ✓ |
| 候选核验报告 | `04_scripts/山海幻境/candidate_verify_4.3.json` |
| 装机结果 | `04_scripts/山海幻境/install_result_4.3.json`（设备 `base.apk` md5 == 本地产物） |

## 2. 决定性证据（各一条就够复判）

### 2.1 官方字节保留（`candidate_verify_4.3.json`）

```
official_common_entries_preserved: 34686
official_common_entries_changed:   []
official_non_meta_removed:         []
unexpected_added:                  []
added: META-INF/STARRY.RSA, META-INF/STARRY.SF, assets/certs/starlight.crt,
       classes4.dex, lib/arm64-v8a/libc++_shared.so, lib/arm64-v8a/libstarry_magic.so
```

### 2.2 战斗面复刻：离线预判 == 真机实测

离线（`04_scripts/山海幻境/check_shhj_js_anchors.py`，按 `ShhjMod.cpp::SpliceCombatJs` 逐字复刻）：

```
HIT  spliced GXR/GXD/GX5 preamble
HIT  spliced getHurt dmg5 (spaced) ×1     # RoleBase.getHurt，index.js:195459
HIT  spliced curHp (spaced) ×1            # index.js:198537
HIT  spliced createNum (spaced) ×3        # 首 index.js:75162
HIT  spliced tHurt inv (spaced) ×1        # index.js:198457
原文 10283645 B → 拼接后 10285142 B（Δ+1497）；node --check PASS
```

真机（预检 logcat，pid 13401，冷启）：

```
Mod_Menu: [Shhj] hooked se::evalString
Mod_Menu: [Shhj] early se hook ok iter=288
StarryMagic: [Shhj] 初始化成功
Mod_Menu: [Shhj] spliced GXR/GXD/GX5 preamble
Mod_Menu: [Shhj] spliced getHurt dmg5 (spaced)
Mod_Menu: [Shhj] spliced curHp (spaced)
Mod_Menu: [Shhj] spliced createNum (spaced)
Mod_Menu: [Shhj] spliced tHurt inv (spaced)
Mod_Menu: [Shhj] splice #1 len 10296473→10297970 file=assets/main/index.jsc     ← Δ 恰为 +1497
```

菜单/授权/桥接链：

```
StarryMagic: [桥接] RegisterNatives 20/20 完成（跳过 dex 未声明 0 个）
StarrySignSpoof: spoof hit pkg=com.chx.shhj md5=86d1959875555021642fcf3b0d9c161d
ModBridge: auth game_id synced from native: shhj
MenuService: native menu shown mode=ACTIVITY
```

崩溃关：`FATAL EXCEPTION|Fatal signal|DEBUGGERD|crash_dump|am_crash` 命中 **0**（空转 3 分钟）。

## 3. 最短复现命令

```bash
bash 04_scripts/pipeline.sh 山海幻境 status                 # 27 阶段（16 个历史 + 11 个 v43_*）
bash 04_scripts/山海幻境/decrypt_jsc_shhj.sh                # 官方 JSC → JS（XXTEA key 376f2502-1ead-46）
python3.11 04_scripts/山海幻境/check_shhj_js_anchors.py \
  06_dumped/山海幻境/official_4.3/jsc_decrypted/assets/main/index.js \
  --out 06_dumped/山海幻境/official_4.3/spliced_main_index.js
# 出包 / 核验 / 装机（阶段命令原文见 pipeline.json 的 v43_surgical / v43_verify_package / v43_install）
bash 04_scripts/_common/post_install_precheck.sh --game 山海幻境 --pkg com.chx.shhj --idle-min 3
```

本轮中间证据（可再生产物，勿入库）：`09_test/山海幻境/precheck_20261015_015848/logcat.txt`。

### 2.3 用户 L3 实测（飞行记录仪 `09_test/_recorder/20261015_020249`，31 分钟）

用户结论（m00002）：「**功能都没问题**」。同段 logcat 支撑：

```
02:32:41 AuthClient: POST https://47.100.216.101:8443/v1/auth/login  ← 200 (1177 bytes)
02:32:41 StarryMagic: [验证] grant ok exp=1792089160
02:32:43 ModBridge: Toggle: #1 Toggle_无敌 = true
02:32:43 Mod_Menu: [Shhj] feature #1=ON inv=1 dmg100=0 dmg5=0
02:32:47 ModBridge: Toggle: #2 Toggle_百倍伤害 = true
02:32:47 Mod_Menu: [Shhj] feature #2=ON inv=1 dmg100=1 dmg5=0
02:32:53 ModBridge: Toggle: #3 Toggle_五倍伤害（BOSS） = true
02:32:53 Mod_Menu: [Shhj] feature #3=ON inv=1 dmg100=0 dmg5=1
```

- 开关次数（`[开关] #N` 计数）：功能#1 = 2 次、功能#2 = 14 次、功能#3 = 13 次；每次 ON/OFF 都同步到 native 标志位。
- 崩溃/ANR：`FATAL EXCEPTION|Fatal signal|DEBUGGERD|am_crash|ANR in com.chx.shhj` 命中 **0**。
- 冷启前的 `[验证] grant rejected` 是**未登录会话**的正常表现；登录后 `grant ok`。全段无 `renew auth fail`。

## 4. NOT PROVEN（不因本轮证据而成立的项）

1. 服务端接受度：世界 Boss / 工会 Boss / 道韵 / 水火 Boss 对放大后 `_allhurt` 的结算与排行 —— 用户只反馈「功能都没问题」，未单独确认排行/奖励结果。**状态＝待定**（用户 m00003：「这个暂时我没有，但是用户在使用没反馈是失效，应该是没问题，就先待定」 ⇒ 无失效反馈，但不升格为已验证）。
2. 百倍与五倍**同时**开启时「普通场景取百倍、不叠加成 500 倍」的实测行为（设计如此，用户未单独反馈该组合）。
3. 长时间连打（>31 分钟）与跨天冷启后的稳定性（本轮观测窗口 31 分钟，无崩溃）。

> 已由用户实测证实的项：无敌免伤、百倍伤害、五倍伤害（BOSS）三项数值效果（m00002 + §2.3 日志）。

## 5. 已知偏离（相对框架审计的通用建议，均按既有 3.8/4.0/4.2 先例保留）

| 审计项 | 结论 | 保留依据 |
|---|---|---|
| `TapLoginForceWeb.PKG_WHITELIST` 未含本包名 | 未改 | 本游戏强制网页登录走的是 smali 直接改写（`ClientLoginUtils.isClientLogin→false`、`ClientLoginHandler.canHandle→false`），4.0/4.2 以该路径登录通过（`201/301 ret=0`） |
| `Android.mk` 激活分支 ≠ SHHJ | 未改（按设计） | so 在**隔离副本**中构建（`build_shhj_so_4.sh`），工作区 mk 零接触；产物已验 `[Shhj]`+`shhj`+4 splice 标签、无其它游戏标签 |
| 入口形态用 `<application android:name=StarryApp>` 而非 Provider | 未改 | 本游戏 3.8/4.0/4.2 均用该入口并实测弹出菜单（本轮 logcat `native menu shown mode=ACTIVITY` 再次证实） |
| `uninstall_official_shhj.sh` 里 `official_apk_backup` 字段写死 4.0 路径 | 已知瑕疵 | 仅记录用；实际底包为 4.3（见 §1） |
