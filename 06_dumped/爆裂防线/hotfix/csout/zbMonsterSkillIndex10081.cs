using System.Linq;

public class zbMonsterSkillIndex10081 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		loopNumber = 2;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 < monsterSkillRecord.param.Count())
			{
				stMonsterSkillIndex10081 stMonsterSkillIndex40463 = new stMonsterSkillIndex10081();
				stMonsterSkillIndex40463.scrBuffId = (int)monsterSkillRecord.param[num2];
				stMonsterSkillIndex40463.extraTime = monsterSkillRecord.param[num2 + 1];
				monster.monsterIndex10081.Add(stMonsterSkillIndex40463);
				num++;
				continue;
			}
			break;
		}
	}

	public override void Update()
	{
	}

	public override void ClearList()
	{
		monster.monsterIndex10081.Clear();
	}
}
