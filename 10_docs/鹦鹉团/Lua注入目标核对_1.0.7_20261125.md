# 鹦鹉团 1.0.7（TapTap 原版）Lua 注入目标核对（函数级）

> 日期：2026-11-25　范围：`02_apks/鹦鹉团/鹦鹉团_1.0.7_taptap原版.apk`　只读静态核对
> 上游文档：`静态结构核对_1.0.7_20261125.md` §6（名称存在性）；chunk 序号一律本包现取

## 结论

**可注入，直接复用 v7 脚本（`kYwtPatch`，`__ywt_ver=7`）。脚本引用的 17 个游戏侧名字/字段逐字全部命中，a–f 六项全通过，0 处需要改脚本。**

## 0. 输入与可复现

| 项 | 值 |
|---|---|
| bundle | `assets/Android/lua_csys_dance.unity3d`，837,241 B |
| bundle sha256 | `e43cb00d71201ef4a33df71f4686a3b5f9e63bfe829a7b403867b4d70bda4527` |
| 解帧 | 127 帧 → 127 个 Lua chunk，合计 1,806,006 B（与工具注释一致） |
| 反汇编器 | `04_scripts/_common/lua/lua53_dis.py`（sha256 `5aed9bd6…a4b31`） |
| 脚本原文 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/ywtMod.cpp` 第 143–565 行（`R"YWT(` / `)YWT"` 之间），抽到 `13_scratch/鹦鹉团/lua_dance/kYwtPatch.lua`，sha256 `ce8217bc…0b39e`，423 行 |

复现命令（workdir `/Users/aliang/Documents/nixiang`，前置 `export HOME=/Users/aliang`、`PATH=/opt/homebrew/bin:…`、`TMPDIR=/tmp`）：

```bash
cd 13_scratch/鹦鹉团/lua_dance
unzip -o ../../../02_apks/鹦鹉团/鹦鹉团_1.0.7_taptap原版.apk 'assets/Android/lua_csys_dance.unity3d'
python3 ../../../04_scripts/_common/lua/lua_ab_multiframe.py assets/Android/lua_csys_dance.unity3d chunks
python3 dump_all.py > chunk_map.tsv        # 逐 chunk 反汇编到 dis/NNN.txt，source 名来自 chunk 头
sed -n '143,565p' ../../../mod菜单/starry-mod-menu/app/src/main/jni/Games/ywtMod.cpp > kYwtPatch.lua
```

辅助脚本（都在 `13_scratch/鹦鹉团/lua_dance/`）：`dump_all.py`（解帧→反汇编→chunk 映射）、`methods.py`+`methods.json`（方法名→proto#→params）、`query.py`/`q2.py`/`q4.py`（按名查签名）、`dbg001.py`（chunk 001 变体诊断）。

## 1. chunk ↔ 模块名（按名字核，不按 `__NNN` 核）

127 个 chunk 全部 `consumed == total`（解析自证闭合）。**chunk 序号只作本版定位用，换包会漂；核对一律按 source 名。**

| chunk | 模块（source） | protos | 本任务用途 |
|---|---|---|---|
| 023 | `DanceVM.lua` | 53 | a |
| 052 | `RhythmMasterVM.lua` | 51 | b、f |
| 015 | `DanceTMBattleBase.lua` | 38 | c |
| 117 | `UI_DanceRhythmMasterMode_Ctrl.lua` | 103 | d |
| 050 | `RhythmMasterMode.lua` | 35 | e |
| 012 | `DanceRhythmVM.lua` | 41 | f |
| 008 | `DanceKeyFallingVM.lua` | 41 | f |
| 011 | `DanceModeBase.lua` | 142 | typeMode 工厂（旁证） |
| 014 / 019 | `DanceTMBase.lua` / `DanceTMParty.lua` | 17 / 21 | AdjustJudge 覆写（旁证） |

全表 127 行见 `13_scratch/鹦鹉团/lua_dance/chunk_map.tsv`（`序号<TAB>模块<TAB>protos<TAB>reader`）。

## 2. 逐项核对

### a) `DanceVM.SetCheatMode` / `GetCheatMode` / `CheatMode{None=1,Prefect=2,Random=3}` — **通过**

chunk 023（`dis/023.txt`）：

| 项 | 证据 | 结果 |
|---|---|---|
| `CheatMode` 三个数值 | L121–125：`[119] NEWTABLE R3 0 3` → `[120] SETTABLE R3 R3 "None" 1` / `[121] "Prefect" 2` / `[122] "Random" 3` / `[123] SETTABLE R2 R2 "CheatMode" R3` | None=1、Prefect=2、Random=3 ✓ |
| `GetCheatMode` | L309 `[307] SETTABLE R6 R6 "GetCheatMode" R7`；L946 `function proto #22 params=1`（self），方法体 `[0] GETTABUP R1 U0 "cheat_mode" ; [1] RETURN` | 存在，1 参 ✓ |
| `SetCheatMode` | L312 `[310] SETTABLE R6 R6 "SetCheatMode" R7`；L951 `function proto #23 params=2`（self, mode），方法体 `[0] SETTABUP R0 U0 "cheat_mode" R1` | 存在，2 参 ✓ |

