public class zbMonsterSkillIndex10332 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10332 stMonsterSkillIndex40463 = new stMonsterSkillIndex10332();
		stMonsterSkillIndex40463.attackType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.roll0to1 = monsterSkillRecord.param[1];
		stMonsterSkillIndex40463.subCount = (int)monsterSkillRecord.param[2];
		stMonsterSkillIndex40463.subSkillId = (int)monsterSkillRecord.param[3];
		monster.monsterIndex10332.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10332.Clear();
	}
}
