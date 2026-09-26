using UnityEngine;

public class zbMonsterSkillIndex10121 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10121 data = new stMonsterSkillIndex10121();

	private float timeLeft = 0.1f;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.attrMonsterType = (int)monsterSkillRecord.param[0];
		data.attrPercent = monsterSkillRecord.param[1];
		data.durationTime = monsterSkillRecord.param[2];
		monster.monsterIndex10121.Add(data);
		timeLeft = 0.1f;
		data.durationTimeKeep = data.durationTime;
		PlayTackAnimation(1, flag: true);
	}

	public override void Update()
	{
		data.durationTimeKeep -= Time.deltaTime;
		if (data.durationTimeKeep <= 0f)
		{
			data.durationTimeKeep = 999999f;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.monsterIndex10121.Clear();
		}
		timeLeft -= Time.deltaTime;
		if (timeLeft <= 0f)
		{
			timeLeft = 999999f;
			PlayTackAnimation(1, flag: true);
		}
	}

	public override void ClearList()
	{
		base.ClearList();
		data = null;
		monster.monsterIndex10121.Clear();
	}
}