脚本 L371–377 `DM= getmod("DanceVM"); set = idx(DM,"SetCheatMode"); pcall(set, DM, want)`，`want ∈ {1,2,3}` 与上表一致。

### b) `RhythmMasterVM.JudgeResultType` 七字段 — **通过（但字段名是长名，不是文档里的短名）**

chunk 052（`dis/052.txt` L22–30，挂到 `RhythmMasterVM` 表上）：

```
[ 20] NEWTABLE  R4   0 7
[ 21] SETTABLE  R4   R4 "Miss" 1
[ 22] SETTABLE  R4   R4 "Good" 2
[ 23] SETTABLE  R4   R4 "Great" 3
[ 24] SETTABLE  R4   R4 "Perfect" 4
[ 25] SETTABLE  R4   R4 "SuperPerfect" 5
[ 26] SETTABLE  R4   R4 "SuperPerfectPlus" 6
[ 27] SETTABLE  R4   R4 "SuperPerfectPlusPlusAdd" 7
[ 28] SETTABLE  R3   R3 "JudgeResultType" R4      -- R3 = RhythmMasterVM (L19 GETTABUP)
```

数值 1..7 与预期完全一致；**字段名在 1.0.7 是 `SuperPerfect` / `SuperPerfectPlus` / `SuperPerfectPlusPlusAdd`**，没有 `SperfectPlus` 这种小写 p 写法。

⚠ 对照陷阱：`DanceVM.JudgeResultType`（chunk 023 L72–78）**只有 6 项**且名字不同——`Miss=1 Good=2 Great=3 Perfect=4 Sperfect=5 SperfectPlus=6`。取 7 档枚举必须走 `RhythmMasterVM`，走 `DanceVM` 会拿不到第 7 档。脚本 L47–48 用的正是 `RhythmMasterVM`，没错。

### c) `DanceTMBattleBase.AdjustJudge(self, judgeType)` — **通过**

chunk 015（`dis/015.txt`）：

- L62 `[60] SETTABLE R0 R0 "AdjustJudge" R1` —— 定义在 `DanceTMBattleBase` 类表上（L14 `SETTABUP R0 U0 "DanceTMBattleBase" R0` 已建立类表）。
- L739–751 `function proto #15 params=2 vararg=0 stack=6 nups=0`：

```
[ 0] LE        R0   256 1        -- K0(=7) <= judgeType ?
[ 1] JMP       R0   +1 -> 3
[ 2] LOADK     R1        6       -- judgeType = 6
[ 3] SELF      R2   R0 "OnAdjustJudge"
[ 4] MOVE      R4   1 0
[ 5] CALL      R2   3 2         -- self:OnAdjustJudge(judgeType)
[ 6] SELF      R3   R0 "AddMyEnergy"
[ 7] MOVE      R5   2 0
[ 8] CALL      R3   3 1
[ 9] RETURN    R2   2 0
```

