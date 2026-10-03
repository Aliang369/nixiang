# C1 / C2 模式 — 设备侧配置化与诊断分档（通用规范）

> 适用范围：`mod菜单/starry-mod-menu` 下所有 `Games/*Mod.cpp` 模块。
> 首次提炼（2026-10-04）自境界守望者 `Games/WorMod.cpp` 的 `wor_cfg.txt`（C1）+ `WOR_DIAG_FULL`（C2）。
> 现状：本仓库 73 个游戏模块里，**native 侧走 C1 的只有境界守望者 1 个**；走 ilpatch/`StarryConfig`
> 路线的有 3 个游戏（无限升级、无限升级 vc1637、境界死神激斗）。C2 的公共件早已存在，但实际
> 接入的只有 1/73。本文就是把「WOR 试出来的那套」写成可复制的清单，避免第二款游戏重新发明一遍。

---

## 0. 一句话

| | 解决什么 | 省掉什么 | 不省什么 |
|---|---|---|---|
| **C1** 设备侧配置化 | 把「会变的数值」从 `.so` 里抽成设备侧文本文件 | 同一款游戏的**反复调参**：不用重编 so、不用重打包、不用重签名、不用重装 APK | **加键要重编**；**接入新游戏要一次性接线** |
| **C2** 诊断分档 | 把只在排障时要的诊断/扫描代码抽成编译期开关 | 发行包里不带诊断功能 | **不省重编**（编译期开关） |

准确表述：**C1 让「改值」永久免费，让「加键」和「接入新游戏」各付一次性成本。**

---

## 1. C1 能省什么（用真实数字说话，取材境界守望者）

| 动作 | 没有 C1 | 有 C1 |
|---|---|---|
| 改一个字段偏移 / 阈值 | 改 `.cpp` → NDK 重编 so → surgical 重打包 → 重签名 → 重合并单包（206,066,487 B）→ `adb install` | `adb push` 一个 ~300 B 的 txt |
| 生效条件 | 全套重来 | 重启游戏（见 §4 热重载，做好后连重启都不用） |
| 出错成本 | 一次全链路 | 改错也只是「没效果」，不崩（见 §2 边界②） |

---

## 2. 三条边界（缺任何一条，「看起来通用，其实第二款游戏就翻车」）

### ① 缺键必须 = 原版行为 / 内建默认（安全底线）

- 读不到文件、文件被删、键写错 → **全部回退内建默认，不报错、不崩、不缺功能**。
- 否则：用户删了个文件 ⇒ 崩溃或功能消失 ⇒ C1 从「省事」变成「埋雷」。
- 参考实现：`WorMod.cpp:WorCfgLoad()` 两个候选路径都失败时，所有全局量保持编译期默认值；
  ilpatch 路线的 `StarryConfig::GetBool(缺键) == false`（缺省=全关=原版）同理。
- **因此内部状态必须保证「可回退」**：解析前先把每个可覆盖目标存在一份**基线快照**里，
  热重载时先还原基线再重新套用覆盖。境界守望者的做法是 `g_offBase[WOR_CFG_OFF_N]` +
  `g_cfgOffBaseTaken` 闩（基线在**运行时对账之后**才取一次，否则会把运行时值当默认值冻死）。

### ② 加了新键要重编

- 键表是**编译期**的（`kWorCfgOffs[]` 这种 `{key, dst, what}` 静态表）。配置文件里写一个
  不存在的键，运行时不认，只 WARN。
- ⇒ **首次接线时要把「以后可能会变的东西」一次抽完**。境界守望者抽了 15 项字段偏移 +
  8 个可调项；若只抽 2 个键，第二轮想调第 3 个就又回到重编。
- 判断标准：问自己「这个东西我以后会不会想不重编就试一下？」——会，就抽。

### ③ 「操作员旋钮」与「玩家开关」不能混成一份配置

