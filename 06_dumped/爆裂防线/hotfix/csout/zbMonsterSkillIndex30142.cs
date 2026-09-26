using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30142 : zbMonsterSkillBase
{
	private int monsterUUID;

	private bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin2", loop: true);
		int num = 0;
		monster.monsterSkillIndex30142.stopDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30142.skill1Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30142.addSpeedPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.addSpeedTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.skill2Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.dazeTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.hitDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.dazeCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.hpPre30143 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30142.stopDistance += monster.combatData.defense.GetAlertPositionY();
		monster.monster30142Class = this;
		monster.monsterSkillIndex30142.skill1TimeKeep = 0f;
		monster.monsterSkillIndex30142.skill2TimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30142()
	{
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin1toskin2", loop: false);
		monster.monsterSkillIndexBianShen = true;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin2", loop: true);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public void TriggerDaze()
	{
		if (!(monster.monster3014DazeKeep < monster.monsterSkillIndex30142.dazeCd))
		{
			((MonoBehaviour)monster).StartCoroutine(PlayTriggerDazeAction());
		}
	}

	private IEnumerator PlayTriggerDazeAction()
	{
		monster.monster3014DazeDam = 0f;
		monster.monster3014DazeKeep = 0f;
		actioning = true;
		monster.canMoveOrAttack = false;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill5", loop: true);
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30142.dazeTime);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3014 != 2)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30142.hpPre30143 && !actioning)
		{
			monster.monsterSkillIndex3014 = 3;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30143Class.Trigger30143());
		}
		else if (!IsBuffStop() && !actioning)
		{
			monster.monsterSkillIndex30142.skill1TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30142.skill1TimeKeep >= monster.monsterSkillIndex30142.skill1Time)
			{
				monster.monsterSkillIndex30142.skill1TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
			monster.monsterSkillIndex30142.skill2TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30142.skill2TimeKeep >= monster.monsterSkillIndex30142.skill2Time)
			{
				monster.monsterSkillIndex30142.skill2TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
		}
	}

	private IEnumerator PlaySkill1Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill1", loop: false);
		yield return (object)new WaitForSeconds(2f / 3f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, monsterRangeAllList, monster.monsterSkillIndex30142.rangeDistance, monsterUUID);
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			stMonsterAddSpeed item = new stMonsterAddSpeed
			{
				attrMonsterType = 5,
				attrPercent = monster.monsterSkillIndex30142.addSpeedPre,
				durationTime = monster.monsterSkillIndex30142.addSpeedTime,
				durationTimeKeep = 0f
			};
			monsterRangeAll.monsterAddSpeedList.Add(item);
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	private IEnumerator PlaySkill2Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		Vector3 vector3 = Vector3.right * 1.8f + Vector3.up * 1f + ((Component)monster).transform.position;
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301411).InitData(combatData, 301411, vector3, "animation");
		yield return (object)new WaitForSeconds(1.6666666f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30142.monsterTid, vector3, ((Component)monster).transform.parent);
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1.6666666f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public override void ClearList()
	{
	}
}
