using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30143 : zbMonsterSkillBase
{
	private int monsterUUID;

	private bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30143.delCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.skill1Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30143.addSpeedPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.addSpeedTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.skill2Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.dazeTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.hitDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.dazeCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.skill3Time = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.monsterLionTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30143.dashDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monster30143Class = this;
		monster.monsterSkillIndex30143.skill1TimeKeep = 0f;
		monster.monsterSkillIndex30143.skill2TimeKeep = 0f;
		monster.monsterSkillIndex30143.skill3TimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30143()
	{
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin2toskin3", loop: false);
		monster.monsterSkillIndexBianShen = true;
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin3", loop: true);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public void TriggerDaze()
	{
		if (!(monster.monster3014DazeKeep < monster.monsterSkillIndex30143.dazeCd))
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
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30143.dazeTime);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3014 == 3 && !IsBuffStop() && !actioning)
		{
			monster.monsterSkillIndex30143.skill1TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30143.skill1TimeKeep >= monster.monsterSkillIndex30143.skill1Time)
			{
				monster.monsterSkillIndex30143.skill1TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
			monster.monsterSkillIndex30143.skill2TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30143.skill2TimeKeep >= monster.monsterSkillIndex30143.skill2Time)
			{
				monster.monsterSkillIndex30143.skill2TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
			monster.monsterSkillIndex30143.skill3TimeKeep += Time.deltaTime;
			if (!actioning && monster.monsterSkillIndex30143.skill3TimeKeep >= monster.monsterSkillIndex30143.skill3Time)
			{
				monster.monsterSkillIndex30143.skill3TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill3Action());
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
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, monsterRangeAllList, monster.monsterSkillIndex30143.rangeDistance, monsterUUID);
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			stMonsterAddSpeed item = new stMonsterAddSpeed
			{
				attrMonsterType = 5,
				attrPercent = monster.monsterSkillIndex30143.addSpeedPre,
				durationTime = monster.monsterSkillIndex30143.addSpeedTime,
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
		yield return (object)new WaitForSeconds(1.6666666f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			Vector3 spawnPosition = Vector3.right * 2f + Vector3.up * 1f + ((Component)monster).transform.position;
			combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30143.monsterTid, spawnPosition, ((Component)monster).transform.parent);
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1.6666666f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	private IEnumerator PlaySkill3Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill3", loop: false);
		yield return (object)new WaitForSeconds(1.1666666f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		zbExtraDirection2.direction = Vector3.down;
		zbExtraDirection2.duration = 0.1f;
		zbExtraDirection2.speed = monster.monsterSkillIndex30143.dashDistance;
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (monster.Value.monsterRecord.id == base.monster.monsterSkillIndex30143.monsterLionTid)
			{
				monster.Value.AddExtraDirectionList(zbExtraDirection2);
			}
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1.1666666f);
		actioning = false;
		base.monster.anim.spineAnimation.StopAnimation(1);
		base.monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
