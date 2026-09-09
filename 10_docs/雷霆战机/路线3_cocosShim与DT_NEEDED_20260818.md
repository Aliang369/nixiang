# 路线3 续测：cocos shim / DT_NEEDED（2026-08-18）

> 承接 `路线3_纠偏与未试假说_20260817.md`；H1 用户确认「十余秒可点可登录」后的跟进。

## 结论摘要

| 实验 | 改动 | 结果 |
|------|------|------|
| H1 | 只翻 `libcocos` 末 1 字节 | **活 ~10s+**，可点可登录（用户确认） |
| H1follow shim | REPLACE `libcocos`→薄包装，`dlopen` 原 cocos | shim/starry 能进；`CocosActivity.onCreateNative` **ULE**（JNI 未绑 PathCL） |
| sysload / load0 | 同 shim，改 `System.load` / `Runtime.load0(Class,)` | load 成功；**GameActivity SIGSEGV**（shim 占 `libcocos` 名，真 cocos 旁路加载） |
| cocos-only diag | shim 只 load0 原 cocos、不起 starry | 仍 SIGSEGV → **不是 starry 问题，是双份 cocos 身份问题** |
| H3 DT_NEEDED | 原 cocos + `patchelf/lief --add-needed libstarry_boot` | constructor 能跑；**毫秒级** `tprt←cocos` SIGSEGV |
| H3c empty boot | 同上，boot 仅打日志 | 同上即死 → **改 cocos `.dynamic` 即死** |
| H3d | **stock cocos** + **仅 ADD** `libstarry_boot.so`（无 DT_NEEDED） | **AppActivity 正常，~10.6s 后 SIGSEGV**（与 H1 窗同量级） |

## 钉死

1. **不能**用「REPLACE `libcocos` 为 shim + 再 load 真 cocos」——GameActivity 依赖「`loadLibrary("cocos")` 的那一份」就是真引擎。
2. **不能**改 `libcocos` 的 `DT_NEEDED` / `.dynamic`——`tprt` 在加载期秒杀（与 H1 的延迟窗不同）。
3. **可以** ADD 新 `lib/*.so` 文件名：不是「装上就秒死」，而是 **~10s 延迟窗**（旧文档「ADD lib→死」需改成「延迟死，窗内可用」）。
4. `libcocos` dynsym **混淆**（空名 + 异常 st_value）；字符串里是 `ELF_HOOK_*_JNI_OnLoad`，静态找真 `JNI_OnLoad` 地址不直接。

## 产物

- `08_output/雷霆战机/雷霆战机_1.25.31_H1-cocos1byte-releaseSigned.apk` — H1 对照
- `…_H3c-emptyBoot-lief-releaseSigned.apk` — DT_NEEDED 即死
- `…_H3d-addBootOnly-releaseSigned.apk` — ADD boot、~10s 窗（当前若未改装则在机）
- 源：`04_scripts/雷霆战机/ltzj_cocos_shim.c`、`ltzj_starry_boot.c`

## 下一刀（按优先级）

1. ~~**H3 / service.json**~~ → **否**：PathCL 无 `SDKInterface` 实现可挂；纯 ADD `service.json` 无效。
2. ~~**H2c**：Manifest 等长改 `ZEUS_PLUGIN_GBSDK_DEBUG` → `minPluginVersion:25000`~~ → **存活**，但 **BootApp 仍未执行**（Zeus 未装 debug；2026-08-18）。
3. **运行时拿 JNI_OnLoad 地址**（自注入 Trace / 短探针）→ 等长 `.text` 跳板（期望仍是 H1 级延迟窗，禁止动 `.dynamic`）。
4. **已知可用（Root）**：H2d 包 + `ltzj_install_device_autoload.sh`（须 `STARRY_GAME_LTZJ` so；勿用误切的 PCR so）。

### 2026-08-18 续

| 项 | 结果 |
|----|------|
| 误用 PCR so | `JNI_OnLoad` 有日志但无 `[Ltzj]` / MenuService |
| 重编 `STARRY_GAME_LTZJ` + autoload | `MenuService created` + `auth game_id=ltzj` |
| H2b/H2c 缺 `assets/certs/starlight.crt` | AuthClient `FileNotFoundException` |
| H2d | 补 cert；零 Root 冷启菜单仍未打通 |

### H2e（2026-08-18）— COMMON internalPath 等长改道

