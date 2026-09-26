using Proto.Enums;
using UnityEngine;

public class zbBuffIce : zbBuffBase
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
		damageInterval = GetAttrValueByType(EAttrSkillType.DamageInterval);
		durationTime = GetAttrValueByType(EAttrSkillType.IceTime) + extraTime;
		damage += GetAttrValueByType(EAttrSkillType.IceDamage);
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
