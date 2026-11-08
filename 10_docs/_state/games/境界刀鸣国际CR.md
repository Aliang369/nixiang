# 境界刀鸣 国际服 Crunchyroll 渠道（com.crunchyroll.bleachsoulres）

> 更新：2026-11-08 15:40。**已交付**：星光单包 1.5 GB 已装，**三功能玩家实测通过**（用户原话「功能都没问题」）。
> 与 GP 版（`com.bladetw.bd` 1.48）**同源但不同渠道**：`libgame.so` md5 完全相同。

## 1. 为什么重做（用户原话）

「上次做的是谷歌下载的，别人安装后需要安装谷歌，所以需要换一个新渠道下载重新做」——GP 版依赖 Google Play 服务（PairIP + 字节 GSDK + Play 登录），分发面太窄。改用 APKPure 上的 **Crunchyroll 渠道版**，不依赖 GMS。

## 2. 源包（真机 pull，用户经 APKPure 安装）

`02_apks/境界刀鸣国际CR/`：

| 文件 | 大小 |
|---|---|
| `境界刀鸣国际CR_1.50.212144_official_from_device_base.apk` | 35,536,153 B |
| `…_split_config.arm64_v8a.apk` | 54,847,255 B |
| `…_split_installtime.apk` | **1,470,416,294 B**（资源直接打在里面，装完即玩） |

- 包名 `com.crunchyroll.bleachsoulres`，vc50 / 1.50.212144，minSdk 26 / targetSdk 36
- 官方签名 `CN=Android, OU=Android, O=Google Inc.`（APKPure 重打包），SHA-256 `d2b810ea…`
- APKPure 缓存原件：`_96f0ee07.apk`(264 MB) + `_9d42bf5b.xapk`(1.56 GB)，位于 `/sdcard/Android/data/com.apkpure.aegon/files/download/`

## 3. 与 GP 版的三家对照（同源铁证）

| 项 | 国服 1.39 | GP 1.48 | **CR 1.50** |
|---|---|---|---|
| `libgame.so` | 8,615,736 B | 同 | **8,615,736 B，md5 `9b436aee01fdeab9b7a916e430fe8fa1` 三版全同** |
| lua 挂点 | `gettop=0x515e38` `pcallk=0x518fec` `loadstring=0x51bf04` `pcall=0x512920` `tonumber=0x51292c` | 同 | **同（一字不差）** |
| 加固 | hhld | hhld + PairIP + 字节 GSDK | **无 hhld、无 PairIP、无 GSDK** |
| so 数 | — | 70 | **19**（纯 Unity + `libhades` + `libcrashlytics*`） |
| Application | — | `com.pairip.application.Application` | 默认（`appComponentFactory=androidx.core.app.CoreComponentFactory`） |
| 资源 | 单包 1.99 GB | 首次热更 3.9 GB + 955 MB | **installtime 1.47 GB，无热更** |

⇒ `JjdmMod.cpp` **零改动**复用；上次为 GP 版做的 hhld NOP 在本版**不需要**（零注入重签实测开机不崩）。

## 4. 关键坑：CR 版不自带 `libc++_shared.so`

首版单包装上后星光整条 native 链失效：

```
E/ModBridge: loadLibrary starry_magic failed
java.lang.UnsatisfiedLinkError: dlopen failed: library "libc++_shared.so" not found
  : needed by .../lib/arm64/libstarry_magic.so in namespace clns-5
E/…bleachsoulres: No implementation found for nativeSetApkPath
I/MenuService: auth game_id=（空）
E/AuthClient: native 未加载，无法读取烘焙证书（fail-closed）
```

GP 版 split_config 里有 `libc++_shared.so`（919,600 B），星光 so 一直借用游戏的；CR 版 19 个 so 里没有，必须**随包注入**。
修法：`surgical_jjdmcr_universal.sh` 从 `libs_jjdmintl/arm64-v8a/libc++_shared.so`（1,794,776 B，md5 `ed7aeec92bf60fd9880890040cf5e129`）注入 `lib/arm64-v8a/`，并在成包断言里加必含校验。

