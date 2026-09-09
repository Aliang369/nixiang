# 弓箭传说2国际版 1.0.1 Trace 实锤（2026-08-23）

> 来源：`StarryTrace` 自注入，`adb logcat -s StarryTrace | grep Gjc2Intl`  
> 原始 log：收尾已按 TOOLBOOK 清理 `06_dumped/`（结论保留本文）  
> Il2CppDumper 对 `global-metadata.dat`：**加密失败** → 只能运行时 dump

## 与国服 1.1.8 差异（数据）

| 项 | 国服 gjc2 | 国际 gjc2intl 1.0.1 |
|----|-----------|----------------------|
| 全屏敌人列表 | `EntityManager.GetCachedCharacterList()` | **不存在**；用 `BattleWorldContext.get_TmpEntityCharacterList()` 或字段 `_tmpCharacters@0x10` |
| SelectSlotCommand | 含 RandomSkillResult 链 | **仅** `SkillId@0x20`、`PlayerId@0x24` + `Exec()` |
| 偷技能候选 | `RandomSkillResult.get_ShowSkills` | 同 + `HeroComponentRandomSkill.get_RandomSkillResult` + 两个 `AddSkill(1)` |
| EntityManager 字段 | `_cachedCharacterList` 等 | `_entityDic@0x10`、**无** cached character list |

## 全屏攻击挂点（实锤）

```
BattleWorldContext.get_TmpEntityCharacterList(0) mp=0x...
BattleWorldContext._tmpCharacters off=0x10
```

## 偷技能挂点（实锤）

```
RandomSkillResult.get_ShowSkills(0)
RandomSkillResult.<ShowSkills>k__BackingField off=0x40
SelectSlotCommand.SkillId off=0x20
SelectSlotCommand.PlayerId off=0x24
SelectSlotCommand.Exec(0)
HeroComponentRandomSkill.get_RandomSkillResult(0)
HeroComponentRandomSkill._randomSkillResult off=0x78
HeroComponentRandomSkill.AddSkill(1) ×2（不同 MP）
EntityManager.GetPlayer(1)
BattleWorldContext.AddSkillToCharacter(3) — 仍存在，hook 已保留
```

## Mod 改动（按上表）

1. **全屏**：`UpdateHp` 扩散改读 `get_TmpEntityCharacterList` / `_tmpCharacters@0x10`
2. **偷技能**：`get_RandomSkillResult` 缓存 ShowSkills；`Exec` 后按 `SkillId` + `GetPlayer` 补技能；`HeroComponentRandomSkill.AddSkill` 双 overload 兜底

## 验证 log 期望

- `[Gjc2Intl] aoe list get_TmpEntityCharacterList -> 0x...`
- `[Gjc2Intl] aoe spread=N`
- `[Gjc2Intl] steal skills +N` 或 `steal hero AddSkill +N`