常量表实测 `k = [7, 6, "OnAdjustJudge", "AddMyEnergy"]`：**入参 ≥7 时先归一到 6，再进 `OnAdjustJudge` + `AddMyEnergy`，返回值即（归一后的）判定档。**

参数个数 = 2（self + judgeType）✓。旁证：`DanceTMBase.AdjustJudge`（chunk 014 proto#3）、`DanceTMParty.AdjustJudge`（chunk 019 proto#8）同为 `params=2`；`DanceTM{Normal,Single,Party,Qualify,Challenge}` 全部 `Class(DanceTMBattleBase)`（018/019/020/016/021 首 10 条指令均为 `Class(DanceTMBattleBase)`），所以 `typeMode:AdjustJudge` 一定有。

### d) `UI_DanceRhythmMasterMode_Ctrl.OnJudgeResult` 6 参 + `JudgeModule` 兜底 — **通过**

chunk 117（`dis/117.txt`）：

| 项 | 证据 | 结果 |
|---|---|---|
| 类表建立 | L35–41 `GETTABUP R1 U0 "UI_DanceRhythmMasterMode_Ctrl"` → `SETTABUP R0 U0 "UI_DanceRhythmMasterMode_Ctrl" R1` | 全局名逐字命中 ✓ |
| `OnJudgeResult` | L230 `[228] SETTABLE R20 R20 "OnJudgeResult" R21`；L2883 `function proto #50 params=6 vararg=0` | **6 参（self+5）** ✓ |
| `JudgeModule` 兜底 | L172 `[170] SETTABLE R19 R19 "JudgeModule" R20`；L1766 `function proto #30 params=3`：`U0:ShowJudgeResult(R1,false,false)`，`if R2 then self:PlayJudgeAudio(R1)` | 存在，`(self, judgeType, playAudio)` ✓ |

`OnJudgeResult` 实参映射（proto#50 前 5 条指令）：`[0] GETTABLE R6 R0 "typeMode"` → `[1] SELF R6 R6 "AdjustJudge"` → `[2] MOVE R8 2 0`（R8 = R2）→ `[3] CALL R6 3 2` → `[4] MOVE R2 R6`，即 **`self.typeMode:AdjustJudge(第 2 个实参)` 的返回值覆盖成新判定档**。

调用点核对（chunk 117 `AutoHitTap`，proto#72，L4230–4261）：

```
[  3] GETTABUP  R3   U0 "RhythmMasterVM"
[  4] GETTABLE  R3   R3 "JudgeResultType"
[  5] GETTABLE  R3   R3 "SuperPerfectPlusPlusAdd"   -- R3 = 7
...
[ 22] SELF      R4   R0 "OnJudgeResult"
[ 23] GETTABLE  R6   R1 "x"
[ 24] MOVE      R7   3 0                             -- R7 = R3 = 7（判定档）
[ 25] GETTABLE  R8   R1 "type"
[ 26] LOADNIL   R9   0 0
[ 27] GETTABLE  R10  R1 "nid"
[ 28] CALL      R4   7 1                             -- self + 5 实参
```

⇒ 6 参、第 3 位是判定档，与脚本包装签名 `function(self, x, judgeType, noteType, a4, nid)`（L156）+ `orig(self, x, judgeType, noteType, a4, nid)`（L179）**一致**。

### e) `RhythmMasterMode.OnUpdate` 存在且取 `self.typeMode` — **通过**

chunk 050（`dis/050.txt`）：

- L66–68 `GETTABUP R4 U0 "RhythmMasterMode"` → `CLOSURE R5 -> proto#14` → `SETTABLE R4 R4 "OnUpdate" R5`。
- L1249 `function proto #14 params=2 vararg=0 stack=6`（self, dt）。
- L1317–1322（同 proto 内）：`[67] GETTABLE R2 R0 "typeMode"` / `[68] TEST R2 0 0` / `[70] GETTABLE R2 R0 "typeMode"` / `[71] SELF R2 R2 "Sync"` / `[72] CALL R2 2 1` ⇒ **确实取 `self.typeMode` 并调用其 `Sync`**。
- 同函数末尾 `[74] SELF R2 R0 "UpdateShowPosition" / [76] CALL R2 3 1` —— 每帧入口成立。

