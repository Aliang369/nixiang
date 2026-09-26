public class zbMonsterSkillIndex10000 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10000.attrPercent = monsterSkillRecord.param[0];
	}
}
