# 无悔华夏 — 斌哥改包提取 / 星光落地

> 包名 `com.yhhy.whrhx.aligames` · v3.8.280 (33) · 九游渠道  
> 真机 OnePlus · `ANDROID_SERIAL=f9a116a2` · 2026-08-26 起手

> ⚠️ **好游快爆版（com.syyx.whrhx，game_id=whhxkb）进行中**：功能 RVA 全部落定，但新版易盾壳签名绑密钥、外科重签必死 → 走脱壳重建路线。**续作入口：[好游快爆版_脱壳重建_进行中_20260831.md](./好游快爆版_脱壳重建_进行中_20260831.md)**（含已完成清单/死路清单/明日步骤）。构建用私有 `Android_kb.mk`（并行会话共用仓库）。

## 当前进度

| 阶段 | 状态 | 说明 |
|------|------|------|
| 斌哥包识别 | ✅ 实锤 | 签名 `CN=binge2.keystore` + `libSDKloading.so` + `libtoolChecker.so` + `bg888@188.com` |
| 九游原包拉取 | ✅ | `02_apks/无悔华夏/无悔华夏_3.8.280_九游原版.apk`（CN=9Game） |
| 斌哥 APK | 🗑 已删 | 误路径 `02_raw_apk/` 已清理 |
| Il2CppDumper（斌哥明文 meta） | ✅ | `06_dumped/无悔华夏/il2cpp_dump/` |
| Il2CppDumper（原包 APK 直读） | ❌ | metadata **HTPX** |
| **原包 runtime metadata** | ✅ | `work/无悔华夏_official/global-metadata_runtime*.bin` |
| **原包挂点实锤** | ✅ | [原包dump实锤_20260826.md](./原包dump实锤_20260826.md)（**旧 RVA 0x4845388 等已作废**） |
| 斌哥菜单 / prefs | ✅ | [prefs对照定稿_20260826.md](./prefs对照定稿_20260826.md) |
| 斌哥 memdiff | ✅ | il2cpp **0 stub**（特征索引失败） |
| **原包玩法 RVA** | ✅ 六项（新表） | [原包dump实锤_20260826.md](./原包dump实锤_20260826.md) |
| Time 加速 RVA | ✅ | `get_deltaTime` × 滑条（早期已验收） |
| Trace / WhrhxMod | ✅ | v11 延迟 hook · 启动零挂点 |
| 启动闪退修复 | ✅ | [闪退修复_20260827.md](./闪退修复_20260827.md)（MenuService + 懒 hook） |
| 真机六项验收 | ✅ | 2026-08-27 用户确认 |
| 3.8.295 热更资源修复 | ✅ | 2026-09-12，九游星空底包；无限金币/食物/军队/人口由用户确认正常 |
| 科研立即完成 | ✅ | 2026-09-12，覆盖 `GameUI.PolicyUI` 与蒙华科技入口；用户确认功能正常 |
| 后台菜单 JSON | ✅ | [**whhx_menu_template.json**](./whhx_menu_template.json)（`game_id=whhx`） |
| 中间 Trace 包清理 | ✅ | 2026-08-27 只留 release；中间 work 已归档 `99_archive/无悔华夏/work_cleanup_20260827/` |
| 宣传图 | ✅ | `12_images/无悔华夏/无悔华夏_宣传图.png` |

## 引擎与保护（静态实锤）

| 项 | 结论 |
|----|------|
| 引擎 | Unity IL2CPP + **xLua**（`libxlua.so` · `LuaEnv.DoString` RVA `0x6B95D84`） |
| 热更 | `Assembly-CSharp.patch.bytes`（非 HybridCLR 主路径） |
| 分包 | 乐变 `LBApplication` + 多 Plugin P00–P09 |
| 加固 | 百度 Sagittarius `StubApplication` + `libbaiduprotect.so` |
| 反作弊 | `libNetHTProtect.so` / `libnesec.so` |
| 斌哥壳 | `libSDKloading.so`（~23MB packed）+ RootBeer `libtoolChecker.so` |

