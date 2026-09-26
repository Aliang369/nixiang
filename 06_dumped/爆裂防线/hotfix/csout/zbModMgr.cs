using System.Collections.Generic;

public class zbModMgr
{
	public List<zbModBase> modList;

	public zbCombatTeam team;

	public void Init(zbCombatTeam _team)
	{
		team = _team;
		modList = new List<zbModBase>();
	}

	public void Free()
	{
		modList.Clear();
		modList = null;
	}

	public void CreateMod(int modId, int level, bool isUserSelect = false)
	{
		zbModBase zbModBase2 = zbSkillSystem.Instance.modRegister.CreateMod(modId);
		if (zbModBase2 != null)
		{
			zbModBase2.Init(team, modId, level, isUserSelect);
			modList.Add(zbModBase2);
		}
	}

	public bool IsModIdSelMax(int modId, int maxNum)
	{
		int num = 0;
		foreach (zbModBase mod in modList)
		{
			if (mod.modRecord.id == modId)
			{
				num++;
			}
		}
		return num >= maxNum;
	}

	public bool HasMod(int modId)
	{
		foreach (zbModBase mod in modList)
		{
			if (mod.modRecord.id == modId)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasModIndex(int modIndexId)
	{
		foreach (zbModBase mod in modList)
		{
			if (mod.modRecord.modIndex == modIndexId)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasSkillId(int skillId)
	{
		foreach (zbHero hero in team.heroList)
		{
			if (hero.GetSkillId() == skillId)
			{
				return true;
			}
		}
		return false;
	}
}
