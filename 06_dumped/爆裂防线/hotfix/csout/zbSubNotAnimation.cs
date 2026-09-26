using Proto.Enums;
using UnityEngine;

public class zbSubNotAnimation
{
	public static void CalSubNotAnimation(int subBulletId, zbAttackerBase hero, int mainMonsterUUID, Vector3 transformPosition, zbSkillBase parentSkillBase, Vector2 dir)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		switch (subBulletId)
		{
		case 11302:
			Cal11302(hero, mainMonsterUUID, transformPosition, parentSkillBase);
			break;
		case 10203:
			Cal10203(hero, mainMonsterUUID, transformPosition, parentSkillBase, dir);
			break;
		}
	}

	public static void Cal11302(zbAttackerBase hero, int mainMonsterUUID, Vector3 transformPosition, zbSkillBase parentSkillBase)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = hero.combatData.monsters.GetMonsterById(mainMonsterUUID);
		if (!((Object)(object)monsterById != (Object)null))
		{
			return;
		}
		Vector3 targetNormalized = zbUtil.GetTargetNormalized(monsterById.GetCurPosition(), transformPosition);
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		zbExtraDirection2.direction = targetNormalized;
		zbExtraDirection2.duration = 0.5f;
		zbExtraDirection2.speed = 0f;
		if ((Object)(object)parentSkillBase != (Object)null)
		{
			float attrValueByType = parentSkillBase.GetAttrValueByType(EAttrSkillType.Tow);
			float num = 1f + monsterById.GetMass() / 100f;
			float speed = attrValueByType / num / zbExtraDirection2.duration * (monsterById.m_KnockbackTowUpPercent + 1f);
			zbExtraDirection2.speed = speed;
			if (parentSkillBase.isMainSkill && hero.GetSkillType() == 13 && zbCombatSystem.Instance.buffSystem.IsMonsterHaveAnyBuff(monsterById.GetUUID()))
			{
				zbExtraDirection2.speed *= 1f + hero.team.ExtraProperties[ETeamExtra.LongjuanMod2132];
			}
		}
		monsterById.AddExtraDirectionList(zbExtraDirection2);
	}

	public static void Cal10203(zbAttackerBase hero, int mainMonsterUUID, Vector3 transformPosition, zbSkillBase parentSkillBase, Vector2 dir)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		zbMonster monsterById = hero.combatData.monsters.GetMonsterById(mainMonsterUUID);
		if (!((Object)(object)monsterById != (Object)null) || monsterById.monsterIndex10171)
		{
			return;
		}
		Vector3 direction = Vector2.op_Implicit(dir);
		if (hero.GetSkillType() == 2)
		{
			direction = zbUtil.GetTargetNormalized(transformPosition, monsterById.GetCurPosition());
		}
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		zbExtraDirection2.direction = direction;
		zbExtraDirection2.duration = 0.1f;
		zbExtraDirection2.speed = 0f;
		if ((Object)(object)parentSkillBase != (Object)null)
		{
			zbExtraDirection2.speed = parentSkillBase.GetAttrValueByType(EAttrSkillType.Knockback) / (1f + monsterById.GetMass() / 100f) / zbExtraDirection2.duration * (monsterById.m_KnockbackTowUpPercent + 1f);
		}
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
}