## 文档索引

| 文件 | 内容 |
|------|------|
| [斌哥包识别_20260826.md](./斌哥包识别_20260826.md) | 签名、壳文件、Java 桥、免责声明 |
| [闪退分析_20260826.md](./闪退分析_20260826.md) | 斌哥包 SIGSEGV 根因 |
| [斌哥菜单功能清单_20260826.md](./斌哥菜单功能清单_20260826.md) | 6 项菜单 |
| [prefs对照定稿_20260826.md](./prefs对照定稿_20260826.md) | prefs ↔ get_ret |
| [抓点定稿_20260826.md](./抓点定稿_20260826.md) | prefs + 斌哥/原包 RVA 对照 |
| [原包分析_20260826.md](./原包分析_20260826.md) | HTPX / CodeGen 提取过程（**RVA 表已被 §新文档覆写**） |
| [原包dump实锤_20260826.md](./原包dump实锤_20260826.md) | **官方 metadata + token + RVA 定稿** |
| [**whhx_menu_template.json**](./whhx_menu_template.json) | **后台菜单模板 JSON（导入用）** |
| [热更资源失效修复_20260912.md](./热更资源失效修复_20260912.md) | **3.8.295 四项资源修复 + 最终包指纹** |
| [政策科技研发加速与立即完成_可行性_20260912.md](./政策科技研发加速与立即完成_可行性_20260912.md) | **科研立即完成实现与真机验收** |
| [热更失效修复_20260923.md](./热更失效修复_20260923.md) | **本期**：科研热更失效修复 + 新版菜单 + 卸载重装 |
| [OTA守卫误清补丁修复_20260924.md](./OTA守卫误清补丁修复_20260924.md) | **本期**：OTA 守卫误清补丁双根因修复（各功能 prefs 单开记录已并入对照定稿） |

## 产物路径

当前最终包（2026-09-12）：

```text
08_output/无悔华夏/无悔华夏_3.8.280_科研立即完成_20260912-releaseSigned.apk
APK SHA-256 = 0b6c4772e64a6753b3a3169c9aea3a2e9af435c6361746c83b13516ba3f03167
SO  SHA-256 = 742b366f8a68cc479450db0aac29de75c7ce344a987e317200f54e561aed708d
```

历史 8 月产物路径：

```
08_output/无悔华夏/无悔华夏_3.8.280_surgical-releaseSigned.apk   # 正式包（唯一 APK）
12_images/无悔华夏/无悔华夏_宣传图.png
12_images/无悔华夏/无悔华夏_官方icon.png
10_docs/无悔华夏/whhx_menu_template.json
work/无悔华夏_official/trace10_logcat.txt                      # Trace 验收日志（保留）
work/无悔华夏_official/trace_v9_battle.txt
99_archive/无悔华夏/work_cleanup_20260827/                     # 已归档中间分析文件
```

重建：`surgical_whrhx_3.8.280.sh` 底包优先 `02_apks/…九游原版.apk`，缺失时用 `08_output/…releaseSigned.apk` 自举。

## Round2 完成（2026-08-30 定稿）

**产物**：`08_output/无悔华夏/无悔华夏_3.8.280_surgical-releaseSigned.apk`（release，14:02）+ traceSigned.apk（带诊断，13:50）
**验收**：六功能全通（金币/粮食/军队/人口/瞬间胜利/加速），换国家全功能跟随，原始双 bug（时灵时不灵/换国家失效）根除
**技术定稿**：详见 [抓点候选_20260829.md](./抓点候选_20260829.md) §六/§七——v23 零钩子身份链（IsPlayerCountry 永久禁钩，改直呼 Singleton getter）、军队/人口城源头钉值（CityDataMgr 单例→城字典→ArmyData/PopulationData m_num）、FileOffToVaAny 优先可写段、KWrite 必须配 countryId 校验
**遗留**：官版库（211MB 无导出）经济功能惰性，双库适配需补官版 dump；人口/军队国家级假显示已移除，显示游戏真实汇总值
**OTA（8/30 定稿）**：会话改动 100% 在 so（dex/manifest 沿用 8/28 包）；`08_output/无悔华夏/ota_so/libstarry_magic.so`（sha fc01694e…）供服务器下发。注意：①8/28 老包菜单的补丁下载有 bug（显示空，8/29 20:42 菜单源码已修）——用户须先装本最终包，之后纯 so OTA 才可用；②OTA=整包覆盖安装，乐变回退官版一轮，用户须知"更新后重启一次"；③游戏热更致 RVA 漂移时按抓点候选文档重定位

