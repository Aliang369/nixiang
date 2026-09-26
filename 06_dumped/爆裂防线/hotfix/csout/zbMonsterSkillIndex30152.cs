using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30152 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public ClawGrowthState leftClaw = new ClawGrowthState
	{
		clawIndex = 1
	};

	public ClawGrowthState rightClaw = new ClawGrowthState
	{
		clawIndex = 2
	};

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int num = 0;
		monster.monsterSkillIndex30152.hpPre30153 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.breakHitAttackType = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill1MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill1DamPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill2HitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill2AlertDistance1 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30152.skill2AlertDistance2 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30152.skill2AlertDistance3 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30152.skill2MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill2MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill3EveryTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill3MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30152.skill3MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monster30152Class = this;
		monster.monsterSkillIndex30152.skill3TimeKeep = 0f;
		monster.monsterSkillIndex30152.skill1LeftKeep = 0f;
		monster.monsterSkillIndex30152.skill1RightKeep = 0f;
		monsterUUID = monster.GetUUID();
		leftClaw.monster = monster;
		rightClaw.monster = monster;
	}

	public IEnumerator Trigger30152()
	{
		monster.breakTotal = monster.monsterSkillIndex30152.breakHitCount;
		monster.monsterSkillIndexBianShen = true;
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin1toskin2", loop: false);
		monster.anim.spineAnimation.skeletonAnimation.AnimationState.AddAnimation(2, "skin2", true, 0f);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		leftClaw.StartDelayedAttach(0f, 0f);
		rightClaw.StartDelayedAttach(0f, 15f);
		monster.monsterSkillIndexBianShen = false;
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void CallBackLostHp(long value)
	{
		if (actioning)
		{
			return;
		}
		monster.monsterSkillIndex30152.skill1HitDam -= value;
		if (IsBuffStop())
		{
			return;
		}
		long num = (long)(monster.monsterSkillIndex30152.skill1DamPre * (float)(long)monster.maxHp);
		if (monster.monsterSkillIndex30152.skill1HitDam >= num && (leftClaw.attached || rightClaw.attached))
		{
			ClawGrowthState claw;
			if (leftClaw.attached && rightClaw.attached)
			{
				float existenceTime = leftClaw.GetExistenceTime(monster.monsterSkillIndex30152.skill1LeftKeep);
				float existenceTime2 = rightClaw.GetExistenceTime(monster.monsterSkillIndex30152.skill1RightKeep);
				claw = ((existenceTime >= existenceTime2) ? leftClaw : rightClaw);
			}
			else
			{
				claw = (leftClaw.attached ? leftClaw : rightClaw);
			}
			((MonoBehaviour)monster).StartCoroutine(PlaySkill1ActionFall(claw));
		}
	}

	public override void Update()
	{
		if (IsBuffStop() || monster.monsterSkillIndex3015 != 2 || actioning)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30152.hpPre30153)
		{
			monster.monsterSkillIndex3015 = 3;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30153Class.Trigger30153());
			return;
		}
		monster.monsterSkillIndex30152.skill1LeftKeep += Time.deltaTime * monster.m_lastTimeScale;
		monster.monsterSkillIndex30152.skill1RightKeep += Time.deltaTime * monster.m_lastTimeScale;
		leftClaw.UpdateDelay(monster.monsterSkillIndex30152.skill1LeftKeep);
		rightClaw.UpdateDelay(monster.monsterSkillIndex30152.skill1RightKeep);
		if (leftClaw.GetExistenceTime(monster.monsterSkillIndex30152.skill1LeftKeep) >= leftClaw.cycleDuration)
		{
			((MonoBehaviour)monster).StartCoroutine(PlaySkill1ActionShoot(leftClaw));
		}
		if (rightClaw.GetExistenceTime(monster.monsterSkillIndex30152.skill1RightKeep) >= rightClaw.cycleDuration)
		{
			((MonoBehaviour)monster).StartCoroutine(PlaySkill1ActionShoot(rightClaw));
		}
		if (!actioning && monster.monsterSkillIndex3015HitCount >= monster.monsterSkillIndex30152.skill2HitCount)
		{
			monster.monsterSkillIndex3015HitCount = 0;
			monster.canMoveOrAttack = false;
			((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
		}
		monster.monsterSkillIndex30152.skill3TimeKeep += Time.deltaTime;
		if (!actioning && monster.monsterSkillIndex30152.skill3TimeKeep >= monster.monsterSkillIndex30152.skill3EveryTime)
		{
			monster.monsterSkillIndex30152.skill3TimeKeep = 0f;
			monster.canMoveOrAttack = false;
			((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
		}
	}

	public IEnumerator PlaySkill1ActionFall(ClawGrowthState claw)
	{
		monster.monsterSkillIndex30152.skill1HitDam = 0L;
		actioning = true;
		monster.anim.spineAnimation.StopAnimation(5 + claw.clawIndex);
		TrackEntry val = ((claw.clawIndex != 1) ? monster.anim.spineAnimation.PlayTackAnimation(7 + claw.clawIndex, "pincer_2fall", loop: false) : monster.anim.spineAnimation.PlayTackAnimation(7 + claw.clawIndex, "pincer_1fall", loop: false));
		claw.StartDelayedAttach(0f, val.AnimationEnd);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(7 + claw.clawIndex);
			monster.canMoveOrAttack = true;
		}
	}

	public IEnumerator PlaySkill1ActionShoot(ClawGrowthState claw)
	{
		monster.monsterSkillIndex30152.skill1HitDam = 0L;
		actioning = true;
		monster.anim.spineAnimation.StopAnimation(5 + claw.clawIndex);
		TrackEntry shootTrack = ((claw.clawIndex != 1) ? monster.anim.spineAnimation.PlayTackAnimation(7 + claw.clawIndex, "pincer_2shoot", loop: false) : monster.anim.spineAnimation.PlayTackAnimation(7 + claw.clawIndex, "pincer_1shoot", loop: false));
		claw.StartDelayedAttach(0f, shootTrack.AnimationEnd);
		yield return (object)new WaitForSeconds(29f / 30f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			if (claw.clawIndex == 1)
			{
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30152.skill1MonsterTid, new Vector3(1f, -2f, 0f) + monster.GetCurPosition(), ((Component)monster).transform.parent);
			}
			else
			{
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30152.skill1MonsterTid, new Vector3(-1f, -2f, 0f) + monster.GetCurPosition(), ((Component)monster).transform.parent);
			}
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(7 + claw.clawIndex);
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
		float unityX = 6 / (monster.monsterSkillIndex30152.skill2MonsterNum - 1);
		for (int j = 0; j < 3; j++)
		{
			yield return (object)new WaitForSeconds(0.5f);
			float num = j switch
			{
				0 => monster.monsterSkillIndex30152.skill2AlertDistance1 + monster.combatData.defense.GetAlertPositionY(), 
				1 => monster.monsterSkillIndex30152.skill2AlertDistance2 + monster.combatData.defense.GetAlertPositionY(), 
				_ => monster.monsterSkillIndex30152.skill2AlertDistance3 + monster.combatData.defense.GetAlertPositionY(), 
			};
			for (int i = 0; i < monster.monsterSkillIndex30152.skill2MonsterNum; i++)
			{
				float num2 = unityX * (float)i - 3f;
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30152.skill2MonsterTid, Vector2.op_Implicit(new Vector2(num2, num)), ((Component)monster).transform.parent);
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
		float t = 2.6666667f / (float)monster.monsterSkillIndex30152.skill3MonsterNum;
		for (int i = 0; i < monster.monsterSkillIndex30152.skill3MonsterNum; i++)
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30152.skill3MonsterTid, monster.GetCurPosition(), ((Component)monster).transform.parent);
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
