using System.Collections;
using UnityEngine;

public class zbMonsterSkillIndex20161 : zbMonsterSkillBase
{
	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex20161.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex20161.distance = monsterSkillRecord.param[1] / 100f;
	}

	public override void Update()
	{
		if (monster.fsm.GetStateId() == EMonsterState.Move && !IsBuffStop() && Time.frameCount != monster.monsterIndex20161.frameCount)
		{
			monster.monsterIndex20161.everyTimeKeep += Time.deltaTime;
			if (monster.monsterIndex20161.everyTimeKeep >= monster.monsterIndex20161.everyTime)
			{
				monster.monsterIndex20161.everyTimeKeep = 0f;
				((MonoBehaviour)monster).StartCoroutine(ActionCoroutine());
			}
			monster.monsterIndex20161.frameCount = Time.frameCount;
		}
	}

	public IEnumerator ActionCoroutine()
	{
		PlayTackAnimation(1, flag: false);
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		zbExtraDirection2.direction = Vector2.op_Implicit(Vector2.down);
		zbExtraDirection2.duration = 1f;
		zbExtraDirection2.speed = monster.monsterIndex20161.distance / 1f;
		monster.AddExtraDirectionList(zbExtraDirection2);
		yield return (object)new WaitForSeconds(1f);
		monster.anim.spineAnimation.skeletonAnimation.AnimationState.SetEmptyAnimation(1, 0f);
	}

	public override void ClearList()
	{
	}
}
