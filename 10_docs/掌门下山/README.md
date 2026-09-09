# 掌门下山 — 初判分析

> 版本：com.yofijoy.zmxs **1.0.2**（versionCode 12）  
> 分析日期：2026-08-24  
> **结案：2026-08-25** — 战斗（无敌/秒杀）为 **服务端权威**，客户端改血/改包无法真生效；工程产物已清理，仅保留本目录文档。

## 结案摘要

| 项 | 结论 |
|---|---|
| 引擎 | Unity IL2CPP + xLua + `YF.Net.HjTcpNetwork` |
| 伤害 | 服务器结算；客户端 `sendAttack` 只报技能意图 |
| 客户端改血/挡死亡 | 仅 UI 假效果（已实锤） |
| 协议只读抓包 | 应用层大体明文（`no16` + protobuf 可见）；改上行无伤害字段可改 |
| 工程清理 | 已删 so/APK/脚本/`ZmxsMod`；`Android.mk` 已切回非 ZMXS |

详见：`抓点定稿_20260825.md`、`协议摸底_只读_20260825.md`、`协议抓包结果_20260825.md`。

---

## 基本信息

| 项 | 值 |
|---|---|
| 包名 | `com.yofijoy.zmxs` |
| 发行 | 上海游斐（Yofi）/ Juhe 聚合 SDK |
| 渠道 | TapTap（`libtapsdkcore.so`） |
| 底包大小 | ~1.46 GB |
| Application | `com.yofi.legendmp.AppApplication` |
| 入口 Activity | `com.yofi.legendmp.LaunchActivity` → `AppActivity`（Unity） |

## 引擎与架构

| 特征 | 判定 |
|---|---|
| `libil2cpp.so` + `global-metadata.dat` | Unity IL2CPP |
| `libtuanjie.so` | 团结引擎（Unity 中国版 fork） |
| `libxlua.so` + `XLuaManager` | **xLua 热脚本** |
| `HjTcpNetwork` | 自研 TCP 长连接（服务端权威） |
| 热更 | AssetBundles 下载到 `files/AssetBundles/` |

### 脚本加载路径（C# 侧已确认）

`YF.XLuaManager` 常量：

- `Game/ScriptLuaStarter` ← `luastarter.assetpkg`（MD5 `5c14803d…`）
- `Game/ScriptLua` ← `luagame.assetpkg`（MD5 `76a5d4eaa9…`）
- 运行时入口：`InitLua` → `OnLuaABLoad` → `StartLua` → `LoadScript` / `SafeDoString`

**结论：核心玩法（战斗/经济/成长）在 Lua 脚本中，不在 IL2CPP 导出类里。**

C# `YF` 命名空间主要是框架：AssetBundle 管理、地图、网络、UI 桥接。  
`BattleDamageMono.ShowDamage` 仅为**飘字表现**，不是伤害结算。

## 保护与安全

| 检测项 | 结果 |
|---|---|
| 腾讯 ACE/TSS（`libtersafe2`） | ❌ 无 |
| 百度加固（`libbaiduprotect`） | ❌ 无 |
| 斌哥壳 / `libSDKloading` | ❌ 无 |
| QK 签名校验（`libqkcheck`） | ❌ 无 |
| RootBeer（`libtoolChecker`） | ❌ 无 |
| DEX 加密 | ❌ jadx 可读 ~8968 类 |
| Il2CppDumper | ✅ 可 dump（提示 *may be protected*，但产出完整） |
| Lua 包加密 | ✅ `*.assetpkg` 非明文 UnityFS，需运行时注入或解密 |
| TapTap Themis（`libthemis.so`） | 设备指纹/GID，非战斗反作弊 |
| ByteDance ShadowHook | SDK 侧 hook 框架（统计），非游戏反作弊 |
| `libEncryptorP.so` | AppLog 日志加密 |
| `libluster.so` | 光效插件（`com.illusion.light`），非检测 |

