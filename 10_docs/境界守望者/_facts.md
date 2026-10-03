# 境界守望者 事实台账（`_facts.md`）

> 用途：只记**已验证的硬事实**，供后续会话在上下文被压缩后快速恢复。
> 规则：每条必须标来源；未验证的写进「待验证」，不要混进「已定案」。
> 建立：2026-10-04（星光改造 e1+e2 收尾后）。

---

## 1. 结论一句话

国服星空版框架已复刻到国际服 `com.ap.watcherofrealms` 1.6.38.829.1：**e1（去签名 + 菜单进游戏）+ e2（无敌 / 伤害倍率）+ C1（设备侧 `wor_cfg.txt` 配置化）+ C2（`WOR_DIAG_FULL` 诊断两档）均真机验收通过**；正式交付包为**单 APK 无 root 直装**。
已知限制：服务端战斗校验（`FightVerify` 子系统）——受校验玩法内该局不结算。

---

## 2. 产物指纹（来源：真机 `adb install` 回读 + `md5sum`/`sha256sum` 实算）

| 产物 | 大小 | md5 | sha256 |
|---|---|---|---|
| **单 APK 正式交付包** `08_output/境界守望者/境界守望者_1.6.38.829.1_starry-single.apk` | 206,066,487 B | `e7fcfa4565e756329c18ca39bedb98a2` | `a707b39aeb003fd0c0065987fb5f3ad02e78add768a1ec8097f5548618e2338c` |
| 开发形态 base `..._starry-releaseSigned.apk` | 173,181,186 B | `c80726911ddbfdbfc247abd9154d9a38` | — |
| 开发形态 split `..._starry-splitSigned.apk` | 33,393,563 B | `abd232c22858887094c6d5ab8581f0d5` | — |
| 星光 so·**正式档 RELEASE**（构建产物三副本一致）| 2,217,480 B | `7b754b2c1e1d594a1afe3b03a400ad79` | — |
| 星光 so·排障档 FULL | 2,240,856 B | `904d0673cebbb92470cfdb0c77550d61` | — |

- **单 APK 内 `lib/arm64-v8a/libstarry_magic.so` 的 md5 已实测 = `7b754b2c…`**，与契约路径 `mod菜单/starry-mod-menu/libs_wor/arm64-v8a/` 一致（2026-10-04 核对）。
- 签名证书 SHA-256：`83c5cc0be218d046b3b7af0d2a9f8236297fe228171af2974b71bac130736f1d`（DN `CN=StarryMagic, OU=Starry, O=Starry, L=Shanghai, ST=Shanghai, C=CN`）
- 原包签名（`WOR_CERT_B64`）：DER 708 B / RSA-2048 / SHA1withRSA / MD5 `714d7927b0e8041ee55a40eaefa56adb`
- 构建 tag：`WOR_BUILD_TAG "wor-e2-20261004a"`（`Games/WorMod.cpp`，**行号会漂移，用符号名 grep**）
- 历史产物（2026-10-03，已被上表取代，仅供对照）：单 APK 206,074,679 B / md5 `88f5d479…`；so `c67ab0c0…` 2,233,888 B / tag `wor-e2-20261003c`

## 3. 设备/包信息

- 测试机序列号 `96f0d883`；包名 `com.ap.watcherofrealms`；`versionCode=16388291`、`versionName=1.6.38.829.1`、`minSdk=26`、`targetSdk=36`、`primaryCpuAbi=arm64-v8a`
- 单 APK 装机后：`splits=[base]`，`codePath/lib/arm64` 下 **36** 个 `.so`（35 官方 + `libstarry_magic.so`）
- 设备侧证据目录（可读可写可 pull）：`/sdcard/Android/data/com.ap.watcherofrealms/files/`
  `/data/data/com.ap.watcherofrealms/` **不可读**；`run-as` 报 not debuggable
- 关键输入（`06_dumped/境界守望者/base_1.6.38.829.1/`）：`liblogic.bytes` 137,648,336 B、`global-metadata.dat` 18,184,372 B（version 24 / magic `0xFAB11BAF`）

## 4. 三个卡点（定论，来源：`正式包_单APK合并_20261003.md` §10 / 功能阶段文档 §10）

1. **壳劫持 soname**：split 内 232,520 B 的 `libil2cpp.so` 转发壳与真 `liblogic.so` **同 DT_SONAME**；`Api::Load("libil2cpp.so")` 命中壳并把 `api.ready` 提前点亮 ⇒ 27 个指针全落在壳里。
   → 修：`WorForceApiFromRealLib()` 用 maps-path 真句柄 `dlsym` 覆盖 **28** 个 `il2cpp_*` 成员（`Games/WorMod.cpp`）。
