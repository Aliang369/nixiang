public class zbMonsterSkillIndex40111 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex40111.attackNum = (int)monsterSkillRecord.param[0];
		monster.monsterIndex40111.skillIndexFlag = true;
		monster.monsterIndex40111.monsterSkill = this;
	}

	public override void ClearList()
	{
	}
}