| 形态 | 谁写 | 谁读 | 回写风险 | 例 |
|---|---|---|---|---|
| **只读工程旋钮** | `adb push` / 编辑器 | `.so` | 无 | `wor_cfg.txt` |
| **玩家功能开关** | 悬浮菜单（Java → `nativeSetValue`） | DLL / `.so` | **有**：菜单状态同步会把值写回 | `starry_config.ini` |

混用的后果是「我明明改了，它自己变回去了」。真实判例（`10_docs/_state/games/无限升级.md:130` ④）：
手改 `starry_config.ini` 写 `exp_multi=N` 会被菜单状态同步写回 `1`（Preferences 无存值 ⇒ 默认 1
重新下发），**必须在菜单里拖滑块才会持久化**。

⇒ 需要两种能力时，开**两个文件**，不要给一个文件加「只读区」。

---

## 3. 两条互不兼容的接线路径

| | **Mono / ilpatch 路线** | **native 自读路线** |
|---|---|---|
| 适用 | 游戏逻辑在 `Assembly-CSharp.dll` 里（Mono / HybridCLR 解释执行 DLL） | 游戏逻辑在 `.so` 里（纯 IL2CPP AOT，没有 DLL 注入面） |
| 现成件 | **有**：`04_scripts/_templates/ilpatch/Patch.cs` 的 `BuildStarryConfig()` 注入 `Starry.StarryConfig` 类型（`README.md:34` 注明「各游戏通用」） | **无**（境界守望者是首个） |
| 消费点 | DLL 内 `StarryConfig.GetBool(string)` / `GetInt(string,int)` | `.so` 内自写 `fopen` + 行解析 → 覆盖自己的全局量 |
| 路径 | `/data/user/0/<pkg>/files/starry_config.ini`（每游戏改 `CFG_PATH` 常量） | `/sdcard/Android/data/<pkg>/files/<game>_cfg.txt`（`adb push` 直达，无需 run-as） |
| 生效延迟 | **2 秒缓存**（改完 ~2s 生效，不用重启） | 启动读一次（**加 §4 热重载后**≈轮询间隔） |
| 已接入 | 无限升级(`WxsjMod`/`WxsjVc1637Mod`)、境界死神激斗(`JjssjdMod`) | 境界守望者(`WorMod`) |

**路线不能混**：有 DLL 注入面就该用 `StarryConfig`（省事、有公共件、有 2s 缓存）；
纯 IL2CPP 才需要自己写解析。别为了「统一」把 native 解析塞进 Mono 游戏。

⚠️ `starry_config.ini` 路径**必须硬编码**：HybridCLR 解释器下 `Application.persistentDataPath`
常返回**空串**（`10_docs/_state/games/境界死神激斗.md:89`），别依赖它拼路径。

---

## 4. 热重载（native 路线的完整形态）

没有热重载的 C1 只省掉「重编+重装」，**没省掉「重启游戏」**；而调偏移/阈值恰恰是要反复试的活。

做法（境界守望者实现，`WorMod.cpp:WorCfgReloadCheck()`，在 `Wor_Tick` 里调用）：

```
每 cfg_poll_ms（默认 2000）:
  stat 候选路径 → 取 (mtime, size)
  与上次相同 → 直接 return（不 fopen）
  不同 → 全部还原内建默认 → 重新解析 → 重取基线外的覆盖 → 打对账日志
```

- **判据用 `mtime`+`size`，不用内容哈希**：`/sdcard`（sdcardfs/FUSE）上 `stat` 只是一次元数据
  读，而每次轮询读全文+哈希是要付 I/O 的。残留漏判：同一秒内改成**同长度**内容才会漏
  —— 本场景改的都是数值文本，长度几乎必变。要绝对确定就改完 `touch` 一下或调大 `cfg_poll_ms`。
- `cfg_poll_ms = 0` **关闭热重载**（语义：一旦设为 0，重载后 `g_cfgPollMs` 回默认 2000 ——
  即「0 只对本次运行有效」，不能靠配置文件永久关掉）。