改动：
- 官方 `assets/gbsdk.plugin.common.apk` **字节不动**
- ADD `assets/gbsdk.plugin.starry.apk`（`package=gbsdk.plugin.common` + BootApp + LTZJ so）
- Manifest `ZEUS_PLUGIN_GBSDK_COMMON` 的 `internalPath`：`…common.apk` → `…starry.apk`（等长 23）

| 项 | 结果 |
|----|------|
| 存活 | **≥40s+**（无 Root 注入） |
| `LtzjDebugBoot` | **无** |
| MenuService | **无** |

结论：Zeus **不会**用我们的自定义插件 Application 当 COMMON 冷启入口（验签失败静默跳过，或 Tap 根本不装 COMMON Application）。**插件 BootApp ≠ 零 Root 解。**

### 纠偏（对用户）

Root autoload **不是**交付目标；仅可作分析探针。零 Root 下一刀：`libcocos` 等长 `.text` 跳板 + starry 内 `LtzjAceSoft`（官方 4 点内存补丁抢延迟窗）。

### H5 系列（2026-08-18）— cocos DT_INIT cave → starry

| 包 | 要点 | 结果 |
|----|------|------|
| H5b | INIT→cave，oneshot flag 写 RX `.text` | **秒崩** `SEGV_ACCERR` @ cave+0x34 |
| H5c | 去掉写 flag | 活 ~10s SIGBUS；**无** Starry 日志（`GetCreatedJavaVMs` 用 `RTLD_DEFAULT` 在 app ns 失败） |
| H5d | 诊断：dlopen 句柄非空 | 确认 `dlopen(starry)` **成功** |
| H5e | cave 里 `dlopen(libart)` | `art=0`（namespace 挡） |
| **H5f** | cave→`Starry_CaveBoot`；**xDL** 解析 libart `JNI_GetCreatedJavaVMs` + AceSoft | **零 Root 冷启菜单成功**：`MenuService` / `auth game_id=ltzj` / `OVERLAY`；约 **2s** 后 SIGBUS |
| H5g | 同 H5f 但 **跳过 AceSoft** | 菜单仍起；约 **11s** SIGBUS（回到 H1/ADD 窗）；**用户确认可见菜单后闪退** |
| H5h | CaveBoot + 软补 tersafe `sectionhash/filecrc/HashBySyscall` 三处 `blr→mov x0,0`（3/3 ok） | 菜单仍起；约 **10.7s** SIGBUS → **这三刀不够**（与历史 ace12 磁盘结论一致） |
| H5i | + kill GOT stub + IntegritySoft | ~3s **SIGSEGV**（更差）；无 `blocked kill` |
| H5i2 | 仅 kill GOT + SIGBUS handler | ~7–10s SIGBUS；**无** blocked/swallowed 日志 |
| H5i3 | + 劫持 sigaction/signal GOT + watchdog | 同上；**无** blocked sigaction |
| H5i4 | + SA_SIGINFO PC+4 skip（真 bus fault 候选） | ~8s SIGSEGV；**watchdog skips=0**（handler 从未进） |
| H5i5 | + tprt `exit_group(7)` 盲扫 `0x1A0000` + exit/_exit/abort GOT | **秒崩** `SEGV_ACCERR` @ tprt+`0x15f000`（扫进 LOAD 空洞）；未打到 stub |
| H5i6 | 同上但只扫 `PF_X` PT_LOAD（应 ~100 处） | **菜单 + ~8–10s** 后 SIGSEGV；无 `blocked _exit/exit` 残留；与历史 mem-NOP 同窗 → **exit_group 线无效** |

钉死：

1. **零 Root 冷启拉起星光菜单已打通**（CaveBoot + xDL 取 VM）；用户已肉眼确认菜单。
2. 官方 4 点 `LtzjAceSoft` **不能**扛住「改 cocos `.text` + ADD starry」；反而把杀窗从 ~10s **加速到 ~2s**。
3. 仅软补 sectionhash/filecrc/HashBySyscall **不能**挡住该延迟杀。
4. **自杀软补（kill/sigaction GOT + 可捕获 SIGBUS/SEGV handler）目前未命中投递路径**——进程仍死，且 handler 计数为 0。ACE 更可能走 **raw SVC / `_exit` / 不可捕获故障**，或其它进程杀。
5. H5i5 自伤：盲扫越过 tprt RX（`memsz=0x15efa0`）进 gap。H5i6 修扫描后能冷启，但 **~10s 仍死** → exit_group/`_exit` GOT 挡不住该延迟杀。**① 自杀软补线收束**；下一刀转 **(B) 不改 libcocos 的冷启入口**（保 H2d 存活面）。

### 路线②（2026-08-18 晚）— 不改 libcocos / 不 ADD host `lib/`