## 2026-08-29 重建会话（Round 2 启动）

> 背景：8/28 后工作区产物被清理（02_apks / 04_scripts / 06_dumped / 08_output 全空，dump.cs 与脚本均不在）。
> 目标：修复用户反馈的两个 bug（功能时灵时不灵；换国家无效果）→ 根因 = `CachePlayerFromCountry`「首个过钩国家当玩家」启发式 + 缓存不随换国家/换剧本重置。方案：改走「玩家动作/实体数值」路线（资源车加钱粮、用军不扣兵、人口 cap），瞬间胜利/加速不变。

### 产物（真机 f9a116a2 拉取）

| 文件 | 说明 |
|------|------|
| `02_apks/无悔华夏/无悔华夏_3.8.280_九游原版.apk` | **干净九游原包**（cert CN=9Game / SHA1 `7E:D5:24:0F…`，无 starry 条目、仅 classes.dex 单壳 dex）→ 外科底包。sha256 `6af61c2c…` |
| `02_apks/无悔华夏/无悔华夏_3.8.280_星空mod_from_device.apk` | 8/28 17:55 装机的 v19 系 mod 包（so sha256 `0664e1fd…`），现状参照。sha256 `548c434f…` |
| `02_apks/无悔华夏/无悔华夏_3.8.280_星空mod_v12_from_123yunpan.apk` | ⚠️ 用户从 123云盘下载、**误当原版**：实为 8/27 19:52 v12 旧 mod（`lib ready (minimal hooks, feature-gated)` + STARRY.SF/RSA），**禁止安装/当底包**（含 IsPlayerCountry Dobby 旧钩，剧情闪退未修线） |

### 设备状态（8/29 21:34）

- 用户**卸载了 v19 mod** 后装上干净原版（firstInstallTime=2026-08-29 21:34:11，已 grep 验证 0 STARRY 特征）→ 游戏数据目录被清，DLC/热更资源首启会重下；进度走九游账号云存档。
- 待办：重建 dump 脚本与产物（runtime metadata + Il2CppDumper）→ 静态找新锚点（资源车发放 / 军队扣减 / 人口 cap / 玩家国家权威字段）→ Trace 定稿 → 重写 WhrhxMod。

## 后台导入

1. 管理端 → **菜单模板** → JSON 导入 `whhx_menu_template.json`
2. **游戏列表** → 添加/`game_id=whhx` → 选模板 `whhx` → 同步菜单
3. 功能页按需开关门控

## 下一步

- ~~Trace / 写 Mod / 真机验收 / JSON / 清理~~ → **已完成**
- 产物：`08_output/无悔华夏/无悔华夏_3.8.280_surgical-releaseSigned.apk`
- 维护：换包版本后重跑 Il2CppDumper + Trace 对比 RVA

## 真机验收（2026-08-27）

| 菜单 | 状态 | 备注 |
|------|------|------|
| 无限金币/粮食 | ✅ | `CountryResData.m_num` + Cost* bypass |
| 无限人口 | ✅ | `SetTotalNum` + 玩家 PopData |
| 无限军队 | ✅ | `EffectData.get_Value` 按 `m_countryId==玩家`；敌城不变 |
| 瞬间胜利 | ✅ | Trace 定稿：`UpdateBattleTime` tick8 → `SetEnd(true)` |
| 游戏加速 | ✅ | `get_deltaTime` × 滑条 |
