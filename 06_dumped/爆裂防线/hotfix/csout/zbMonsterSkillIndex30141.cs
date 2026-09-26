using System;
using System.Collections;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30141 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30141.stopDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30141.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30141.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30141.addSpeedPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30141.addSpeedTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30141.hpPre30142 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30141.stopDistance += monster.combatData.defense.GetAlertPositionY();
		monster.monsterSkillIndex3014 = 1;
		monster.monsterSkillIndexClass = this;
		monster.monster30141Class = this;
		monster.monsterSkillIndex30141.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public override void TriggerDieSkill(Action callback)
	{
		((MonoBehaviour)monster).StartCoroutine(TriggerDieSkillIEnumerator(callback));
	}

	public IEnumerator TriggerDieSkillIEnumerator(Action callback)
	{
		monster.monsterSkillIndex3014 = 0;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill4", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3014;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3014 = monsterBoss.monsterSkillIndex301x;
		if (monster.monsterSkillIndex3014 >= 0 && monster.monsterSkillIndex3014 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3014, loop: true);
		}
	}

	public override void Update()
	{
		if (monster.monsterSkillIndex3014 != 1)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30141.hpPre30142 && !actioning)
		{
			monster.monsterSkillIndex3014 = 2;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30142Class.Trigger30142());
		}
		else if (!IsBuffStop() && !actioning)
		{
			monster.monsterSkillIndex30141.everyTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30141.everyTimeKeep >= monster.monsterSkillIndex30141.everyTime)
			{
				monster.monsterSkillIndex30141.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
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
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, monsterRangeAllList, monster.monsterSkillIndex30141.rangeDistance, monsterUUID);
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			stMonsterAddSpeed item = new stMonsterAddSpeed
			{
				attrMonsterType = 5,
				attrPercent = monster.monsterSkillIndex30141.addSpeedPre,
				durationTime = monster.monsterSkillIndex30141.addSpeedTime,
				durationTimeKeep = 0f
			};
			monsterRangeAll.monsterAddSpeedList.Add(item);
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
