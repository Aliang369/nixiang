using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbHeroAttr
{
	public zbAttackerBase hero;

	public Dictionary<EAttrSkillType, float> skillAttrList = new Dictionary<EAttrSkillType, float>();

	private const int ModeNone = 0;

	private const int ModeHero = 1;

	private const int ModeMecha = 2;

	private const int ModeWall = 3;

	private const int ModeFoothold = 4;

	private int lastRefreshMode;

	private Dictionary<EAttrSkillType, float> recomputeCache = new Dictionary<EAttrSkillType, float>();

	public virtual void Init(zbAttackerBase _hero)
	{
		hero = _hero;
	}

	public float GetAttrValueByType(EAttrSkillType attrSkillType)
	{
		if (skillAttrList.TryGetValue(attrSkillType, out var value))
		{
			return zbSkillSystem.Instance.SafeCheckAttrNeedDiv100(attrSkillType, value);
		}
		return 0f;
	}

	public void RefreshHeroAttr(ref float sum)
	{
		zbCombatTeamAttr teamAttr = hero.team.teamAttr;
		int skillType = hero.GetSkillType();
		foreach (stTeamAttrData teamBaseAttr in teamAttr.teamBaseAttrList)
		{
			if (teamBaseAttr.eSkillType == (ESkillType)skillType)
			{
				skillAttrList[teamBaseAttr.eAttrSkillType] = teamBaseAttr.eValue;
			}
		}
		foreach (stTeamAttrData teamPercentAttr in teamAttr.teamPercentAttrList)
		{
			if (teamPercentAttr.eSkillType == (ESkillType)skillType && skillAttrList.ContainsKey(teamPercentAttr.eAttrSkillType))
			{
				skillAttrList[teamPercentAttr.eAttrSkillType] *= 1f + teamPercentAttr.eValue;
			}
		}
		foreach (stTeamAttrData teamFixedAttr in teamAttr.teamFixedAttrList)
		{
			if (teamFixedAttr.eSkillType == (ESkillType)skillType && skillAttrList.ContainsKey(teamFixedAttr.eAttrSkillType))
			{
				skillAttrList[teamFixedAttr.eAttrSkillType] += teamFixedAttr.eValue;
			}
		}
		foreach (KeyValuePair<EAttrSkillType, float> skillAttr in skillAttrList)
		{
			sum += skillAttr.Value;
		}
		lastRefreshMode = 1;
	}

	public void RefreshMechaAttr()
	{
		foreach (BattleMechaAttrDetail attr in hero.team.battleTeamInfo.MechaInfo.AttrList)
		{
			skillAttrList[attr.Type] = attr.Value;
		}
		lastRefreshMode = 2;
	}

	public void RefreshWallAttr()
	{
		skillAttrList[EAttrSkillType.Damage] = 100f;
		lastRefreshMode = 3;
	}

	public void RefreshFootholdAttr()
	{
		skillAttrList[EAttrSkillType.Damage] = 100f;
		lastRefreshMode = 4;
	}

	public bool CheckAttrByRecompute(out EAttrSkillType attrType, out float expected, out float live)
	{
		attrType = EAttrSkillType.EmptyValue;
		expected = 0f;
		live = 0f;
		if (lastRefreshMode == 0 || (Object)(object)hero == (Object)null)
		{
			return false;
		}
		recomputeCache.Clear();
		switch (lastRefreshMode)
		{
		case 1:
		{
			zbCombatTeamAttr teamAttr = hero.team.teamAttr;
			int skillType = hero.GetSkillType();
			foreach (stTeamAttrData teamBaseAttr in teamAttr.teamBaseAttrList)
			{
				if (teamBaseAttr.eSkillType == (ESkillType)skillType)
				{
					recomputeCache[teamBaseAttr.eAttrSkillType] = teamBaseAttr.eValue;
				}
			}
			foreach (stTeamAttrData teamPercentAttr in teamAttr.teamPercentAttrList)
			{
				if (teamPercentAttr.eSkillType == (ESkillType)skillType && recomputeCache.ContainsKey(teamPercentAttr.eAttrSkillType))
				{
					recomputeCache[teamPercentAttr.eAttrSkillType] *= 1f + teamPercentAttr.eValue;
				}
			}
			foreach (stTeamAttrData teamFixedAttr in teamAttr.teamFixedAttrList)
			{
				if (teamFixedAttr.eSkillType == (ESkillType)skillType && recomputeCache.ContainsKey(teamFixedAttr.eAttrSkillType))
				{
					recomputeCache[teamFixedAttr.eAttrSkillType] += teamFixedAttr.eValue;
				}
			}
			break;
		}
		case 2:
			foreach (BattleMechaAttrDetail attr in hero.team.battleTeamInfo.MechaInfo.AttrList)
			{
				recomputeCache[attr.Type] = attr.Value;
			}
			break;
		case 3:
		case 4:
			recomputeCache[EAttrSkillType.Damage] = 100f;
			break;
		}
		foreach (KeyValuePair<EAttrSkillType, float> item in recomputeCache)
		{
			if (!skillAttrList.TryGetValue(item.Key, out var value))
			{
				attrType = item.Key;
				expected = item.Value;
				live = 0f;
				return true;
			}
			float num = Math.Max(1f, Math.Abs(item.Value) * 0.05f);
			if (Math.Abs(value - item.Value) > num)
			{
				attrType = item.Key;
				expected = item.Value;
				live = value;
				return true;
			}
		}
		return false;
	}
}
