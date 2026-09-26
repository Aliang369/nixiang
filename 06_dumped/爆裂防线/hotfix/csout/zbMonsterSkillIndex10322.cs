using System.Linq;
using UnityEngine;

public class zbMonsterSkillIndex10322 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		loopStart = 1;
		loopNumber = 1;
		monster.monsterIndex10322 = new stMonsterSkillIndex10322();
		monster.monsterIndex10322.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex10322.everyTimeKeep = 0f;
		int num = 0;
		while (true)
		{
			int num2 = num * loopNumber + loopStart;
			if (num2 >= monsterSkillRecord.param.Count())
			{
				break;
			}
			monster.monsterIndex10322.immeTagList.Add((int)monsterSkillRecord.param[num2]);
			num++;
		}
		if (monster.isRestoreData)
		{
			ClearList();
		}
		else
		{
			PlayTackAnimation(1, flag: true);
		}
	}

	public override void Update()
	{
		monster.monsterIndex10322.everyTimeKeep += Time.deltaTime;
		if (monster.monsterIndex10322.everyTimeKeep > monster.monsterIndex10322.everyTime)
		{
			ClearList();
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10322.immeTagList.Clear();
		monster.anim.spineAnimation.StopAnimation(1);
	}
}
