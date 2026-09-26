using System.Collections.Generic;

public class zbWallSkillHandleBase
{
	public zbCombatData combatData;

	public zbCombatTeam team;

	public zbWallSkillCsvRecord wallSkillRecord;

	public bool isP1;

	public zbWallSkillDataBase wallSkillDataBase;

	public void Init(zbCombatData _combatData, int _wallSkillId, zbCombatTeam _team, bool _isP1)
	{
		combatData = _combatData;
		team = _team;
		isP1 = _isP1;
		wallSkillRecord = zbDataConfig.Instance.wallSkillCsvTable.GetRecordByKey(_wallSkillId);
		RefreshProperty();
	}

	public virtual void RefreshProperty()
	{
	}

	public virtual void TriggerAction(zbMonster attacker = null)
	{
	}

	public virtual void UnRestoreData()
	{
	}

	public virtual void Restore(List<int> list)
	{
		wallSkillDataBase.Restore(list);
	}

	public virtual void Update()
	{
	}
}
