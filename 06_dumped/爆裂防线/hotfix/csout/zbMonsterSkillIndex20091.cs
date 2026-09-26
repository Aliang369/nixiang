public class zbMonsterSkillIndex20091 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex20091.monsterSkill = this;
		monster.monsterIndex20091.skillSubTid = (int)monsterSkillRecord.param[0];
	}

	public override void ClearList()
	{
	}
}
