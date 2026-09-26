public class zbMonsterSkillIndex10101 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10101 = true;
	}

	public override void ClearList()
	{
		monster.monsterIndex10101 = false;
	}
}
