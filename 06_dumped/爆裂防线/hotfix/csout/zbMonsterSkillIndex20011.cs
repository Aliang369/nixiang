public class zbMonsterSkillIndex20011 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex20011 = true;
	}

	public override void ClearList()
	{
		monster.monsterIndex20011 = false;
	}
}
