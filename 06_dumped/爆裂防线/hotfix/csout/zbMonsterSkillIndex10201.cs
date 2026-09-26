public class zbMonsterSkillIndex10201 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10201 stMonsterSkillIndex40463 = new stMonsterSkillIndex10201();
		stMonsterSkillIndex40463.unAttackType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.monsterNum = (int)monsterSkillRecord.param[1];
		stMonsterSkillIndex40463.monsterTid = (int)monsterSkillRecord.param[2];
		monster.monsterIndex10201.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10201.Clear();
	}
}