`typeMode` 从哪来（旁证，chunk 011 `DanceModeBase` proto#16，L1389 起；`SETTABLE R0 R0 "typeMode"` 在 L1409–1473）：按 `DanceVM.SourceChannel` 造 `DanceTMQualify/Challenge/Party/Normal/Single` 或 `DanceTMBase`，`SETTABLE R0 R0 "typeMode" R3` 挂到 mode 实例上。脚本 `armAdjust` 从 `OnUpdate(self)` 拿 `self.typeMode`（L269）路径成立。

### f) 三个 `*_VM.GetJudgeResult` — **通过（只记录，v7 不包）**

| 模块 | chunk | 赋值行 | proto | 参数个数 |
|---|---|---|---|---|
| `RhythmMasterVM.GetJudgeResult` | 052 | L140 `[138] SETTABLE R3 R3 "GetJudgeResult" R4` | L565 `proto #4` | 2（self + time） |
| `DanceRhythmVM.GetJudgeResult` | 012 | L84 `[82] SETTABLE R1 R1 "GetJudgeResult" R2` | L552 `proto #4` | 3（self + 2） |
| `DanceKeyFallingVM.GetJudgeResult` | 008 | L84 `[82] SETTABLE R1 R1 "GetJudgeResult" R2` | L516 `proto #4` | 2（self + 1） |

三个都还在，名字逐字一致；脚本未引用它们（v7 走 `AdjustJudge`），无需改。

## 3. 脚本 ↔ 游戏侧符号对照表

`kYwtPatch.lua`（423 行）里所有游戏侧符号逐条对照：

| 脚本行 | 脚本写法 | 游戏侧证据 | 结果 |
|---|---|---|---|
| L4 / L411 / L415 | `__ywt_ver == 7` / `rs(G,"__ywt_ver",7)` | 脚本自身版本号，非游戏符号 | **是 7** ✓ |
| L38–43 | `ROWS[].enum` = `SuperPerfectPlusPlusAdd` / `SuperPerfectPlus` / `SuperPerfect` / `Perfect` / `Great` / `Good`，`fb` = 7/6/5/4/3/2 | 052 L21–27 七个字段逐字命中，数值 1..7 | ✓ 6 个枚举名 0 处偏差 |
| L47–48 | `getmod("RhythmMasterVM")` → `idx(VM,"JudgeResultType")` | 052 L19–30（`SETTABUP U0 "RhythmMasterVM"`） | ✓ |
| L202 | `rg(env,"UI_DanceRhythmMasterMode_Ctrl")` | 117 L41 全局名逐字命中 | ✓ |
| L153 / L223 | `idx(ctrl,"OnJudgeResult")` 必须 `type=="function"` | 117 L230 + proto#50 `params=6` | ✓ |
| L156 / L179 | 包装签名 6 参并原样透传 | 117 proto#50 `params=6`；AutoHitTap `CALL R4 7 1` | ✓（self+5） |
| L212 / L224 | `idx(t,"JudgeModule")` 兜底 | 117 L172 + proto#30 `params=3` | ✓ |
| L269 / L272 | `idx(self,"typeMode")` → `idx(tm,"AdjustJudge")` | 050 L1317 读 `self.typeMode`；015 L62+proto#15 `params=2` | ✓ |
| L291 | `judgeType >= 7 → 6`（auto 归一） | 015 proto#15 常量 `[7,6]`：游戏自己就把 ≥7 归一成 6 | ✓ 与游戏行为一致 |
| L321 | `aj(tm2, judgeType, ...)` | `AdjustJudge(self, judgeType)` params=2，`...` 透传 | ✓ |
| L333–334 / L337 | `getmod("RhythmMasterMode")` → `idx(M,"OnUpdate")`，包 `M.OnUpdate = function(self, ...)` | 050 L66 + proto#14 `params=2`，内部读 `self.typeMode` | ✓ |
| L371–377 | `getmod("DanceVM")` → `idx(DM,"SetCheatMode")`，`pcall(set, DM, want)`，`want∈{1,2,3}` | 023 L312 + proto#23 `params=2`；`CheatMode` None=1/Prefect=2/Random=3（L121–123） | ✓ |
| L414 | `getmod("RhythmMasterVM") ~= nil` 作为「模块已加载」判据 | 052 全局表存在 | ✓ |

