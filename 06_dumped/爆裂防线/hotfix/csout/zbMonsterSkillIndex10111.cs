using UnityEngine;

public class zbMonsterSkillIndex10111 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10111 data = new stMonsterSkillIndex10111();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.distance = monsterSkillRecord.param[0] / 100f;
		data.attrMonsterType = (int)monsterSkillRecord.param[1];
		data.attrPercent = monsterSkillRecord.param[2];
		data.durationTime = monsterSkillRecord.param[3];
		monster.monsterIndex10111.Add(data);
		monster.anim.spineAnimation.StopAnimation(1);
	}

	public override float GetDiameter()
	{
		return data.distance * 2f;
	}

	public override void Update()
	{
		if (!data.triggerFlag && combatData.monsters.IsInAlertRange(monster.GetCurPositionXY().y, data.distance))
		{
			data.triggerFlag = true;
			PlayTackAnimation(1, flag: true);
		}
		if (data.triggerFlag)
		{
			data.durationTime -= Time.deltaTime;
			if (data.durationTime <= 0f)
			{
				data.durationTime = 0f;
			}
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10111.Clear();
	}
}
