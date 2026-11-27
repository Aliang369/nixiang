---
experience_id: exp-20261127-houdini-no-x-segment
class: tech
status: merged
scope: "雷电模拟器（x86_64 + houdini 转译，Android 14/SDK34）下的 arm64 IL2CPP 包；MuMu/BlueStacks 未验"
source: "10_docs/独奏骑士/复刻_1.1.768_雷电模拟器_20261127.md"
evidence: "10_docs/独奏骑士/复刻_1.1.768_雷电模拟器_20261127.md"
targets: "skill/re-implement/hook-rules.md#30.2, skill/re-implement/hook-rules.md#30.4, skill/starry-mod/SKILL.md"
approved_by: auto:tech-promote
approved_at: 2026-11-27
approval_ref: "10_docs/独奏骑士/复刻_1.1.768_雷电模拟器_20261127.md（修前 resolve 循环行 + 修后 hooked 行 + 三项计数）"
merged_ref: "skill/re-implement/hook-rules.md#30.2, skill/re-implement/hook-rules.md#30.4, skill/starry-mod/SKILL.md#出包自检步骤 5/6"
merged_at: 2026-11-27
merged_commit: fb590ac6
---

# 经验候选卡：houdini 转译环境下 arm64 库在 maps 里没有 `x` 段，按文件偏移换算 VA 不能只认可执行段

> 证据定位：`10_docs/独奏骑士/复刻_1.1.768_雷电模拟器_20261127.md` 「环境差异」节（maps 权限实测 + 修前/修后日志对照）。
> 现状：`status: merged`（2026-11-27 按 §4 门槛自动合并，class: tech）。落点见 frontmatter `merged_ref`；合并提交 `fb590ac6`。

## 0. 一句话

在 x86_64 + houdini 的模拟器里，arm64 的 `libil2cpp.so` 在 `/proc/self/maps` 中只有 `r--p`/`rw-p`（**没有 `x` 段**）；把「命中段必须可执行」写成先决条件的 `FileOffToVa()` 会恒返回 `nullptr`，表现为**不崩溃、不报错、只循环打一行 resolve 失败**。判据应改为「区间覆盖 fileOff 即成立」，权限位只用来在多个命中之间排序。

## 1. 现场

| 项 | 值 |
|---|---|
| 日期 / 来源 | 2026-11-27，独奏骑士 1.1.768 复刻（Windows 侧出包 + 雷电验收） |
| 包名 / 版本 | `com.Shimmer.SoloKnight.cn`，1.1.768 / vc240；Unity IL2CPP + HybridCLR |
| 环境 | 雷电模拟器 `emulator-5554`，Android 14 / SDK 34，**x86_64 + houdini 转译**，root |
| 样本指纹 | 底包 md5 `ccf1b28bf7b4b9d9a950bc7a86fbaf9e`；`libil2cpp.so` md5 `26a4e1174bacf2fd836bd4e833f9572b`；产物 md5 `02fa2e0f2edfa96cf96d2b799bb35baf` |
| 代码位置 | `mod菜单/starry-mod-menu/app/src/main/jni/Games/DzqsMod.cpp` → `FileOffToVa()` |
| 授权范围 | 用户授权范围内本机出包 + 模拟器装机验证；真机未参与 |

## 2. 观察、解释与候选经验

- **观察事实**（不夹带解释）：
  - 修正前，挂点 `0x20B5A40` 每 200 ms 打一行 `[Dzqs] resolve enter-interp failed file=0x20B5A40`，hook 计数恒 0，三项功能全哑；不崩溃、无异常。
  - `/proc/self/maps` 中 `libil2cpp.so` 的 arm64 映射只有 `r--p` 与 `rw-p`，无任何 `x` 段。
  - 修正后（可执行段优先、无 `x` 段退回只读段）立刻打 `[Dzqs] hooked enter-interp @ 0x4000254b9a40 TRACE=0 DMG_WRITE=1`，随后 `isCdOK patch #37000` / `isManaOk patch #28200` / `AddRtHPTo #2000` / `nocd clear … hit=5` 持续增长；FATAL/SIGSEGV/ANR 均为 0，用户终审「功能都可以用的」。
- **候选解释 H1（主）**：guest 的 arm64 代码由 houdini 翻译后在宿主执行，guest 代码页本身不被标可执行 ⇒ 无 `x` 段是该环境的常态，而非「模块没加载好」。
- **替代解释 H2（已排除）**：映射尚未建立/名字不匹配。排除依据：同一行 `maps` 记录里 `off…end` **区间确实覆盖** `0x20B5A40`，换算出的 `0x4000254b9a40` 低 32 位正好等于 VA `0x20B9A40`，且在该地址装钩后立即生效。
- **拟提炼的经验**：
  1. 按文件偏移换算 VA 的代码，**正确性条件只有区间关系** `off ≤ fileOff < off + (end − start)`；`x` 权限只能作为「多处命中时选谁」的**排序偏好**，不能作为必要条件。
  2. 该环境里 **Dobby inline hook 实测有效**——转译层不阻止对 guest 代码段的改写，装钩后功能计数正常增长。
  3. 该环境的失败形态是**静默的**：诊断第一判据是「某 resolve/装钩行在循环打印 + 业务计数恒 0」，不是崩溃日志。
