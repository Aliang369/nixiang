public class zbMonsterSkillIndex20132 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex20132.skillIndexFlag = true;
		monster.monsterIndex20132.monsterTid = (int)monsterSkillRecord.param[0];
		monster.monsterIndex20132.attrMonsterType = (int)monsterSkillRecord.param[1];
		monster.monsterIndex20132.attrPercent = monsterSkillRecord.param[2];
	}

	public override void ClearList()
	{
	}
}
