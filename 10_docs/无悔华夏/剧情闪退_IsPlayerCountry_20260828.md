# 无悔华夏 — release 开功能进剧情闪退（2026-08-28）

## CLAIM

正式 release so 开菜单功能（如无限金币）后进剧情 → UnityMain SIGSEGV；根因是对 `CountryData.IsPlayerCountry` 做了 Dobby 钩（与历史 `CanAddArmy` 同类）。

## VERDICT

**PROVEN**（真机 crash buffer）→ 已改 so 覆盖装，待用户复测剧情。

## EVIDENCE（闪退前）

| 项 | 值 |
|----|-----|
| 时间 | 15:49:39 开 `#1 无限金币` → 15:49:42 崩 |
| 信号 | `SIGSEGV` fault `0xd1b50000b0` |
| PC | `libil2cpp.so+0x3f0c40c` |
| 定稿 RVA | `IsPlayerCountry = 0x3F0C3AC`（PC ≈ RVA+0x60） |
| 对照 | 2026-08-26 `CanAddArmy` 进剧情 SIGILL @ RVA+0xC，已禁钩 |

## WHY「测试 OK、r 包崩」

| 测试包（常 STARRY_TRACE / 未开经济钩） | release（r） |
|----------------------------------------|--------------|
| 热更后 RVA 常 miss → `IsPlayerCountry` **未挂上** | 序言命中 → **Dobby 钩上** |
| 或测时未开金币再进剧情 | 开功能 → 装 eco → 钩 IsPlayerCountry → 进剧情崩 |

Trace 宏本身几乎不改逻辑；差在 **是否真挂上 IsPlayerCountry**。

## FIX（v16）

`WhrhxMod.cpp`：

1. **删除** `Hooked_IsPlayerCountry` / 对该函数的 `HookRvaOrIl2Cpp`
2. `InstallPlayerFilter` 仅 `ResolveOrigAny`（调用判定，**无 Dobby**）
3. `CachePlayerFromCountry` 嵌套指针空检查；`ListAllCostsBypass` size 上限 64

产物：`08_output/无悔华夏/无悔华夏_3.8.280_surgical-releaseSigned.apk`（15:55 已 `pm install -r`）  
so 自检串：`IsPlayerCountry resolve-only (no Dobby)`

## FIX v17（2026-08-28 16:22）— 新版剧情仍崩

真机 16:19 进新版剧情仍 `SIGSEGV @ libil2cpp+0x3f0c40c`（同 IsPlayerCountry+0x60）。  
v16 只去掉 **Dobby 钩**，经济钩里仍 **调用** `g_origIsPlayerCountry` → 新版剧情半初始化 `CountryData` 上照样崩；旧版剧情对象较完整故可过。

v17：

- 全路径 **禁止调用** `IsPlayerCountry`（也不 resolve）
- `CountryLooksReady`（money/food 指针非空）才缓存
- 敌我：缓存指针 / Effect.countryId 自举

热更 so：`08_output/无悔华夏/libstarry_magic_whhx_release.so`  
串：`player filter: no IsPlayerCountry (cache/Effect id only)`  
sha256：`774e572b90c1947af631cbc7b913e1cf4554134935b22751925d625e732f0372`
