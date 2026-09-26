public class zbMonsterSkillIndex10261 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		stMonsterSkillIndex10261 stMonsterSkillIndex40463 = new stMonsterSkillIndex10261();
		stMonsterSkillIndex40463.skillType = (int)monsterSkillRecord.param[0];
		stMonsterSkillIndex40463.damagePercent = monsterSkillRecord.param[1];
		monster.monsterIndex10261.Add(stMonsterSkillIndex40463);
	}

	public override void ClearList()
	{
		monster.monsterIndex10261.Clear();
	}
}
