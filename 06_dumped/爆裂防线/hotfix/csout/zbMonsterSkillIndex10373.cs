using UnityEngine;

public class zbMonsterSkillIndex10373 : zbMonsterSkillBase
{
	private bool stopAnimationFlag;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10373.monsterIndex10373 = true;
		monster.monsterIndex10373.monsterIndex10373Keep = monsterSkillRecord.param[0];
		monster.monsterIndex10373.monsterIndex10373Distance = monsterSkillRecord.param[1] / 100f;
		monster.monsterIndex10373.monsterIndex10373Speed = monster.monsterIndex10373.monsterIndex10373Distance / monster.monsterIndex10373.monsterIndex10373Keep;
		monster.monsterIndex10373.skillBase = this;
	}

	public override void Update()
	{
		if (!monster.monsterIndex10373.monsterIndex10373)
		{
			monster.monsterIndex10373.monsterIndex10373Keep -= Time.deltaTime;
		}
		if (monster.monsterIndex10373.monsterIndex10373Keep > 0f)
		{
			stopAnimationFlag = true;
		}
		if (monster.monsterIndex10373.monsterIndex10373Keep < 0f && stopAnimationFlag)
		{
			stopAnimationFlag = false;
			monster.anim.spineAnimation.StopAnimation(1);
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10373.monsterIndex10373 = false;
		monster.monsterIndex10373.skillBase = null;
	}
}