- **线程安全**：只在 Unity 主线程（`Wor_Tick`）调用；hook 侧只读 `int32_t` 对齐量
  （ARM64 上 32 位对齐读写不撕裂）。最坏是某帧读到半新半旧的偏移 ⇒ 写闸门拒绝 ⇒
  「那一帧没效果」，不会崩。**绝不在重载路径里做 il2cpp 调用 / 扫堆 / 遍历对象。**
- **已置真的闩不要回退**（境界守望者的 `g_offHdataRuntime`）：它同时被运行时对账置真，
  回退会误伤正常路径；宁可「只置真」。

---

## 5. 键分三类（决定热重载能不能生效，必须显式区分并打日志）

| 类别 | 例 | 热重载行为 | 处理 |
|---|---|---|---|
| **立即生效** | 功能开关、探针上限、写闸门阈值、静音开关 | 重载后当帧起生效 | 打 `（立即生效）` |
| **仅初始化期生效** | 布局闸门常量、扫描预算（类数/秒数）、诊断表输出路径 | 重载无效，本次运行不再用 | 打 `（需重启游戏）` + 汇总条数 |
| **热路径直接寻址** | 字段偏移 `off.*` | 当帧起生效，但**改错会静默无效** | 打 `（当帧起生效）` + 逐项 `旧值→新值` + 安全说明 |

偏移类的两个必备兜底（否则「改错」= 崩溃或读到垃圾）：
1. **写闸门**：只在数值合理时才写（境界守望者 `rawSane = (raw < 0 && raw > -g_rawSaneMax)`）。
2. **范围上限**：`g_rawSaneMax`（`RAW_SANE_MAX_DEFAULT (1LL << 44)`）本身也可由配置文件调。

---

## 6. 每游戏首次接线 Checklist（照抄即可）

- [ ] **1. 定路径**：`/sdcard/Android/data/<pkg>/files/<game>_cfg.txt`（主）+ `alternate`
      备用路径；两者都失败 ⇒ 内建默认、不报错。
- [ ] **2. 定消费点**：哪一个函数在启动时/每帧读这些值？native 侧通常是「启动读一次」。
- [ ] **3. 抽键**：把「以后可能想不重编就试一下」的全部抽成键；分「立即生效 / 仅初始化期 /
      偏移」三类。
- [ ] **4. 写解析**：`key = value`，`strtoll(base 0)`（支持 `0x`），`#` 注释，`\r` 容错，
      未知键只 WARN；末尾打一行汇总（原值回显，见 §7）。
- [ ] **5. 加基线快照**：解析前把可覆盖目标存进 `g_offBase[]`，只用一次闩保护，且必须在
      「运行时对账/真值恢复之后」取，否则基线会污染。
- [ ] **6. 加热重载**：`stat` 指纹轮询 + 还原基线 + 重新解析（§4）。
- [ ] **7. 加日志锚点**：按 §7 固定前缀，**结论必须由脚本/代码打印**，不能靠人读日志归纳。
- [ ] **8. 加验收方法**：按 §8 四条，缺一条就不算「验过」。

---

## 7. 日志锚点约定（可 grep、可被脚本判定）

| 锚点 | 含义 |
|---|---|
| `cfg: <path> ok=N bad=M` | 解析成功/失败行数（**ok>0 且 bad 已知**） |
| `cfg: <k=v 汇总> tier=<tier>` | 关键键的实际取值回显 |
| `cfg-off: N 项由 <file> 覆盖` | 偏移覆盖条数 |
| `cfg-off <KEY> = 0xNN（<file> 覆盖）` | 逐项覆盖 |
| `cfg-reload #N: <path> 已重载（mtime=… size=…）` | 热重载发生 |
| `cfg-reload: <key> 旧→新（立即生效）` | 立即生效类变更 |
| `cfg-reload: <key> 旧→新（需重启游戏）` | 仅初始化期类变更 |
| `cfg-reload: off.<KEY> 0x旧→0x新（当帧起生效）` | 偏移变更 |
| `cfg-reload: 配置文件已消失 → 全部回退内建默认值` | 删除文件 |