- **适用范围**：x86_64 宿主 + houdini（或同类二进制翻译层）跑 arm64 包；Android 14/SDK34 实测，其它版本未验；只对「按文件偏移或按 maps 区间换算」的解析路径成立。
- **不适用范围 / 已知例外**：真机（arm64 原生）有 `x` 段，原判据本来就成立，本改动对其是行为不变；MuMu 上曾出现的「同名分块假映射」是**另一种**问题（VA 口径选错），与本条不冲突。

## 3. 反证、失效与最小区分实验

- 可推翻该经验的观察：若某翻译层环境下同样「无 `x` 段」但 Dobby `DobbyHook` 返回非 0 或改写无效 ⇒ 「无 `x` 段不影响装钩」这半条被推翻（前半条「不能只认 x 段」仍成立）。
- 已收集反证：无（本轮未遇到装钩失败的样本）。
- 失效条件：换模拟器/换翻译层（MuMu、BlueStacks）、宿主 ABI 变 arm64、或游戏 so 被壳改成注入段形态（此时 maps 里可能有 `x` 段但地址不在模块内）。
- 失效后处置：降为待验证/撤销，保留本条作为「历史判据」并链到新卡。
- 最小区分实验：同一包两次冷启 —— A＝只认 `x` 段（预期：循环刷 `resolve … failed`、功能计数 0）；B＝区间覆盖即可（预期：`hooked … @` 一行 + 计数增长）。H1 预测 A 必失败、H2 预测 A 偶发成功（若真是映射未建立，重试会自愈）——实测 A 稳定失败、B 稳定成功，故采 H1。
- 停止条件：A/B 各一次冷启结论一致即达标；出现「A 成功」则判 H2 或另有因素，回查 maps 与模块加载时机。

## 4. 门槛自查

| 门槛 | 情况 |
|---|---|
| 可定位证据 | maps 权限实测 + 修前 `resolve enter-interp failed` 循环行 + 修后 `hooked enter-interp @ 0x4000254b9a40` + 四项 patch 计数 + 用户终审 |
| 事实 / 推断 | 无 `x` 段、旧判据必失败、Dobby 在此环境有效 = **PROVEN（样本内）**；「houdini 对改写块会重译」= **推断，NOT PROVEN** |
| 范围 | 已写（雷电 + houdini + Android 14 + 该包） |
| 反例已查 | 真机行为不变（有 `x` 段）；MuMu 假映射属另一问题，已区分 |
| 同义条目 | `hook-rules.md` §30「模拟器/翻译层」已有 §30.2 第 2 级「maps 段属性筛选（可执行段…）」与 §30.4「环境差异必须落文档」⇒ **本条是修订而非新建**；`pending_experience.md` 内 MuMu 条目讲 VA 口径，不重复 |
| 落点 | `skill/re-implement/hook-rules.md` §30.2（把「可执行段」从必要条件降为排序偏好）＋ §30.4（环境记录项增加「maps 段权限」一行）；出包侧 `skill/starry-mod/SKILL.md` 自检清单加一条「模拟器验收先看段权限」 |
| 政策面 | 不涉及（纯技术） |

## 5. 证据门禁

| CLAIM（限定范围） | EVIDENCE | VERDICT |
|---|---|---|
| 雷电 houdini 下 arm64 `libil2cpp.so` 无 `x` 段 | `/proc/self/maps` 该库只有 `r--p`/`rw-p` | PROVEN（样本内） |
| 把 `x` 段当必要条件 ⇒ 永不装钩且静默 | 修正前 `resolve enter-interp failed` 每 200 ms 循环、业务计数 0、无崩溃 | PROVEN（样本内） |
| 改为「区间覆盖即可」后装钩成功 | `hooked enter-interp @ 0x4000254b9a40 TRACE=0 DMG_WRITE=1` + 四项 patch 计数增长 + 用户终审可用 | PROVEN（样本内） |
| 该环境 Dobby inline hook 有效 | 同上（钩上后计数持续增长，FATAL/SIGSEGV/ANR=0） | PROVEN（样本内） |
| 改写后 houdini 会重译该代码块 | 无独立观测 | NOT PROVEN（推断） |
| 结论可外推到 MuMu/BlueStacks | 未测 | NOT PROVEN |

## 6. 生命周期

- 证据状态：样本内支持（单游戏、单模拟器、单版本；A/B 两次冷启对照）。
- 门槛状态：**已合并**（2026-11-27，tech 自批 auto:tech-promote；证据齐 + 落点明确）。
- 落地状态：已合并 → `hook-rules.md` §30.2（区间覆盖即成立 + `x` 段仅排序偏好 + 无 `x` 段 fallback）／
  §30.3（静默失败形态判据）／§30.4（环境记录加 maps 段权限集合）；`starry-mod/SKILL.md` 出包自检步骤 6（game_id 字节断言）。
- 合并提交：`fb590ac6`（收尾白名单提交，rebase 后哈希）。
- 重新验证触发：换模拟器/翻译层、宿主 ABI 变化、或首次遇到「无 x 段且装钩失败」。
