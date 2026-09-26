using System.Collections.Generic;

public class zbWallSkillMgr
{
	public Dictionary<int, zbWallSkillHandleBase> defenseListP1;

	public Dictionary<int, zbWallSkillHandleBase> defenseListP2;

	public zbCombatData combatData;

	public void Init(zbCombatData _combatData)
	{
		combatData = _combatData;
		defenseListP1 = new Dictionary<int, zbWallSkillHandleBase>();
		defenseListP2 = new Dictionary<int, zbWallSkillHandleBase>();
	}

	public void Free()
	{
		defenseListP1.Clear();
		defenseListP2.Clear();
		defenseListP1 = null;
		defenseListP2 = null;
	}

	public void CreateDefense(int skillId, zbCombatTeam team, bool isP1)
	{
		zbWallSkillHandleBase zbWallSkillHandleBase2 = zbSkillSystem.Instance.defenseRegister.CreateDefense(skillId);
		if (zbWallSkillHandleBase2 != null)
		{
			zbWallSkillHandleBase2.Init(combatData, skillId, team, isP1);
			if (isP1)
			{
				defenseListP1[skillId] = zbWallSkillHandleBase2;
			}
			else
			{
				defenseListP2[skillId] = zbWallSkillHandleBase2;
			}
		}
	}

	public void Restore(zbCombatRecorder restoreData)
	{
		foreach (KeyValuePair<int, List<int>> item in restoreData.loadedData.recordDefense.wallSkillIndexsP1)
		{
			if (defenseListP1.TryGetValue(item.Key, out var value))
			{
				value.Restore(item.Value);
			}
		}
		foreach (KeyValuePair<int, List<int>> item2 in restoreData.loadedData.recordDefense.wallSkillIndexsP2)
		{
			if (defenseListP2.TryGetValue(item2.Key, out var value2))
			{
				value2.Restore(item2.Value);
			}
		}
	}

	public void Update()
	{
		if (defenseListP1 != null)
		{
			foreach (KeyValuePair<int, zbWallSkillHandleBase> item in defenseListP1)
			{
				item.Value.Update();
			}
		}
		if (defenseListP2 == null)
		{
			return;
		}
		foreach (KeyValuePair<int, zbWallSkillHandleBase> item2 in defenseListP2)
		{
			item2.Value.Update();
		}
	}
}
