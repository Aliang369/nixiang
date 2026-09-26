public class zbMonsterSkillIndex10382 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10382.distance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex10382.damagePercent = monsterSkillRecord.param[1];
		monster.monsterIndex10382.speed = 8f;
		monster.monsterIndex10382.skillIndexFlag = true;
		monster.monsterIndex10382.beginMove = false;
		monster.monsterIndex10382.monsterSkill = this;
	}

	public override void ClearList()
	{
	}
}
