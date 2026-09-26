using System;
using Proto.Enums;
using UnityEngine;

public class zbBuffIndex10 : zbBuffBase
{
	private float damageInterval;

	private float currInterval;

	public override void Reset()
	{
		base.Reset();
		currInterval = 0f;
		damageInterval = 0f;
	}

	public override void RefreshProperty()
	{
		damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval) + 0.5f;
		durationTime = extraTime;
		zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			damage = Math.Min((float)(long)monsterById.maxHp * buffRecord.param, (float)(int)skillBase.hero.team.teamAttack * buffRecord.param * 62.5f);
		}
	}

	public override void Update()
	{
		base.Update();
		currInterval += Time.deltaTime;
		if (currInterval >= damageInterval)
		{
			zbMonster monsterById = combatData.monsters.GetMonsterById(monsterUUID);
			if ((Object)(object)monsterById != (Object)null)
			{
				zbCombatSystem.Instance.combatFormula.CalcBuffMonsterDamage(skillBase, monsterById, damage, buffRecord.attackType);
			}
			currInterval = 0f;
		}
	}
}
