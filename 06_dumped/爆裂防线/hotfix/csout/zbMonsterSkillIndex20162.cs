public class zbMonsterSkillIndex20162 : zbMonsterSkillBase
{
	public stMonsterSkillIndex20162 data = new stMonsterSkillIndex20162();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.hpWarning = monsterSkillRecord.param[0];
		data.attrMonsterType = (int)monsterSkillRecord.param[1];
		data.attrPercent = monsterSkillRecord.param[2];
		monster.monsterIndex20162.Add(data);
	}

	public override void ClearList()
	{
		data = null;
		monster.monsterIndex20162.Clear();
	}
}
