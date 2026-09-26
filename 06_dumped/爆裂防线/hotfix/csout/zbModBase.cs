public class zbModBase
{
	public zbCombatData combatData;

	public zbCombatTeam team;

	public zbModCsvRecord modRecord;

	public bool isUserSelect;

	public int level;

	public void Init(zbCombatTeam _team, int _modId, int _level, bool _isUserSelect = false)
	{
		team = _team;
		combatData = team.combatData;
		isUserSelect = _isUserSelect;
		level = _level;
		modRecord = zbDataConfig.Instance.modCsvTable.GetRecordByKey(_modId);
		RefreshProperty();
		foreach (zbHero hero in _team.heroList)
		{
			hero.OnAfterModCreated();
		}
	}

	public virtual void RefreshProperty()
	{
		team.teamAttr.RefreshAttr(modRecord);
	}
}
