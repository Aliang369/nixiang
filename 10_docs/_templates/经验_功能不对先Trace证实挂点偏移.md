# 经验：功能不对 → 先 Trace/log 证实挂点与偏移，再写功能

> 首次沉淀：弹力果冻 2.4.0 无限技能（2026-08-20）  
> 适用：HybridCLR 热更 / 单跳板 / 星光自注入 `libstarry_magic.so`

---

## 核心教训（一句话）

**功能无效或闪退时，禁止继续猜 hook 点、猜字段、换挂法试菜单；先用自注入 `StarryTrace`（`adb logcat -s StarryTrace`）把挂点、MI/MP、字段偏移、写入计数证实，再改 `Games/*Mod.cpp`。**

---

## 标准顺序（三阶段，不许跳）

| 阶段 | 做什么 | 产出 |
|------|--------|------|
| 1. 收集 | 开 Trace；log 打 `types ok`（offset/MI）、命中计数、`patch=` | 知道「有没有写到」「写到哪里」 |
| 2. 判断 | 对照反编译 / memdiff / 运行时 `FieldOffset`；标注 ⚠️ 待验证 | 明确无效是「未命中」还是「偏移错」 |
| 3. 行动 | 只改有数据支撑的一处；关 Trace 出正式 so | 功能可复现、可回归 |

---

## 弹力果冻无限技能：反面 vs 正面

| 乱猜（无效/闪退） | log/Trace 实锤后 |
|------------------|------------------|
| 跳板 **MethodPointer** 匹配共用 stub（ReCal/onEnter 同 MP） | 仅 **MI 指针**匹配 ReCal；CanUse 可用独占 MP |
| nocd 开时对**每个跳板调用**做 `IsPlayerSkillObj` | **先 MI 匹配**，再读 Skill 字段；拒 `0x2e/0x71` 标签指针 |
| `FieldOffHotfix` 默认 **+8 bias** → `cd=60` 写到 `skillTimeCounter` | bias **0** → `cd=52` 才是 `cdCounter` |
| `SkillContainer` 解析失败仍走 unit 链 → `run=0 sk=0`，**patch=0** | 布局兜底 + 玩家 **Skill 堆扫描**；log 里 `patch>0` 才验收 |
| 外部 **Frida** 长挂探针 | **自注入 Trace**；Frida 仅无壳短探针且会话结束清理 |

---

## Trace 最低清单（写功能前）

在 `ResolveTypes` / 跳板 / tick 路径至少 log 一次：

1. **类与方法**：`g_miCanUse`、`g_miReCalSkillCD`、`MethodPointer`（是否共用 stub）
2. **字段偏移**：`cdCounter`、`CanUseTimes`、`unit`、`runner/curSkill/_skills`（含兜底值）
3. **命中计数**：`nocd canuse n=`、`nocd recal n=`、`tick cd patch=N`（**N 必须 >0 才算写入成功**）
4. **战斗上下文**：`cache player=N`（nocd 门控）

---

## 硬禁（HybridCLR 单跳板）

- 无 log 命中 → 不换方案堆叠（tick 扫堆 + 跳板 MP + invoke 三连猜）
- **共有 MethodPointer 禁止 MP 过滤**（须 MI 或形态识别，参考 HpHandle DamageHandle）
- **热更 FieldOffset 禁止再 +8**（见 `skill/re-extract/hybridclr-runtime.md`）
- 默认 **禁止外部 frida-server**；动态分析优先自注入 Trace

---

## 关联

- 总纲：`skill/SKILL.md`「数据先于行动」
- Hook 铁律：`skill/re-implement/hook-rules.md`
- HybridCLR：`skill/re-extract/hybridclr-runtime.md`
- 弹力果冻定稿：`10_docs/弹力果冻/抓点定稿_20260819.md`
