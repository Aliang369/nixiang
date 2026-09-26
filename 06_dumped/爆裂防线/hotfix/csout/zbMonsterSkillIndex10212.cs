public class zbMonsterSkillIndex10212 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		float[] param = monsterSkillRecord.param;
		foreach (float num in param)
		{
			monster.immBuffList.Add((int)num);
		}
	}

	public override void ClearList()
	{
		float[] param = monsterSkillRecord.param;
		foreach (float num in param)
		{
			monster.immBuffList.Remove((int)num);
		}
	}
}