**对不上的数量：0。** 唯一不一致是注释：

- L36 注释写 `SuperPerfect=5 SPerfectPlus=6 SPlusAdd=7`（短名 + 大小写混用），与 1.0.7 实际长名不符。**代码 L38–43 用的是长名，注释是旧版本的残留**，不影响运行。
- 注释里 `__117:4237-4240`、`__015:741-752` 是源码行号引用，本版可对上同语义位置（117 的 `SuperPerfectPlusPlusAdd` 在 `dis/117.txt` L4235–4236；015 的归一在 proto#15 L739–751），但行号会随版本漂，**只能当线索不当断言**。

## 4. 结论与建议

1. **直接复用 v7 脚本，不需要改 `ywtMod.cpp` 的 `kYwtPatch`。** `__ywt_ver=7`，17 个游戏侧名字/字段全部逐字命中，6 项签名/数值全对。
2. 唯一可选动作（纯注释、不影响行为，非必须）：把 L36 注释改成实际长名 `SuperPerfect=5 SuperPerfectPlus=6 SuperPerfectPlusPlusAdd=7`，避免下次维护按注释去 `idx(JT,"SPlusAdd")` 踩空。改法：只改注释行，不动 L38–43 的 `ROWS`。
3. 运行时仍需真机验证的点（静态不可判）：`debug.getupvalue` 在被 hook 的 xLua 环境里是否可用（脚本 L113 `envsOf` 依赖它）；`UI_DanceRhythmMasterMode_Ctrl` 类表是否有防写元表（脚本 L151 `rg(ctrl,"__ywt_hooked")` 先探再写、写入走 `pcall(rs, …)`，已有兜底）。这与本次底包结构无关，是既有机制。

## 5. 踩坑记录（本版新增，供复现）

1. **chunk 001（`DanceActorUpdateAnimMgr.lua`）解析失败**：该 chunk 的常量里出现 `tag=20`（长串）+ `0xFF` 长度前缀，但长度字段是 **4 字节 u32**（实测 `ff 64 01 00 00` → 356 字节串），不是 Lua 5.3 标准的 8 字节 `size_t`。`lua53_dis.py` 按 8 字节读会跑飞（`IndexError`）。处理：`dump_all.py` 里先试标准 `Reader`，`consumed != total` 再退回 `Reader4`（0xFF 后读 u32），两者都要求 `consumed == total` 才落盘。**本版 127 个 chunk 只有 001 是 `Reader4`，且它不参与判定链路**；若后续要按这个 chunk 做结论，记得用同一套 reader。
2. **chunk 序号会漂**：本版 `023=DanceVM`、`052=RhythmMasterVM` 只是本包顺序（按模块名排序），换包全部重排。核对一律按 source 名（chunk 头 + `文件头 ::lua_csys_dance\<name>.lua`）。
3. **`DanceVM.JudgeResultType` ≠ `RhythmMasterVM.JudgeResultType`**：前者 6 项（`Sperfect`/`SperfectPlus`），后者 7 项（`SuperPerfect`/`SuperPerfectPlus`/`SuperPerfectPlusPlusAdd`）。历史上这两张表都叫 `JudgeResultType`，只看名字会取错表。
4. **`OnJudgeResult` 的入参会被就地覆盖**：`[4] MOVE R2 R6`（AdjustJudge 返回值）+ `[19]–[28]` 随机模式下 `math.random(2,6)` 再覆盖。绕 `OnJudgeResult` 入参改判定**不生效**在随机模式下必然发生——脚本以 `typeMode.AdjustJudge` 为主落点（L321）是对的，`OnJudgeResult` 包装只当补充日志。