另：`StarryInitProvider` 的 `loadLibrary("luster")` 报 `UnsatisfiedLinkError`（W 级、有 catch），不影响功能。

## 5. 产物

| 文件 | 值 |
|---|---|
| `08_output/境界刀鸣国际CR/境界刀鸣国际CR_1.50.212144_三功能_jjdmintl_universal-releaseSigned.apk` | 1.5 GB |
| md5 | `ff7ff809be03aee178b95668aa2c1b63` |
| sha256 | `3a66f5905735278de9333e6be6408e40b97243bd5266079f33be9d56763c4dad` |
| 条目 | 1681（base 1171 条全透传 + 19 so + 486 installtime assets + `classes6.dex` + `libc++_shared.so`） |
| 签名 | `CN=StarryMagic` `83c5cc0b…6f1d` |

出包脚本：`mod菜单/starry-mod-menu/scripts/surgical_jjdmcr_universal.sh`（`INJECT_DEX=classes6.dex`，base 有 5 个 dex）
装机：`adb install -r -d <apk>`（单文件，已实测覆盖三件套与重装均 OK，耗时 3 s 级）

## 6. 装机实测（2026-11-08 15:1x）

```
pm path → 只有 base.apk（单包结构）
[桥接] RegisterNatives 19/19 全部成功
星光魔法 MOD 菜单已加载 (MainModern tag=Jjdm)
native via StarryNativeLoad: …/lib/arm64/libstarry_magic.so
auth game_id synced from native: jjdmintl
pinned cert loaded from native so: CN=47.100.216.101
bootstrap fetch game_id=jjdmintl ← 200 (586 bytes)
native menu shown mode=ACTIVITY
```

- game_id 沿用 `jjdmintl`（用户明确「game_id 用相同的」），与 GP 版共用同一卡密池
- `[Jjdm] lua hook` 暂无输出：游戏停在 `enter_game_login_view`（登录页），未进战斗，Lua 补丁面未触发 —— 非异常

## 7. 功能验证（用户实测，2026-11-08 15:3x）

用户登录进游戏后三项全部生效：「功能都没问题」。
- 菜单浮层正常出现，「伤害倍率 / 无敌 / 无限技能」三项齐全
- 与 GP 版共用 `game_id=jjdmintl`，同一卡密直接可用、无需重输

### 本地产物与关键输入

| 文件 | 说明 | 阶段 |
|---|---|---|
| `02_apks/境界刀鸣国际CR/…_official_from_device_{base,split_config.arm64_v8a,split_installtime}.apk` | 真机拉的官方三件套（35.5 MB / 54.8 MB / 1.47 GB） | 输入 |
| `10_docs/境界刀鸣国际CR/jjdmcr_menu_template.json` | 菜单模板，`template_id=jjdmintl`、`package=com.crunchyroll.bleachsoulres`、3 项 | 输入 |
| `mod菜单/starry-mod-menu/scripts/surgical_jjdmcr_universal.sh` | 出包脚本（注入 `classes6.dex` + 星光 so + `libc++_shared.so`，删 split 属性） | 出包 |
| `10_docs/境界刀鸣国际CR/正式包裁剪清单_20261108.json` | 裁剪清单（保留 universal 正式包） | 最终交付 |

## 8. 复现路径

```bash
# 1) 拉包（真机）
adb shell pm path com.crunchyroll.bleachsoulres   # base/split_config.arm64_v8a/split_installtime
adb pull <各路径> 02_apks/境界刀鸣国际CR/
# 2) 出单包（含星光 + libc++_shared.so）
bash 04_scripts/_common/preflight.sh mod菜单/starry-mod-menu/scripts/surgical_jjdmcr_universal.sh
bash mod菜单/starry-mod-menu/scripts/surgical_jjdmcr_universal.sh
# 3) 装机
adb install -r -d 08_output/境界刀鸣国际CR/…_universal-releaseSigned.apk
```
