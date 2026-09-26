using UnityEngine;

public class zbMonsterSkillIndex10211 : zbMonsterSkillBase
{
	public int monsterUUID;

	private bool stopAnimationFlag;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10211.monsterIndex10211 = true;
		monster.monsterIndex10211.durationTimeKeep = 0f;
		monster.monsterIndex10211.skillAttackType = (int)monsterSkillRecord.param[0];
		monster.monsterIndex10211.attrMonsterType = (int)monsterSkillRecord.param[1];
		monster.monsterIndex10211.attrPercent = monsterSkillRecord.param[2];
		monster.monsterIndex10211.durationTime = monsterSkillRecord.param[3];
		monster.monsterIndex10211.skillBase = this;
		monsterUUID = monster.GetUUID();
	}

	public override void Update()
	{
		if (monster.monsterIndex10211.monsterIndex10211)
		{
			monster.monsterIndex10211.durationTimeKeep -= Time.deltaTime;
			if (monster.monsterIndex10211.durationTimeKeep > 0f)
			{
				stopAnimationFlag = true;
			}
			if (monster.monsterIndex10211.durationTimeKeep < 0f && stopAnimationFlag)
			{
				stopAnimationFlag = false;
				monster.anim.spineAnimation.StopAnimation(1);
			}
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10211.monsterIndex10211 = false;
	}
}