**改包可行性：高。** 无 ACE/TSS、无 DEX 完整性黑名单特征 → 可走**外科 zip 组装** + 星光 `SignSpoof`。

## 登录与支付

- TapTap SDK（含 ClientLogin / WebLogin）
- Juhe / Yofi SDK（QQ、微信、支付宝、WebPay）
- 过检：星光标准 `SignSpoof` + TapTap `ClientLoginUtils.isClientLogin → false` + `TapSignKit.getSign` 原包 MD5

## 功能可行性评估

⚠️ 以下为**静态初判**，战斗/经济类须 Lua 抓点 + Trace 实锤 + 服务端行为验证。

### 高可行性（客户端纯本地）

| 功能 | 依据 | 路径 |
|---|---|---|
| 星光悬浮菜单注入 | 无强壳、DEX 可读 | 外科组装 + `libstarry_magic.so` |
| 游戏加速 | Unity `Time.timeScale` / DOTween | IL2CPP hook 或 xLua `SafeDoString` |
| 过 TapTap 登录 | 标准星光过检链 | Java SignSpoof + TapTap patch |

### 中可行性（须 Lua 层抓点，可能有服务端校验）

| 功能 | 依据 | 风险 |
|---|---|---|
| 秒杀 / 无敌 / 倍攻 | 玩法在 `luagame.assetpkg` Lua | TCP 服务端可能校验战斗结果 |
| 零 CD / 无限技能 | 同上 | 同上 |
| 本地 UI/表现类修改 | C# 桥接类可见 | 低 |

**推荐抓点路径：**

1. 自注入 `StarryTrace=1` → hook `XLuaManager::SafeDoString` / `libxlua` `luaL_loadbuffer`
2. 参考仓内 `Match2Mod.cpp`（xLua C API，勿 il2cpp DoString）
3. 或 KSU memdump 运行期 Lua 明文（若 loader 解密后在内存）

### 低 feasibility / 不建议优先

| 功能 | 原因 |
|---|---|
| 货币 / 灵石 / 修为直改 | `HjTcpNetwork` 服务端同步，改本地大概率回滚或异常 |
| 内购免费 | Yofi/Juhe 支付 + 服务端发货 |
| 免广告 | 字节/GDT 广告 SDK，奖励可能服务端校验 |

## 下一步（若落地 Mod）

1. ~~`Android.mk` 新增 `STARRY_GAME_ZMXS` + `ZmxsMod.cpp`~~ ✅
2. ~~外科组装首包~~ ✅ `scripts/surgical_zmxs_1.0.2.sh`
3. Trace 窗口：进战斗 30–60s，抓 Lua 加载与战斗相关函数名
4. 评估服务端：小倍率试改 → 看结算/同步是否报错

## Mod 菜单功能（ZmxsMod）

| 菜单项 | 实现路径 | 状态 |
|---|---|---|
| 游戏加速 | Lua `UnityEngine.Time.timeScale` + Slider 1–5 | **NOT PROVEN** |
| 无敌 / 秒杀 / 无限技能 / 零 CD | xLua 运行时扫描 wrap（`XLuaManager.Update` 注入） | **NOT PROVEN** |
| 免广告 | Lua ad/reward wrap + `ZmxsHooks.setNoAd` | **NOT PROVEN** |
| TapTap 过检 | SignSpoof + TapSignKit MD5 + WebLogin | 静态已 patch，装机待验 |

## 功能无效修复（2026-08-25）

**根因（已修）：**

