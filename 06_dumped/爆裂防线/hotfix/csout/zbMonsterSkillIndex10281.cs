public class zbMonsterSkillIndex10281 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10281 stMonsterSkillIndex40463 = new stMonsterSkillIndex10281();
		stMonsterSkillIndex40463.attackType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.attrPercent = monsterSkillRecord.param[1];
		monster.monsterIndex10281.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10281.Clear();
	}
}
