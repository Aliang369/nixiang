using UnityEngine;

public class zbMonsterSkillIndex10021 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10021 data = new stMonsterSkillIndex10021();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data.everyTime = monsterSkillRecord.param[0];
		data.attrMonsterType = (int)monsterSkillRecord.param[1];
		data.attrPercent = monsterSkillRecord.param[2];
		data.durationTime = monsterSkillRecord.param[3];
		data.everyTimeKeep = 0f;
		data.durationTimeKeep = 0f;
		monster.monsterIndex10021.Add(data);
	}

	public override void Update()
	{
		if (!IsBuffStop())
		{
			data.everyTimeKeep += Time.deltaTime;
		}
		if (data.everyTimeKeep >= data.everyTime)
		{
			PlayTackAnimation(1, flag: true);
			data.everyTimeKeep = 0f;
			data.durationTimeKeep = data.durationTime;
		}
		data.durationTimeKeep -= Time.deltaTime;
		if (data.durationTimeKeep <= 0f)
		{
			data.durationTimeKeep = 0f;
			monster.anim.spineAnimation.StopAnimation(1);
		}
	}

	public override void ClearList()
	{
		data = null;
		monster.monsterIndex10021.Clear();
	}
}