1. **Lua 闭包固化** — `STARRY_AN_*` 曾读入 local 后不再跟随菜单开关；已改为每次 `feat()` 读 `_G`
2. **错误 hook 符号** — `libxlua.so` 导出 `xluaL_loadbuffer`，非 `luaL_loadbuffer`
3. **IL2CPP 签名错误** — `InitLua`/`LateUpdate` 等 0 参方法误加 `method` 参数 → **SIGSEGV**；已改为 `void(*)(void* thiz)` / `void(*)(void* thiz, void* arg1)`
4. **注入时机** — Lua 在 hook 安装前已加载；现：`Update`/`LateUpdate`/`UpdateGameScene`/`LoadScript` + `xluaL_loadbuffer` + 主线程 `get_Instance` 兜底
5. **后台线程 lua_pcall** — 曾 SIGABRT；Lua 同步仅限主线程 hook
6. **Update/LateUpdate 内 pcall 闪退** — 见上；Update 仅写全局，install 只在脚本加载后
7. **登录页卡死** — 改 `xlua_setglobal`；Starter 阶段不注入
8. **进游戏卡 34%** — 加载期 `loadbuffer` 内 `pcall` 扫描 Lua + `g_wantInstall` 默认 true；已 **移除 loadbuffer hook**，仅用户开功能后于 `StartGameScene`/`LoadScript` 安装 patch

**装机验证（修复包，需进 Unity 主界面）：**

```
[Zmxs] early loadbuffer hook ok
[Zmxs] hooked XLuaManager.Update / LoadScript / LateUpdate ...
[Zmxs] 初始化成功
[Zmxs] lua patch installed spd=1   ← 期望出现
```

| CLAIM | VERDICT |
|---|---|
| 启动不闪退 / Hook 就绪 | **PROVEN**（签名修复后） |
| lua patch installed | **NOT PROVEN**（须进游戏过 Unity 加载） |
| 战斗/加速/免广告生效 | **NOT PROVEN**（须用户进战斗实测） |


**现象：** 首包 `BaseAppActivity.onCreate` 重复 `loadLibrary` + il2cpp 就绪前 hook `Update` → `SIGSEGV`（`libstarry_magic.so` → `libil2cpp.so`，uptime≈3s）。

**修复：**
1. 外科脚本不再 patch `BaseAppActivity`（仅 `StarryApp.attachBaseContext` 加载 so）
2. `ZmxsMod`：init 前 `sleep(5)`；`il2cpp_method_get_pointer` + `IsInIl2Cpp` 校验；`Hooked_Update` 校验 `thiz`

**装机验证（修复包）：**

```
[Zmxs] hooked XLuaManager.Update @ 0x755d0d0a6c envOff=0x20
[Zmxs] 初始化成功
[Zmxs] lua patch installed
pid 28852 存活 >25s，无 SIGSEGV
```

| CLAIM | VERDICT |
|---|---|
| 启动不闪退 / Hook 就绪 | **PROVEN** |
| 战斗/加速/免广告生效 | **NOT PROVEN**（须进战斗实机） |

构建：

```bash
cd mod菜单/starry-mod-menu
bash scripts/surgical_zmxs_1.0.2.sh
# 仅换 so：
bash scripts/zmxs_so_only.sh
# 覆盖安装：
STARRY_INSTALL=1 bash scripts/surgical_zmxs_1.0.2.sh
```

装机日志：

```bash
adb logcat -s StarryApp StarryMagic MenuService StarrySignSpoof | grep -E 'Zmxs|zmxs|auth game_id'
# 期望：auth game_id synced from native: zmxs
# 期望：[Zmxs] hooked XLuaManager.Update / lua patch installed
```

## 产物路径

| 路径 | 说明 |
|---|---|
| `02_apks/掌门下山/掌门下山_1.0.2_from_device.apk` | 真机 pull |
| `08_output/掌门下山/掌门下山_1.0.2_surgical-releaseSigned.apk` | 外科星光包 |
| `08_output/掌门下山/掌门下山_1.0.2_starry.apk` | 同上（别名） |
| `mod菜单/starry-mod-menu/scripts/surgical_zmxs_1.0.2.sh` | 首包组装 |
| `mod菜单/starry-mod-menu/scripts/zmxs_so_only.sh` | 仅换 so |
| `03_decompiled/掌门下山/jadx_1.0.2/` | jadx |
| `03_decompiled/掌门下山/il2cpp_1.0.2/` | Il2CppDumper dump.cs + script.json |
| `/tmp/zmxs_apktool/` | apktool 解 Manifest |