保 H2d 存活面：host `lib/` 仍 27 个官方 so；星光只在 **assets / 插件 apk**。

| 包 | 改动 | 结果 |
|----|------|------|
| **H2f** | H2d + ADD `assets/plugins-debug.json` + 刷新 `gbsdk.plugin.debug.apk`（BootApp `System.loadLibrary(starry_magic)`，LTZJ so） | **存活不闪退；无菜单**；无 `LtzjPluginBoot` → Zeus **仍未**装/跑 debug |
| **H2f2** | H2d + ADD `gbsdk.plugin.starry.apk`（Application=`tob.common.PluginApplication` + BootProvider）+ Manifest COMMON `internalPath` 等长 → `…starry.apk` | **没菜单**；设备上无 Zeus 解压的 gbsdk 目录 |

假说：H2c/H2e BootApp 未跑，可能缺 `plugins-debug.json`（host 有 `plugins-` / `plugins-cps.json` 拼装痕迹），或 Zeus 对自签插件静默跳过；H2f2 用**官方同名** `PluginApplication` 再试 COMMON 槽。

### 路线② 签名墙（2026-08-18）— 官方壳+我们 dex **不可行**

GBSDK 用的是 **`com.volcengine.zeus`**（host `classes4.dex`，混淆名 `gbsdk.common.host.abxh`），不是 Pangle 的 `com.bytedance.pangle`。装插件必过 `PluginInstaller.checkSignature` → `abxh.f()`：

1. **先验整包签名**：有 v2/v3 块就走 scheme 校验（`abxg.a` 对 APK 内容做 chunk digest）；失败再退 v1。v1（`abxl`）会扫 **所有非 `META-INF/` 条目**，无证书或证书不一致即失败。
2. **再比证书字节**：`Base64.decode(Plugin.mSignature)` 与插件 signer 的 `Signature.toByteArray()` **逐字节相等**（`abzb.a`）。不比宿主 clover/STARRY，也不接受「只换 dex、壳还在」。

`mSignature` 来源：`ZeusManager.initGlobalParamToZeus`（`absw.aE`）对 `gb()` 里每个插件 + debug 调用

`GlobalParam.setSignature(pkg, aCX)`

`aCX` 就是官方 `assets/gbsdk.plugin.common.apk` 的 **CN=union** 证书 DER 的 Base64。离线对照：

| 项 | 值 |
|----|----|
| 官方 common 签名 | v1+v2，signer `CN=union` |
| 证书 SHA-256 | `713ca0ffc462acaebf1c6dd3bb2fb46db00ebd60864a2c4a45a13674bd83e618` |
| 与 host 硬编码 `aCX` | **字节一致**（DER 869B） |
| 官方宿主 | `CN=clover / O=5agame`（另一套证，不参与插件比对） |
| H2d 宿主 | `CN=StarryMagic`（STARRY.RSA，≠ union） |
| Manifest `ZEUS_PLUGIN_GBSDK_COMMON` | **无** `signature` 字段；有也赢不过 GlobalParam 已写入的 `aCX` |

离线突变（`apksigner verify`）：

- 官方 common：**Verifies**（v1+v2）
- ADD 任意 zip 条目 / `zip -u`：v2 块被剥或 digest 坏；CERT.SF 还带 **v2 stripping protection** → **DOES NOT VERIFY**
- 没有 union **私钥**，无法在改 `classes.dex` 后重签回同一张证

因此：

- **自签 STARRY 插件**（H2e/H2f/H2f2）：APK 可能合法，但证书 ≠ `aCX` → `安装包签名校验失败[1]`，Zeus 静默不装。这与「没菜单、无解压目录」一致。
- **官方壳 + 我们 dex**：证书能对上，但改 dex/加条目会坏 v2（及 v1 清单）。META-INF 夹带即使骗过 v1，ClassLoader 也不会当 `classes.dex` 加载。
- **改 host `classes4` 换掉 `aCX`**：等于 REPLACE 宿主 dex → 已知 ACE 死面，禁。

**本刀收束**：不改 libcocos 的 Zeus 插件冷启，被签名墙钉死。保 H2d 存活面；零 Root 出菜单仍需别的入口（不是拼 common.apk）。

产物：

- `…_H5f` … `…_H5i6-exitGroup7RX-…`
- `…_H2f-pluginsDebugJson-…`、`…_H2f2-commonPluginApp-…`
- 源：`Starry_CaveBoot`；`Ltzj_AceSuicideSoft12531` / `Ltzj_AceIntegritySoft12531`