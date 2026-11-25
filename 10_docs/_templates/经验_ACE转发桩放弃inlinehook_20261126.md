---
experience_id: exp-20261126-ace-il2cpp-stub
class: tech
status: candidate
scope: "arm64 IL2CPP + 内核级加固壳（ACE 类）改写 il2cpp_init 导出的版本；8 字节 ldr/br 形态未实测"
source: "10_docs/终末地/交付_1.7.2_20261126.md"
evidence: "10_docs/终末地/交付_1.7.2_20261126.md"
targets: "skill/re-implement/hook-rules.md"
approved_by:
approved_at:
approval_ref:
merged_commit:
---

# 经验候选卡：ACE 把 `il2cpp_init` 换成转发桩时，必须放弃 inline hook

> 证据定位：`10_docs/终末地/交付_1.7.2_20261126.md` §6.2（23:58 轮 `hooks sync` / `first-call` 行）与 §八 代码位置表；次要落点 `skill/diagnosis-index.md`（诊断索引一行指针）。
>
> 本轮仅为**提案**（`status: candidate`），未合并、未改 skill。合并门槛见 `经验候选卡.md` §4，由优化会话执行。

## 0. 一句话

`dlsym`/`dynsym` 拿到 `il2cpp_init` 地址后**先做转发桩形态识别**；命中「跳进无 section 的 RX 注入段」就**不要 inline hook**（Dobby 是 16 字节字面量块），改走时间兜底或换挂点。

## 1. 现场

| 项 | 值 |
|---|---|
| 日期 / 来源 | 2026-11-25，终末地 1.7.2（vc71）真机实测；lead 转述＋本会话复核 |
| 包名 / 版本 | `com.hypergryph.endfield`，1.7.2 / vc71，仅 `arm64-v8a` |
| 环境 | OnePlus OPD2404，Android 16，KernelSU root；ACE（`libtersafe2` 5,609,568 / `libtprt` 1,879,688） |
| 样本指纹 | 官方底包 md5 `8abd4b43…`；`libil2cpp.so` 494,380,816 B / md5 `a834fa55…` |

## 2. 观察、解释与候选经验

- **观察事实**：冷启 `SIGILL (ILL_ILLOPC)`，`si_addr == pc`，回栈 `libstarry_magic.so+0x15200`（`EfHooked_Il2cppInit` 里的 `blr x8`）→ `libil2cpp.so+0xa8197e0`。该偏移**不是函数入口**，而是「桩 +12」。
- **候选解释 H1（已证）**：1.7.2 的 `il2cpp_init` 导出被换成 12 字节转发桩 `adrp xN,page; add xN,xN,#imm; br xN` → 无 section/pathname 关联的 RX 注入段派发器执行被搬走的函数头 → **跳回「桩+12」**。Dobby 的 16 字节字面量跳板覆盖了该回落点 ⇒ 回跳执行补丁字面量 ⇒ SIGILL。
- **替代解释 H2（已排除）**：钩子回调自身解引用崩溃 —— 同一份日志显示崩溃前回调未执行；且把 hook 换成「不挂」后 3/3 冷启零 SIGILL。
- **拟提炼的经验**：拿到导出地址后按**指令字位域**识别两种桩形态：① `adrp/add/br`（12 B）；② `ldr xN,[pc,#imm]; br xN`（8 B）。命中且目标落在「**不同 VMA + `r-x`**」的映射（不要用 `pathname` 为空作判据，注入段在 maps 里仍带 `libil2cpp.so` 路径）⇒ 记录形态日志、`g_initHookFailed = true`，**不 inline hook**，改时间兜底或换挂点。
- **适用范围**：arm64、IL2CPP、有内核级完整性/反调试壳（ACE 类）且导出被改写的包；本项目其它同类壳游戏（TP/ACE 系）可先按此检查。
- **不适用/已知例外**：8 B 形态（`ldr/br`）**只有代码分支、无实测命中**；非 arm64 或壳未改写导出时不触发。

## 3. 反证、失效与最小区分实验

- 可推翻的观察：若某版本命中桩判定但进程照常运行且钩子有效 ⇒ 说明判定误报（阈值/位域写错）。
- 已收集反证：**首轮 V3 用「`pathname` 为空」当注入段判据 ⇒ 漏判、hook 照装、SIGILL 复现**；改成「不同 VMA + `r-x`」后命中。
- 失效条件：壳换代（桩指令序列变化）、ABI 换 x86_64、`il2cpp_init` 改回真身导出。
- 最小区分实验：同一包两次冷启，A＝识别并放弃 hook、B＝强行 DobbyHook；预测 A 无 SIGILL 且日志有形态行，B 在启动数秒内 `si_addr==pc` 的 SIGILL。
- 停止条件：连续 3 次冷启无 SIGILL 且日志形态行稳定 ⇒ 证据达标；出现任一次 SIGILL ⇒ 判定逻辑有误，回查位域。

## 4. 门槛自查

| 门槛 | 情况 |
|---|---|
| 可定位证据 | 交付文档 §6.2 的 23:58 轮日志行（`il2cpp_init 为 ACE 转发桩(adrp/add/br 12B @… → 注入段 …)`）+ 修复前后 3/3 冷启对比 |
| 事实/推断 | 桩形态与「桩+12」落点＝**PROVEN（样本内）**；8 B 形态＝**NOT PROVEN**（无实测） |
| 范围 | 已写（arm64 + ACE 类壳 + 导出被改写） |
| 反例已查 | 有（`pathname` 判据漏判，已写入 §3） |
| 同义条目 | 检索 `skill/**`、`pending_experience.md`：未见「转发桩/inline hook 放弃」条目；`hook-rules.md` 现有内容是挂点选取与指纹，不含本判据 |
| 落点 | 建议并入 `skill/re-implement/hook-rules.md`（inline hook 前置检查）；诊断入口 `skill/diagnosis-index.md` 增一行「SIGILL 且 si_addr==pc」指向本条 |
| 政策面 | 不涉及（纯技术） |

## 5. 证据门禁

| CLAIM | EVIDENCE | VERDICT |
|---|---|---|
| 1.7.2 的 `il2cpp_init` 导出是 12 B ACE 转发桩 | `il2cpp_init 为 ACE 转发桩(adrp/add/br 12B @0x… → 注入段 0x…)`；`libil2cpp+0xa8197e0` = 桩+12 | PROVEN（样本内） |
| 对桩做 16 B inline patch 会 SIGILL（`si_addr==pc`） | 修复前每次冷启 SIGILL；修复后 3/3 冷启零 SIGILL | PROVEN（样本内） |
| 8 B `ldr/br` 形态同样应放弃 hook | 无实测样本 | NOT PROVEN |

## 6. 生命周期

- 证据状态：样本内支持（单游戏单版本，3 次冷启 + 3 次叠加轮）。
- 门槛状态：**候选**（本人按要求只提案，未自批合并）。
- 目标文件（仅提案）：`skill/re-implement/hook-rules.md`、`skill/diagnosis-index.md`。
- 合并方式建议：在 hook-rules 增「inline hook 前置检查：导出是否转发桩」小节，附位域判据与两个形态、失败症状（SIGILL/pc==si_addr/落点=桩+12）。
- 重新验证触发：换壳版本、换 ABI、或首次遇到 8 B 形态时。
