using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;

public class zbSub10301 : zbSkillBase
{
	protected override void OnInited()
	{
		base.OnInited();
		if ((Object)(object)m_skeletonAnimation != (Object)null)
		{
			m_skeletonAnimation.AnimationState.SetAnimation(0, "animation", false);
		}
		skillSubRecord = zbDataConfig.Instance.skillSubCsvTable.GetRecordByKey(skillSubRecord.id);
		Dictionary<EAttrSkillType, float> dictionary = new Dictionary<EAttrSkillType, float>();
		zbCombatTeamAttr teamAttr = hero.team.teamAttr;
		ESkillType eSkillType = ESkillType.Dianci;
		foreach (stTeamAttrData teamBaseAttr in teamAttr.teamBaseAttrList)
		{
			if (teamBaseAttr.eSkillType == eSkillType)
			{
				dictionary[teamBaseAttr.eAttrSkillType] = teamBaseAttr.eValue;
			}
		}
		foreach (stTeamAttrData teamPercentAttr in teamAttr.teamPercentAttrList)
		{
			if (teamPercentAttr.eSkillType == eSkillType && dictionary.ContainsKey(teamPercentAttr.eAttrSkillType))
			{
				dictionary[teamPercentAttr.eAttrSkillType] *= 1f + teamPercentAttr.eValue;
			}
		}
		foreach (stTeamAttrData teamFixedAttr in teamAttr.teamFixedAttrList)
		{
			if (teamFixedAttr.eSkillType == eSkillType && dictionary.ContainsKey(teamFixedAttr.eAttrSkillType))
			{
				dictionary[teamFixedAttr.eAttrSkillType] += teamFixedAttr.eValue;
			}
		}
		int[] fatherVal = skillSubRecord.fatherVal;
		foreach (int key in fatherVal)
		{
			if (dictionary.ContainsKey((EAttrSkillType)key))
			{
				skillAttrList[(EAttrSkillType)key] = dictionary[(EAttrSkillType)key];
			}
			else
			{
				skillAttrList[(EAttrSkillType)key] = 0f;
			}
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(mainMonsterUUID);
		if ((Object)(object)monsterById != (Object)null)
		{
			CalcMonsterDamage(EAttrSkillType.Damage, monsterById);
		}
		ReturnToPool(0.4f);
	}
}