**回显原值**是关键：只有日志里出现「配置文件里那个非默认值」，才能排除「其实读的是默认值」的
假通过。境界守望者的验收正是靠 `raw_sane_max=8796093022208 min_ms=1600` 这类**刻意非默认**
的值出现在日志里。

⚠️ **首帧时序假象**：模块常见「第一帧打一行 `未解析/未就绪`，~1.6s 后重试才真解析完」。
判定必须用**后出现的那条**（如 `runtime layout end … hdataRT=1` + `hasComponent bound`），
不能用首帧那行下结论。

---

## 8. 验收方法（四条，缺一即为「验过」但没「验真」）

1. **写非默认值 → 日志回显原值**（排除读取失败静默用默认）。
2. **删键 / 删文件 → 回默认且不崩**（验证边界①）。
3. **写错值 → 不崩、只无效**（验证兜底；偏移类必做）。
4. **热重载 → 不重启就能看到 `cfg-reload #N` 且新值生效**（`adb push` 后 ~2s 内）。

第 4 条最好配一条**反证**：改完先 grep 到 `cfg-reload`，再确认功能行为变了
（只看到 reload 行 ≠ 值被使用；见 §9 未验证项）。

---

## 9. 已登记但**尚未验证**的事项（别当成已验证）

- 境界守望者：`off.*` 改成**错值**是否真的改变行为 —— 首轮 12 项都写的**正确值**，
  只证明了「值被采用」，**未证明「值被使用后行为改变」**（`10_docs/境界守望者/_facts.md` §10）。
- 热重载本身在新一轮真机验收前也属「已编译、待实测」。

---

## 10. C2（编译期诊断分档）

- **公共件（早已存在，优先复用）**：`ModDiag.h`（`DLOG/DLOGN/DWARN`，需 `-DSTARRY_DIAG`）+
  `ModDiagLua.h`（`MOD_DIAG_LUA`）+ 模板 `mod菜单/starry-mod-menu/app/src/main/jni/Android_modern_template.mk:58-64`
  + 门禁 `04_scripts/_common/check_release_clean.sh:20`。
- **境界守望者私有件**：`WOR_DIAG_FULL`，由 `Android_wor.mk` 翻成 `-DWOR_DIAG_FULL=$(STARRY_WOR_DIAG)`，
  构建脚本 `wor_build_so.sh` 断言 so 内容里 tier 串是 `diag-tier=FULL` 还是 `diag-tier=RELEASE`。
  **注意它与公共 `STARRY_DIAG` 未打通** —— 新游戏要么用公共件，要么用私有件并在构建脚本里加同款
  内容自证，别两套并存还都不接门禁。
- **体积收益必须整段包**：境界守望者把 13 个诊断/扫描函数**整段** `#if` 掉才省 23,376 B
  （2,240,856 → 2,217,480，≈**1.05%**）。仅包 `DLOG` 语句级别省不到。
- **整段包的前提**：这些符号**无跨文件引用**（核对过 `Archero2Mod.cpp`/`EndfieldMod.cpp` 是
  同名不同签名的私有副本）⇒ 包掉不会链接失败。新游戏包之前必须先核对引用。
- **构建期自证要选对判据**：境界守望者的自检**故意不用** `HpChangeEntitySystem` 当判据，
  因为功能代码 `FindSysClass` 也引用它，两档都会命中。
- **与 C1 的组合**：`FULL` 编一次用于调试，发行档用 `RELEASE`；调试期用 C1 的 `diag = 0`
  在**运行时**静音（免重编）。两者相乘：任一为 0 即静音。

---

## 11. 参考实现坐标（境界守望者，tag `wor-e2-20261004b`；行号会漂移，按符号名 grep）

文件：`mod菜单/starry-mod-menu/app/src/main/jni/Games/WorMod.cpp`