2. **`Api::ImageOf()` 判空**：`Unity/Il2CppApi.cpp:895-909` 的 `IsValidHeapPtr()` 拒绝 Android 14+ tagged 堆指针（`0xb400…`）⇒ 63 个程序集全判空。
   → 修：`WorImageOf()` 优先走真导出 `assembly_image`，失败退回 `asm` 自身；**不再直读首字段**。
   （SELFTEST 实证：`rawFirst == export`，`mscorlib.dll cnt=1361`；`asm自身` 候选 `cnt=116 name='i???t'` 不可用。）
3. **值类型 +0x10 偏置**：EN 环境 `il2cpp_field_get_offset` 对 `stHpChangeData` / `eRelationData` **统一多报 `0x10`**（HybridCLR 伪字段区）。
   → 修：`ApplyRuntimeOffsets()` 对账后**强制恢复国服真值**，打日志 `HPD 值类型 +0x10 偏置已纠正`。
   否则 `fValue` 读在 `show/eShowText` 上、`raw` 恒正、写闸门 `raw<0` 永不通过。

另修：`EnsureLayout()` 两道硬闸门（①库已映射 ②`asmN>=6 && elapsed>=1500ms`）+ `EnsureThreadAttached()` 后移——早于 `il2cpp_init` 调 `il2cpp_thread_attach` 会 `abort()`（曾致无限重启）。

## 5. 字段偏移（来源：`Games/WorMod.cpp:164-181` 声明值 + 真机对账日志）

`WOR_ENV_STAGE 0`（`WorMod.cpp:137`）。

| 变量 | 值 | 字段 |
|---|---|---|
| `g_off_SYS_COM_HP` | `0x50` | `HpChangeEntitySystem.com_hp : HPChangeComponent` |
| `g_off_SYS_HDATA` | `0x60` | `HpChangeEntitySystem.hData : stHpChangeData` |
| `g_off_SYS_MATK` | `0xD0` | `HpChangeEntitySystem.mAtkEntity : Entity` |
| `g_off_SYS_FDAMAGE` | `0xD8` | `HpChangeEntitySystem.fDamage : Fix64` |
| `g_off_HPCOMP_LVALUES` | `0x20` | `HPChangeComponent.lValues : List<stHpChangeData>` |
| `g_off_HPD_FVALUE` | `0x00` | `stHpChangeData.fValue : Fix64` |
| `g_off_HPD_SRC` | `0x08` | `eDamageSourceType` |
| `g_off_HPD_SETTLE` | `0x0C` | `eDamageSettleType` |
| `g_off_HPD_SHOW` | `0x10` | `eTextShowType` |
| `g_off_HPD_ATK_EUID` | `0x1C` | `eRelationData.eUid` |
| `g_off_HPD_ATK_CUID` | `0x20` | `eRelationData.cUid` |
| `g_off_HPD_ATK_UNIT` | `0x24` | `eRelationData.iUnitID` |
| `g_off_HPD_SKILL` | `0x28` | `iAttackSkillID` |
| `g_off_HPD_FORMULA` | `0x2C` | `iFormularID` |
| `g_off_ENTITY_MUID` | `0x10` | `<m_uID>k__BackingField` |

- `FIX64_ONE 4096.0`（Q20.12，`WorMod.cpp:196`）；写闸门 `RAW_SANE_MAX (1LL<<44)`（`:201`）
- 运行时解析：`OffOfField()` / `OffOfFieldChain()`（`WorMod.cpp:~1544-1642`）
- 对账结论（真机）：`SYS_MATK rt=0xD8 cn=0xD0 DIFF`、`SYS_FDAMAGE rt=0xE0 cn=0xD8 DIFF`、`HPD_FVALUE rt=0x10 cn=0x00 DIFF`…；`LIST_ITEMS/LIST_SIZE/HPCOMP_LVALUES/ENTITY_MUID` **match**；`ARRAY_LEN` 运行时返回 `0xFFFFFFFF` ⇒ keep-cn `0x18`
- ⚠️ **禁止**把带 `+0x10` 偏置的运行时值写回 `g_off_*`（历史 bug）

## 6. 符号/运行时事实（来源：真机 `wor_tables_r5.txt` / `[Wor]` 日志）

