using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30153 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int num = 0;
		monster.monsterSkillIndex30153.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.breakHitAttackType = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill1EveryTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill1MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill2HitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill2AlertDistance1 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30153.skill2AlertDistance2 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30153.skill2AlertDistance3 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30153.skill2MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill2MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill3EveryTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill3MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30153.skill3MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monster30153Class = this;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30153()
	{
		monster.breakTotal = monster.monsterSkillIndex30153.breakHitCount;
		monster.monsterSkillIndexBianShen = true;
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin2toskin3", loop: false);
		monster.anim.spineAnimation.skeletonAnimation.AnimationState.AddAnimation(2, "skin3", true, 0f);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void Update()
	{
		if (!IsBuffStop() && monster.monsterSkillIndex3015 == 3 && !actioning)
		{
			monster.monsterSkillIndex30153.skill1TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30153.skill1TimeKeep >= monster.monsterSkillIndex30153.skill1EveryTime)
			{
				monster.monsterSkillIndex30153.skill1TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
			if (!actioning && monster.monsterSkillIndex3015HitCount >= monster.monsterSkillIndex30153.skill2HitCount)
			{
				monster.monsterSkillIndex3015HitCount = 0;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
			monster.monsterSkillIndex30153.skill3TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30153.skill3TimeKeep >= monster.monsterSkillIndex30153.skill3EveryTime)
			{
				monster.monsterSkillIndex30153.skill3TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
			}
		}
	}

	private IEnumerator PlaySkill1Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill4_1", loop: false);
		yield return (object)new WaitForSeconds(1f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30153.skill1MonsterTid, new Vector3(1.5f, -2f, 0f) + monster.GetCurPosition(), ((Component)monster).transform.parent);
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public IEnumerator PlaySkill2Action()
	{
		actioning = true;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(0.5f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		float unityX = 6 / (monster.monsterSkillIndex30153.skill2MonsterNum - 1);
		for (int j = 0; j < 3; j++)
		{
			yield return (object)new WaitForSeconds(0.5f);
			float num = j switch
			{
				0 => monster.monsterSkillIndex30153.skill2AlertDistance1 + monster.combatData.defense.GetAlertPositionY(), 
				1 => monster.monsterSkillIndex30153.skill2AlertDistance2 + monster.combatData.defense.GetAlertPositionY(), 
				_ => monster.monsterSkillIndex30153.skill2AlertDistance3 + monster.combatData.defense.GetAlertPositionY(), 
			};
			for (int i = 0; i < monster.monsterSkillIndex30153.skill2MonsterNum; i++)
			{
				float num2 = unityX * (float)i - 3f;
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30153.skill2MonsterTid, Vector2.op_Implicit(new Vector2(num2, num)), ((Component)monster).transform.parent);
				if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
				{
					yield break;
				}
			}
		}
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	private IEnumerator PlaySkill3Action()
	{
		actioning = true;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill5", loop: false);
		yield return (object)new WaitForSeconds(1f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		float t = 2.6666667f / (float)monster.monsterSkillIndex30153.skill3MonsterNum;
		for (int i = 0; i < monster.monsterSkillIndex30153.skill3MonsterNum; i++)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30153.skill3MonsterTid, monster.GetCurPosition(), ((Component)monster).transform.parent);
			yield return (object)new WaitForSeconds(t);
			if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
			{
				yield break;
			}
		}
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
