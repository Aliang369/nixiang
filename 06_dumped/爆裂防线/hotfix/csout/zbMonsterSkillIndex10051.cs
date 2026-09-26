public class zbMonsterSkillIndex10051 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10051 = true;
		monster.monsterIndex10051Read = monsterSkillRecord.param[0];
		monster.monsterIndex10051Time = monster.monsterIndex10051Read;
		monster.monsterIndex10051Flag = -1;
	}

	public override void ClearList()
	{
		monster.monsterIndex10051 = false;
	}
}