- `Api` 结构体偏移：`domain_get +0x20`、`thread_attach +0xC0`（反汇编认帧得出）
- 程序集总数 **63**；`#61 Assembly-CSharp.dll cnt=7300`、`#62 Assembly-CSharp-firstpass.dll cnt=1626`、`#0 mscorlib.dll cnt=1361`
- `HpChangeEntitySystem` 命名空间 `CSharpBattle.Battle`；`probe short Entity -> HIT`
- `HpChangeEntitySystem` 全字段 dump：`com_hp 0x50` / `hData 0x60` / `mAtkEntity 0xD8` / `fDamage 0xE0` / `lDamageResults 0x1C8`
- `stHpChangeData` dump 值**带 +0x10 偏置**：`fValue 0x10`(真 0x00) / `eDamageSourceType 0x18`(真 0x08) / `eDamageSettleType 0x1C`(真 0x0C) / `eTextShowType 0x20`(真 0x10) / `iAttackSkillID 0x38`(真 0x28) / `iFormularID 0x3C`(真 0x2C) / `AtkRelationData 0x2C`
- `HPChangeComponent`：`lValues 0x20` / `bSuicide 0x28` / `bSuicideNoShow 0x29`
- 布局闸门常量：`g_cfgMinAsm`（默认 6）、`g_cfgMinElapsedMs`（默认 1500）—— 2026-10-04 起由 `wor_cfg.txt` 的 `layout_min_asm` / `layout_min_ms` 覆盖（原 `WOR_LAYOUT_MIN_*` 已删）
- 命名坑：`HPChangeComponent` **大写 HP**；`Entity` 无名前缀（`CSharpBattle.Battle.Entity` = 0 命中，**不可按名查找**，走 `mAtkEntity` 字段类型链）；`comHp`/`ComHp` = 0 命中

## 7. 真机验收实证

### 7.1 2026-10-03（tag `wor-e2-20261003c`，split 形态）

- 桥接自检：`[桥接] RegisterNatives 19/19 全部成功`、`StarryBridgeTest: === 桥接自检结束: PASS=15 FAIL=0 ===`
- 写回：
  ```
  [Wor] INV e=0x73250db2d0 uid=9 raw=-446464->0
  [Wor] DMG x5 e=0x73250dbdc0 uid=6 raw=-671744->-3358720
  [Wor] DMG x10 e=0x732a1ff410 uid=32 raw=-806912->-8069120
  ```
- 稳定性：pid 全程不变、200s 无 `FATAL`/`SIGABRT`/`SIGSEGV`
- 日志：`06_dumped/境界守望者/device_logs/wor_e2_20261003_230844_{raw,filt}.log`；截图 `screenshots/wor_single_apk_20261003_235539.png`

### 7.2 2026-10-04（tag `wor-e2-20261004a`，**正式档 RELEASE** 单 APK，C1+C2 落地后）

- C1 生效（值均为配置文件里故意写的**非默认**测试值 ⇒ 不是假通过）：
  ```
  [Wor] cfg: /sdcard/Android/data/com.ap.watcherofrealms/files/wor_cfg.txt ok=19 bad=0
  [Wor] cfg: diag=0 probe_max=33 raw_sane_max=8796093022208 min_asm=6 min_ms=1600 scan_cls=40000 scan_sec=60 tier=diag-tier=RELEASE
  [Wor] cfg-off: 12 项由 wor_cfg.txt 覆盖（优先于内建/国服真值）
  ```
- C2 生效：`[Wor] lib ready (liblogic.so, feature stage) hooked=0 tag=wor-e2-20261004a diag-tier=RELEASE`
- 功能（RELEASE 档**无 `P#` 探针行**，仍能判定生效）：
  ```
  [Wor] DMG x5 e=0x744b5bbf50 uid=6 raw=-1142784->-5713920     ← 精确 ×5，共 35 条
  [Wor] INV e=0x7455fb01e0 uid=9 raw=-860160->0                ← 我方承伤清零，共 10 条
  ```
  倍率整除验证：`-1142784→-5713920` / `-1114112→-5570560` / `-991232→-4956160` / `-868352→-4341760` / `-565248→-2826240` / `-442368→-2211840`
- 稳定性：pid `20616` 全程不变，`FATAL EXCEPTION`/`SIGSEGV`/`SIGABRT`/`tombstone` **全 0**
- 日志：`…/wor_20261004_015151_raw.log`（DMG 35 条）、`…/wor_20261004_020414_raw.log`（INV 10 条）
- **运行分支语义**（`WorMod.cpp` `isFriendly = (twr==1||cs==1)`，`if(wantInv&&rawSane&&isFriendly)` / `else if(mul>1&&rawSane&&mon==1)`）：
  二者是 `if/else if` ⇒ **倍率调到 5 时我方常被秒不到、`INV` 天然 0 条**。验无敌要**把倍率调回 1**、只留无敌开。