| 符号 | 作用 |
|---|---|
| `WOR_CFG_PRIMARY` / `WOR_CFG_ALT` | 两个候选路径（`/sdcard/…` 与 `/storage/emulated/0/…`） |
| `kWorCfgCands[2]` | `WorCfgStamp()` 轮询用 |
| `struct WorCfgOff { key, dst, what }` + `kWorCfgOffs[]` + `WOR_CFG_OFF_N` | 15 项偏移键表 |
| `g_offBase[]` / `g_cfgOffBaseTaken` | 基线快照 + 只取一次闩（**在运行时对账之后取**） |
| `g_cfgPollMs` / `g_cfgLastPollMs` / `g_cfgMtime` / `g_cfgSize` / `g_cfgReloadN` | 热重载状态 |
| `g_cfgDiag` / `g_cfgProbeMax` / `g_rawSaneMax` / `g_cfgMinAsm` / `g_cfgMinElapsedMs` / `g_cfgScanMaxClasses` / `g_cfgScanMaxSec` / `g_cfgTblPath` | 可覆盖全局量 |
| `CfgTrim()` / `CfgParseI64()` | 解析原语（`strtoll` base 0，尾部只许空白） |
| `WorCfgLoadFile()` | 解析一个文件 |
| `WorCfgLoad()` | 启动一次：试两个候选路径，失败全默认 |
| `WorCfgStamp()` | `stat` 取 `(mtime, size)` |
| `WorCfgReloadCheck()` | 热重载主体（在 `Wor_Tick` 里调用） |
| `WorCfgApplyOffsets()` | 把覆盖套到 `g_off_*` + 点名日志 |
| `ApplyRuntimeOffsets()` | 运行时对账后**强制恢复真值**，末尾调用 `WorCfgApplyOffsets()` |

**生效顺序（不能反）**：`ApplyRuntimeOffsets()`（运行时对账/真值恢复）→ 末尾 `WorCfgApplyOffsets()`。
优先级 = **配置文件 > 运行时真值 > 运行时反推值**。
（境界守望者的运行时反推对值类型统一多报 `+0x10`，所以覆盖必须发生在真值恢复**之后**。）

---

## 12. 键表设计模板（照抄骨架）

```
# <游戏> 运行时配置（adb push 到 /sdcard/Android/data/<pkg>/files/<game>_cfg.txt）
# 语义：缺键 = 内建默认；删掉本文件 = 全部回原版行为；未知键只警告
# 改动后 <轮询间隔> 内自动生效（无需重启游戏）

# ---- 立即生效 ----
diag             = 0            # 1 = 保留运行时诊断日志；0 = 静音（与编译期档位相乘）
probe_max        = 33           # 探针日志上限

# ---- 仅初始化期生效（改了要重启游戏）----
layout_min_asm   = 6
layout_min_ms    = 1600
scan_max_classes = 40000
scan_max_sec     = 60
tbl_path         = /sdcard/Android/data/<pkg>/files/<game>_tables.txt

# ---- 热路径直接寻址（改错只无效、不崩，但别在对局中途盲改）----
off.sys_com_hp   = 0x50
off.sys_hdata    = 0x60
...
```

---

## 13. 现状与债（2026-10-04 盘点）

- `Games/*Mod.cpp` = **73** 个模块；`Android_*.mk` = **48** 个。
- 走运行时布局解析的只有 `WorMod.cpp`；7 个走静态 RVA；17 个走 `HookByName`；12 个走 Lua 补丁链。
- C2 公共件 `DLOG` 实际只有 `YimoMod.cpp` 在用（**1/73**）；48 个 mk 里只有 7 个接了口子。
- 另有 5 个模块读设备侧文件但**只写不读**（纯 dump）：`Match2Mod`、`Sc33Mod`、`SignalQuarantine`、
  `WjtxdMod`、`ZmwsMod`。
- ⇒ 已实现的「读设备文件当配置」共 4 个游戏（3 个走 `StarryConfig`、1 个 native 自读），
  且**没有一个公共件覆盖 native 侧**。本文即为补该缺口。
