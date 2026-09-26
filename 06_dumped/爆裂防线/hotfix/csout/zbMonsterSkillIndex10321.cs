public class zbMonsterSkillIndex10321 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10321 stMonsterSkillIndex40463 = new stMonsterSkillIndex10321();
		stMonsterSkillIndex40463.attackType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.roll0to1 = monsterSkillRecord.param[1];
		stMonsterSkillIndex40463.damagePercent = monsterSkillRecord.param[2];
		monster.monsterIndex10321.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10321.Clear();
	}
}