- **时序假象（勿当真）**：`lib ready … hooked=0` 与 `feature stage: 运行时布局未解析(hData miss) — 拒绝装机` 都是**首帧**打的，布局要 ~1.6 s 后由 `Wor_Tick` 重试路径才解析完。判定以 `runtime layout end … hdataRT=1` + `hasComponent bound` 为准。

## 8. 已知限制：服务端战斗校验

- 症状：开启功能后战斗结束弹「战斗校验结果异常」，**该局不算数、不给奖励**；关掉功能则不弹（用户实测）
- 定位：`FightVerify` 子系统 —— `CreateBattleVerifyFrames`（客户端录校验帧上报）→ 服务端 `ReplayEngine` 独立重放
- 结论：**容差为 0**，客户端改多少都会被重放比出来；缩小倍率无效
- 处置：功能保留在菜单，受校验玩法内不可用；用户决策「不做，就到这」

## 9. 关键路径速查

- 模块源码：`mod菜单/starry-mod-menu/app/src/main/jni/Games/WorMod.cpp`（core）、`WorMod.h`、`WorEntry.cpp`（**不改**）、`Android_wor.mk`
- 设备侧配置：`/sdcard/Android/data/com.ap.watcherofrealms/files/wor_cfg.txt`（C1，全键表见 `境界守望者_C1设备侧配置化与C2诊断分档_20261004.md`）
- 出包链：`04_scripts/境界守望者/wor_build_so.sh`（`DIAG=0|1` 选 C2 档）→ `surgical_wor_1.6.38.829.1.sh` → `wor_sign.sh` → `wor_install.sh`；合并单包 `wor_merge_single.sh`；真机冒烟 `wor_launch.sh [秒数|gate [秒数]]`
- **构建输出目录**：`$NIXIANG/.wor_build/{obj,libs}`（纯 ASCII，仓库根）⇒ 编完 `cp` 回填 `mod菜单/starry-mod-menu/libs_wor/arm64-v8a/libstarry_magic.so`。
  **绝不可把 `NDK_OUT` 指向 `mod菜单/…` 或经 `starry_link/`**：MSYS 会把 junction 解析回真实中文路径（`cygpath -w /e/nixiang/starry_link/obj_wor` → `E:\nixiang\mod菜单\starry-mod-menu\obj_wor`），make 与 clang 编码处理不一致 ⇒ 一边建目录、另一边写 `mod<乱码>/`。
- 台账：`10_docs/_state/games/境界守望者.md`、`10_docs/_state/current.md`

## 10. 待验证 / 未定

- 「战斗校验结果异常」是否仅部分玩法触发（未探测 `FightVerifyType_*`）
- ~~`wor_cfg.txt` 设备侧配置化（C1）与 `WOR_DIAG_FULL` 开关（C2）落地后的真机行为~~ → **2026-10-04 已真机验收通过**，见 §7.2 与 `境界守望者_C1设备侧配置化与C2诊断分档_20261004.md`
- 尚未验证：`wor_cfg.txt` 的 `off.*` 偏移覆盖**改成错值**时是否真能改变行为（本轮 12 项都写的是正确值，只证明了「值被采用」而非「值被使用后行为改变」）

## 11. 工具链债（跨游戏，仅登记不立项）

- **`$VAR` 紧跟非 ASCII**（bash 3.2 下 abort / 无 `set -u` 时静默丢值+乱码）。探测正则：
  `\$[A-Za-z_][A-Za-z0-9_]*[^\x00-\x7F]`
- 本轮已修 **14 处 / 5 文件**（全部在 `04_scripts/境界守望者/`）：`surgical_wor_1.6.38.829.1.sh` 4、`wor_sign.sh` 4、`wor_install.sh` 3、`wor_launch.sh` 2、`wor_build_so.sh` 1。修法统一为 `${VAR}`。
- **全仓仍有 109 处**（代码行 101，涉及 67 文件，已扣除已修 14 处）。**用户决策（m03208）：「不做全仓同模式，后面挨个挨个改就行了」** ⇒ 不批量整改，遇到再顺手改。批量替换时注意区分代码行 / 纯注释行 / 字符串字面量（本轮有一处落在纯注释行）。
