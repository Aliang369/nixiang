using Proto.Enums;
using UnityEngine;

public class zbSub10203 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null && !monsterById.monsterIndex10171)
		{
			Vector3 direction = getDirection();
			if (hero.GetSkillType() == 2)
			{
				direction = zbUtil.GetTargetNormalized(((Component)this).transform.position, monsterById.GetCurPosition());
			}
			zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
			zbExtraDirection2.direction = direction;
			zbExtraDirection2.duration = 0.1f;
			zbExtraDirection2.speed = GetAttrValueByType(EAttrSkillType.Knockback) / (1f + monsterById.GetMass() / 100f) / zbExtraDirection2.duration * (monsterById.m_KnockbackTowUpPercent + 1f);
			if ((Object)(object)parentSkillBase != (Object)null && parentSkillBase.isMainSkill)
			{
				foreach (stModIndex2142 item in hero.team.modIndex2142)
				{
					if (hero.GetSkillType() == item.skillType && zbCombatSystem.Instance.buffSystem.IsMonsterHaveBuff(monsterById.GetUUID(), item.srcBuffId) != null)
					{
						zbExtraDirection2.speed *= 1f + item.knockbackPercent;
					}
				}
				if (hero.team.modIndex2144.skillType == hero.GetSkillType() && hero.team.modIndex2144.knockbackPrecent > 0f)
				{
					zbExtraDirection2.speed *= 1f + hero.team.modIndex2144.knockbackPrecent;
				}
			}
			monsterById.AddExtraDirectionList(zbExtraDirection2);
		}
		ReturnToPool(0.01f);
	}
}
