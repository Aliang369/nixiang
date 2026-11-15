# 08 移速 bug 修复（v1.4 实施记录）

> 触发：用户实测「滑动到 1 还是速度变得很快，滑动到后面的速度都是一个速度」。
> 结论：不是「写错了字段」，而是**写入时序 + base 污染 + 派生值覆盖**三件事叠在一起。
> 本文记录 v1.4 的修法与你（lead）点名的三处硬要求落地位置，供明天复盘与 v1.5 判断。

---

## 1. 用户反馈拆成两条症状

| 症状 | 直译 | 代码含义 |
|---|---|---|
| 滑块=1 速度仍很快 | 该「不写 + 还原」的分支没生效 | 要么从没还原过，要么还原写回的是一个**已经被放大的 base** |
| 滑块加大速度不变 | 倍率没作用到「被读取的那个值」 | 写进去的值被每帧重算覆盖，或 base 被冻结成放大值后所有倍率都以它为基准（观感趋同） |

---

## 2. 三处根因与修法（逐条对应你给的 1)2)3)）

### 2.1 base 捕获时机（对应 1）

**旧实现的问题**：base 在「第一次读到该字段」时就锁定，而这个读可能发生在
① 战斗加载期（值还没初始化）、② 上一轮写入之后（值已被放大）——两种都会把脏值当基准，
再乘倍率只会「越乘越离谱」，滑块变化在观感上被摊平。

**v1.4 做法**
- **写之前**才捕获，且要求**连续 2 次 tick 读到同一值**（±1e-4）才锁定 ⇒ 天然避开战斗加载抖动。
- 锁定后打 `speed base captured base=.. at=<ms>（写前锁定，之后不再变）`。
- 未锁定时不写，打 `speed wait-stable cur=.. n=1`。
- base 只有两种情况会变：
  1. `oknd` 换了对象（换角色/重开）⇒ `speed base re-captured（oknd 变化）`；
  2. 外部把字段改成一个**既不是 base、也不在 base×mult 集合内**的值 ⇒ `speed base re-captured（外部改动）`。
  其余情况（等于 base、等于某个 base×mult）一律视为「我们自己写的」，不动 base。

### 2.2 滑块=1 必须不写且还原（对应 2）

**v1.4 做法**
- 函数入口第一条：`if (slider <= 1) { if (写过或持有 oknd) SpeedRestoreVerify(); return; }`。
- `SpeedRestoreVerify()` 写回 base 后**读回确认**：`|back - base| <= 1e-3` 才算成功；
  不相等最多重试 **3 次**，每次打 `speed restore retry=N back=.. base=..`。
- 成功打 `speed restore ok base=..`；三次都失败打 `speed restore fail（读回不一致，字段疑为派生值）`。
- 还原成功后清空 `wrote / oknd / base / lock / stable` 五个状态，避免「幽灵 base」。
- `ForceClear`（授权失效/清空）走同一个函数，行为一致。

### 2.3 派生值覆盖检测（对应 3）

**v1.4 做法**
- 每 tick 读 `oknd+0x48`，与 `target = base×mult` 比：`|cur - target| > 0.01` ⇒ 判定需要(重)写。
- 写完**立刻读回**，把 `cur`（写前）与 `back`（写后读回）一起打进日志：
  `write ok speed oknd+0x48 base=.. mult=.. target=.. cur=.. back=..`
- 若「上一 tick 已经写过、这一 tick 又发现偏离」⇒ 单独打
  `speed drift cur=.. target=.. back=..（值被覆盖 ⇒ 疑派生值）`（5s 节流）。
  **这一条是判定依据**：drift 频繁 = 0x48 是每帧重算的派生值，v1.5 必须换写入点。
- 不需要写的 tick 打 `speed steady base=.. mult=.. target=.. cur=..`（同样 5s 节流），
  用来确认「值稳定持有」而不是「一直没生效」。

---

## 3. 倍率映射（本次按你的保守线性改）

| slider | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 |
|---|---|---|---|---|---|---|---|---|---|---|
| mult | 不写 | 1.25 | 1.50 | 1.75 | 2.00 | 2.25 | 2.50 | 2.75 | 3.00 | 3.25 |

公式：`mult = 1.0 + (slider - 1) × 0.25`。
写入钳制：`target ∈ [base×0.5, base×5.0]`，越界打 `speed clamp target→..（下限/上限）`
（**夹到边界并写明方向**，不静默丢弃，便于日志回溯）。

---

## 4. 首次进战斗不写（硬要求 1）

三重守卫，缺一不写：
1. 必须命中 `faction == 1` 的单位（`CollectUnits` 已过滤脏 faction）；
2. `oknd` 必须非空且 `+0x48` 可读；
3. 值必须**连续 2 次 tick 相同**（`speed wait-stable` 计数）⇒ 战斗加载期的 0 / 抖动值不会被当 base。

---

## 5. hp_cand 探测（保留，只读）

未改动：仅首个 `faction==1` 单位，采 `Unit+0x00..0x2C0` 与 `state+0x00..0x200`（步长 8），
整数位点 `hp_cand`、float 位点 `hp_cand_f`，每偏移 10s 一条、整包 200 条/分钟上限。

---

## 6. 明日验机时怎么读日志（判据）

| 现象 | 日志 | 结论 |
|---|---|---|
| 开局就快 | `speed base captured base=5.50` 出现在「角色已可控之后」 | 正常；若 `base` 已是放大值（>5.5 明显）⇒ 捕获仍晚，需要再加「等 oknd 稳定 N 秒」
| 滑块=1 | `speed restore ok base=5.50` 且随后 `speed steady ... target=5.50 cur=5.50` | 还原生效 |
| 滑块=1 仍快 | `speed restore fail` 或 `restore retry=3` | 字段被派生覆盖，还原无效 ⇒ v1.5 换写入点 |
| 滑块=4 | `write ok speed ... base=5.50 mult=1.75 target=9.63 cur=9.63 back=9.63` | 倍率真作用到该字段 |
| 滑块=4 但无感 | 连续 `speed drift cur=9.63 target=9.63 back=5.50` | 写入被每帧覆盖 ⇒ 0x48 是派生值，转 ②/③ 路线 |

---

## 6.1 L3 结论（2026-11-15 下午，用户实测）

> 用户原话：**「移动速度没问题了」**。

- 结论：v1.4 的三处修复（base 写前锁定 / 滑块=1 还原并读回确认 / drift 检测）**全部生效**，移速项验收通过。
- 依据：滑块=1 恢复原速；滑块 2-10 逐级生效且随滑块变化（不再「一个速度」）。
- 后续：移速逻辑**冻结**，v1.5 起不再改动（v1.5 只新增 hp198 三态机）。

## 7. 冻结与产物

| 版本 | 包 | md5 | so |
|---|---|---|---|
| v1.3（移速实验，已被本版取代） | abssnmxt_2.36_v1.3.apk | `73710205…` | 327,824 B / `8744d8b0…` |
| **v1.4（本版）** | abssnmxt_2.36_v1.4.apk | `5116a48fd8036360f96bccbf5a085ffe` | 333,040 B / `50cbf9d4cfcb24cb0eb81c82627fc065` |

v1.4 sha256：`c0ce582b27aefad10ab65c01f5632adc825ee50e30ac1bff579d71875b788b3d`，字节 1,133,614,390。
so 内仍**无**无敌/伤害倍率/技能写字符串（`#if !STARRY_PROBE && !STARRY_SPEED_ONLY` 砍掉），
有 `speed base captured / wait-stable / restore ok / restore retry / drift / clamp / steady / write ok speed / hp_cand`。