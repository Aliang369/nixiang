public class zbMonsterSkillIndex10262 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10262 stMonsterSkillIndex40463 = new stMonsterSkillIndex10262();
		stMonsterSkillIndex40463.skillType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.roll0to1 = monsterSkillRecord.param[1];
		monster.monsterIndex10262.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10262.Clear();
	}
}
