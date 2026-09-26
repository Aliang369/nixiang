using System.Linq;

public class zbMonsterSkillIndex10001 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		loopStart = 1;
		loopNumber = 1;
		base.RefreshProperty();
		monster.monsterIndex10001.immTime = monsterSkillRecord.param[0];
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 < monsterSkillRecord.param.Count())
			{
				monster.monsterIndex10001.buffIdList.Add((int)monsterSkillRecord.param[num2]);
				num++;
				continue;
			}
			break;
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10001.buffIdList.Clear();
		monster.monsterIndex10001_2.Clear();
	}
}
