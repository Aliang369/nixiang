public class zbMonsterSkillIndex10112 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10112 = true;
		monster.monsterIndex10112Action = false;
		monster.monsterIndex10112SkillBase = this;
	}

	public override void ClearList()
	{
		monster.monsterIndex10112 = false;
		monster.monsterIndex10112Action = false;
		monster.monsterIndex10112SkillBase = null;
	}
}
