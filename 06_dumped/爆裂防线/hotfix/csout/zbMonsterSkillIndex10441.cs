public class zbMonsterSkillIndex10441 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10441.horizontalRecordSpeed1 = monsterSkillRecord.param[0];
		monster.monsterIndex10441.horizontalRecordSpeed2 = monsterSkillRecord.param[1];
		monster.monsterIndex10441.horizontalSpeed = Rand.Range(monster.monsterIndex10441.horizontalRecordSpeed1, monster.monsterIndex10441.horizontalRecordSpeed2 + 0.1f) / 100f;
		if (Rand.Range(0, 2) == 0)
		{
			monster.monsterIndex10441.horizontalSpeed = 0f - monster.monsterIndex10441.horizontalSpeed;
		}
		PlayTackAnimation(1, flag: false);
	}
}
