public class zbMonsterSkillIndex10302 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10302 = (int)monsterSkillRecord.param[0];
	}

	public override void ClearList()
	{
		monster.monsterIndex10302 = 0;
	}
}
