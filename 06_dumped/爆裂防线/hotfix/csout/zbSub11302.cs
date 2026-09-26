using Proto.Enums;
using UnityEngine;

public class zbSub11302 : zbSkillBase
{
	protected override void OnInited()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		base.OnInited();
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			Vector3 targetNormalized = zbUtil.GetTargetNormalized(monsterById.GetCurPosition(), ((Component)this).transform.position);
			zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
			zbExtraDirection2.direction = targetNormalized;
			zbExtraDirection2.duration = 0.5f;
			zbExtraDirection2.speed = GetAttrValueByType(EAttrSkillType.Tow) / (1f + monsterById.GetMass() / 100f) / zbExtraDirection2.duration * (monsterById.m_KnockbackTowUpPercent + 1f);
			if ((Object)(object)parentSkillBase != (Object)null && parentSkillBase.isMainSkill && hero.GetSkillType() == 13 && zbCombatSystem.Instance.buffSystem.IsMonsterHaveAnyBuff(monsterById.GetUUID()))
			{
				zbExtraDirection2.speed *= 1f + hero.team.ExtraProperties[ETeamExtra.LongjuanMod2132];
			}
			monsterById.AddExtraDirectionList(zbExtraDirection2);
		}
		ReturnToPool(0.01f);
	}
}
