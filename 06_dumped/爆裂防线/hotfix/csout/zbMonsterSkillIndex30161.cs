using System;
using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30161 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	private List<zbMonster> m_monstersInRange = new List<zbMonster>();

	public override void RefreshProperty()
	{
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		base.RefreshProperty();
		monster.monsterSkillIndexBianShen = false;
		monster.beBreakBoss = true;
		monster.needRestoreData = true;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30161.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.breakHitAttackType = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.defensePrecent = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skill1EveryTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skill1Distance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30161.skill1MonsterBuffId = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skill2EveryTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skill2Distance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30161.skill2MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skill2MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30161.skillBornDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.breakTotal = monster.monsterSkillIndex30161.breakHitCount;
		monster.monsterSkillIndex3016 = 1;
		monster.monsterSkillIndex3016HitCount = 1;
		monster.monsterSkillIndexClass = this;
		monster.monster30161Class = this;
		monster.monsterSkillIndex30161.skill1EveryTimeKeep = 0f;
		monster.monsterSkillIndex30161.skill2EveryTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		monster.maxShieldHp = (long)((float)(long)monster.maxHp * monster.monsterSkillIndex30161.defensePrecent);
		monster.SetShieldHp(monster.maxShieldHp);
		Vector3 position = default(Vector3);
		((Vector3)(ref position))._002Ector(((Component)monster).gameObject.transform.position.x, monster.monsterSkillIndex30161.skillBornDistance + combatData.defense.GetAlertPositionY(), ((Component)monster).gameObject.transform.position.z);
		((Component)monster).gameObject.transform.position = position;
	}

	public IEnumerator Trigger30162_30161()
	{
		monster.anim.spineAnimation.StopAnimation(1);
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin2toskin1", loop: false);
		monster.maxShieldHp = (long)((float)(long)monster.maxHp * monster.monsterSkillIndex30161.defensePrecent);
		monster.SetShieldHp(monster.maxShieldHp);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		actioning = false;
		monster.canMoveOrAttack = true;
	}

	public override void TriggerDieSkill(Action callback)
	{
		((MonoBehaviour)monster).StopAllCoroutines();
		((MonoBehaviour)monster).StartCoroutine(TriggerDieSkillIEnumerator(callback));
	}

	public IEnumerator TriggerDieSkillIEnumerator(Action callback)
	{
		monster.monsterSkillIndex3016 = 0;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill6_1", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public void TriggerBreakSkill(int _skillAttackType)
	{
		monster.monsterSkillIndex3016HitCount++;
		if (!monster.monsterSkillIndex3016BreakFlag && _skillAttackType == monster.monsterSkillIndex30161.breakHitAttackType)
		{
			zbMonster obj = monster;
			int breakCount = obj.breakCount + 1;
			obj.breakCount = breakCount;
			if (!monster.monsterSkillIndexBianShen && monster.breakCount >= monster.breakTotal && (long)monster.curHp > 0)
			{
				((MonoBehaviour)monster).StartCoroutine(ShowBreakSkill());
				monster.breakCount = 0;
			}
		}
	}

	public IEnumerator ShowBreakSkill()
	{
		actioning = true;
		monster.canMoveOrAttack = false;
		monster.anim.spineAnimation.PlayTackAnimation(1, "skill5", loop: true);
		monster.monsterSkillIndex3016BreakFlag = true;
		if (monster.monsterSkillIndex3016 == 1)
		{
			monster.monsterSkillIndex3016BreakHitDam = monster.monsterSkillIndex30161.breakHitDam;
		}
		else if (monster.monsterSkillIndex3016 == 2)
		{
			monster.monsterSkillIndex3016BreakHitDam = monster.monsterSkillIndex30162.breakHitDam;
		}
		else if (monster.monsterSkillIndex3016 == 3)
		{
			monster.monsterSkillIndex3016BreakHitDam = monster.monsterSkillIndex30163.breakHitDam;
		}
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30161.breakHunmiTm);
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
		actioning = false;
		monster.monsterSkillIndex3016BreakFlag = false;
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3016;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3016 = monsterBoss.monsterSkillIndex301x;
		if (monster.monsterSkillIndex3016 >= 0 && monster.monsterSkillIndex3016 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3016, loop: true);
		}
		if (monster.monsterSkillIndex3016 == 1)
		{
			monster.breakTotal = monster.monsterSkillIndex30161.breakHitCount;
		}
		else if (monster.monsterSkillIndex3016 == 2)
		{
			monster.breakTotal = monster.monsterSkillIndex30162.breakHitCount;
		}
		else if (monster.monsterSkillIndex3016 == 3)
		{
			monster.breakTotal = monster.monsterSkillIndex30163.breakHitCount;
		}
	}

	public override void Update()
	{
		if (IsBuffStop() || monster.monsterSkillIndex3016 != 1 || actioning)
		{
			return;
		}
		if ((long)monster.shieldHp <= 0)
		{
			monster.monsterSkillIndex3016 = 2;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30162Class.Trigger30162());
			return;
		}
		if (!actioning)
		{
			monster.monsterSkillIndex30161.skill1EveryTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30161.skill1EveryTimeKeep >= monster.monsterSkillIndex30161.skill1EveryTime)
			{
				monster.monsterSkillIndex30161.skill1EveryTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
		}
		if (!actioning)
		{
			monster.monsterSkillIndex30161.skill2EveryTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30161.skill2EveryTimeKeep >= monster.monsterSkillIndex30161.skill2EveryTime)
			{
				monster.monsterSkillIndex30161.skill2EveryTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
		}
	}

	public IEnumerator PlaySkill1Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill1", loop: false);
		yield return (object)new WaitForSeconds(5f / 6f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, m_monstersInRange, monster.monsterSkillIndex30161.skill1Distance, monster.GetUUID());
		foreach (zbMonster item in m_monstersInRange)
		{
			item.monsterBlockCount = monster.monsterSkillIndex30161.skill1MonsterBuffId;
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 5f / 6f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public IEnumerator PlaySkill2Action()
	{
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(5f / 6f);
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			for (int i = 0; i < monster.monsterSkillIndex30161.skill2MonsterNum; i++)
			{
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30161.skill2MonsterTid, monster.GetCurPosition() + Vector3.down * monster.monsterSkillIndex30161.skill2Distance, ((Component)monster).transform.parent);
			}
			yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 5f / 6f);
			actioning = false;
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public override void ClearList()
	{
	}
}
