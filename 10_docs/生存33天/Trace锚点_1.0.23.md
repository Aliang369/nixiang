# 生存33天 1.0.23 — eval 锚点定稿（Trace 实锤）

> 2026-08-24 · 来源：STARRY_TRACE 运行时 eval dump（27221335 B，本地已清理）  
> prefs/get_ret 编号仍见 `抓点定稿_20260824.md`（实锤）

## 实锤 vs 废弃（镇邪人/Zxr 模板）

| 锚点 | 1.0.23 | 镇邪人 ZxrMod | 结论 |
|------|--------|---------------|------|
| `t=c.baseDmg` | ❌ 0 次 | ✅ | 改用 **`t = c.baseDmg;`**（有空格） |
| `var f=o&&o.force` | ❌ 0 次 | ✅ AOE 前 | 改用 **`var p = o && o.force, d = o && o.trueDmg, h = i < 0...`** |
| `e=Math.max(e,.01)` getAtk | ❌ 0 次 | ✅ 攻速 | 改用 **`CalcFinalAtkSpeedValue`** 内 `return Math.max(e, .01)` |
| `d.EntityManager` | ❌ 本函数作用域无 | 误用 | 本包 **`f.EntityManager`** |
| `FightChangeEntityHp(aoeEnt,e,i,g,v)` | — | — | 应为 **`(aoeEnt,e,i,o,n)`** 五参 |

## 实锤锚点（偏移基于 dump）

### 1. 百倍 / 无敌 — `DoSkillFinalDmgCalc` 后

- **锚点**：`t = c.baseDmg;` @ **19760498**
- **上下文变量**：`n`=casterEntity，`r`=targetEntity，`t`=伤害值
- **插入**（赋值后）：
  - get_ret(1) → `n.heroCfg && r.monsterCfg` 时 `t*=100`
  - get_ret(2) → `r.heroCfg` 时 `t=0`

### 2. 全屏 / 战斗内倍伤 — `FightChangeEntityHp`

- **函数**：`prototype.FightChangeEntityHp = function(t, e, i, o, n)` @ **8304235**
- **锚点**：`var p = o && o.force, d = o && o.trueDmg, h = i < 0, _ = i > 0, g = 0;` @ **8304848**
- **变量**：`t`=目标实体，`e`=施法者 ID，`i`=HP 变化（负=伤害），`h`=是否伤害
- **全屏 get_ret(3)**：`h && t.monsterCfg`，施法者 `f.EntityManager.Inst().GetEntity(e).heroCfg`，遍历 `entityMap`，递归 `FightChangeEntityHp(aoeEnt,e,i,o,n)`
- **战斗倍伤/无敌**：同上函数内 `i*=100` / `i=0`（与 baseDmg 路径双保险）

### 3. 攻速 — `CalcFinalAtkSpeedValue`

- **锚点**：`t && t.CheckEntityHaveStatus(l.EnumEntityStatus.bossFakeControl) && (e = .01);` @ **8337108** 附近
- **攻速非 getAtk**：在 `CalcFinalAtkSpeedValue(t,e)` 内，`bossFakeControl` 判断之后、`return Math.max(e, .01)` 之前
- **实现**：`get_ret(5)` 门控 + 读 `Sc33Hooks.getAtkMul()` / `cc.sys.localStorage('sc33_atk_mul')`；倍率在 `Math.min` **之后**应用（绕过 cap=3 压制）
- **双路径**：`CalcFinalAtkSpeedValue` + `GetViewFightTimeRate`

## 构建开关

| 模式 | 宏 | 行为 |
|------|-----|------|
| Trace | `STARRY_TRACE=1` | dump + 锚点扫描，**不 splice** |
| 正式 | `SC33_SPLICE_ENABLED=1` | 按上表 splice |

## 验证清单

- [ ] log：`splice detail fight=1 atk=1 legacyDmg=1`
- [ ] 无 `SyntaxError` / 怪物可正常死亡（关 mod）
- [ ] 百倍 / 无敌 / 全屏 / 攻速滑条逐项真机
