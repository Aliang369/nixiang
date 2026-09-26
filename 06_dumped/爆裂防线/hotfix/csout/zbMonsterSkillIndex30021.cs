public class zbMonsterSkillIndex30021 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex30021.monsterIndex30021Flag = true;
		monster.monsterIndex30021.distance = monsterSkillRecord.param[0] / 100f;
	}

	public override void ClearList()
	{
	}
}
