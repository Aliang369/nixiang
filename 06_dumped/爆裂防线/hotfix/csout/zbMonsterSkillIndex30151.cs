using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30151 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterSkillIndexBianShen = false;
		monster.beBreakBoss = true;
		monster.needRestoreData = true;
		monster.anim.spineAnimation.PlayTackAnimation(2, "skin1", loop: true);
		int num = 0;
		monster.monsterSkillIndex30151.hpPre30152 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.breakHitAttackType = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.alertDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.mass = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.giveMonsterSpeed = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.giveMonsterTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.monsterTid1 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.monsterTid2 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.monsterTid3 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.skill2HitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.skill2AlertDistance1 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.skill2AlertDistance2 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.skill2AlertDistance3 = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30151.skill2MonsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.skill2MonsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30151.skillBornDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.breakTotal = monster.monsterSkillIndex30151.breakHitCount;
		monster.monsterSkillIndex3015 = 1;
		monster.monsterSkillIndex3015HitCount = 1;
		monster.monsterSkillIndexClass = this;
		monster.monster30151Class = this;
		monster.monsterSkillIndex30151.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		((MonoBehaviour)monster).StartCoroutine(TriggerEnter());
	}

	public IEnumerator TriggerEnter()
	{
		if (!monster.isRestoreData)
		{
			monster.canMoveOrAttack = false;
			actioning = true;
			TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "enter", loop: false);
			Vector3 position = default(Vector3);
			((Vector3)(ref position))._002Ector(((Component)monster).gameObject.transform.position.x, monster.monsterSkillIndex30151.skillBornDistance + combatData.defense.GetAlertPositionY(), ((Component)monster).gameObject.transform.position.z);
			((Component)monster).gameObject.transform.position = position;
			yield return (object)new WaitForSeconds(val.AnimationEnd);
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
			actioning = false;
		}
	}

	public override void TriggerDieSkill(Action callback)
	{
		((MonoBehaviour)monster).StopAllCoroutines();
		((MonoBehaviour)monster).StartCoroutine(TriggerDieSkillIEnumerator(callback));
	}

	public IEnumerator TriggerDieSkillIEnumerator(Action callback)
	{
		monster.monsterSkillIndex3015 = 0;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill6_1", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public void TriggerBreakSkill(int _skillAttackType)
	{
		monster.monsterSkillIndex3015HitCount++;
		if (!monster.monsterSkillIndex3015BreakFlag && _skillAttackType == monster.monsterSkillIndex30151.breakHitAttackType)
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
		monster.monsterSkillIndex3015BreakFlag = true;
		if (monster.monsterSkillIndex3015 == 1)
		{
			monster.monsterSkillIndex3015BreakHitDam = monster.monsterSkillIndex30151.breakHitDam;
		}
		else if (monster.monsterSkillIndex3015 == 2)
		{
			monster.monsterSkillIndex3015BreakHitDam = monster.monsterSkillIndex30152.breakHitDam;
		}
		else if (monster.monsterSkillIndex3015 == 3)
		{
			monster.monsterSkillIndex3015BreakHitDam = monster.monsterSkillIndex30153.breakHitDam;
		}
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30151.breakHunmiTm);
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
		actioning = false;
		monster.monsterSkillIndex3015BreakFlag = false;
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3015;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3015 = monsterBoss.monsterSkillIndex301x;
		if (monster.monsterSkillIndex3015 >= 0 && monster.monsterSkillIndex3015 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3015, loop: true);
		}
		if (monster.monsterSkillIndex3015 == 1)
		{
			monster.breakTotal = monster.monsterSkillIndex30151.breakHitCount;
		}
		else if (monster.monsterSkillIndex3015 == 2)
		{
			monster.breakTotal = monster.monsterSkillIndex30152.breakHitCount;
		}
		else if (monster.monsterSkillIndex3015 == 3)
		{
			monster.breakTotal = monster.monsterSkillIndex30153.breakHitCount;
		}
	}

	public override void Update()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuffStop() || monster.monsterSkillIndex3015 != 1 || actioning)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30151.hpPre30152)
		{
			monster.monsterSkillIndex3015 = 2;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30152Class.Trigger30152());
			return;
		}
		if (!actioning && ((Component)monster).transform.position.y - monster.combatData.defense.GetAlertPositionY() >= monster.monsterSkillIndex30151.alertDistance)
		{
			monster.monsterSkillIndex30151.everyTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30151.everyTimeKeep >= monster.monsterSkillIndex30151.everyTime)
			{
				monster.monsterSkillIndex30151.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
		}
		if (!actioning && monster.monsterSkillIndex3015HitCount >= monster.monsterSkillIndex30151.skill2HitCount)
		{
			monster.monsterSkillIndex3015HitCount = 0;
			monster.canMoveOrAttack = false;
			((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
		}
	}

	public IEnumerator PlaySkill1Action()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30151.rangeDistance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 150f && monster.Value.GetMass() <= (float)base.monster.monsterSkillIndex30151.mass)
			{
				list.Add(monster.Value.GetUUID());
				if (list.Count >= base.monster.monsterSkillIndex30151.monsterNum)
				{
					break;
				}
			}
		}
		actioning = true;
		TrackEntry shootTrack = ((list.Count > 0) ? base.monster.anim.spineAnimation.PlayTackAnimation(1, "skill1", loop: false) : base.monster.anim.spineAnimation.PlayTackAnimation(1, "skill3", loop: false));
		yield return (object)new WaitForSeconds(1.0333333f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		if (list.Count <= 0)
		{
			int num = Rand.Range(1, 4);
			int monsterTid = ((num == 1) ? base.monster.monsterSkillIndex30151.monsterTid1 : ((num != 1) ? base.monster.monsterSkillIndex30151.monsterTid2 : base.monster.monsterSkillIndex30151.monsterTid2));
			combatData.monsters.SpawnMonsterAtPosition(monsterTid, ((Component)base.monster).transform.position, ((Component)base.monster).transform.parent).AddExtraDirectionList(new zbExtraDirection
			{
				direction = Vector3.down,
				speed = base.monster.monsterSkillIndex30151.giveMonsterSpeed,
				duration = base.monster.monsterSkillIndex30151.giveMonsterTime
			});
		}
		else
		{
			foreach (int item in list)
			{
				zbMonster monsterById = combatData.monsters.GetMonsterById(item);
				if ((Object)(object)monsterById != (Object)null)
				{
					zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
					zbExtraDirection2.direction = Vector3.down;
					zbExtraDirection2.speed = base.monster.monsterSkillIndex30151.giveMonsterSpeed;
					zbExtraDirection2.duration = base.monster.monsterSkillIndex30151.giveMonsterTime;
					monsterById.AddExtraDirectionList(zbExtraDirection2);
				}
			}
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 1.0333333f);
		actioning = false;
		base.monster.anim.spineAnimation.StopAnimation(1);
		base.monster.canMoveOrAttack = true;
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
		float unityX = 6 / (monster.monsterSkillIndex30151.skill2MonsterNum - 1);
		for (int j = 0; j < 3; j++)
		{
			yield return (object)new WaitForSeconds(0.5f);
			float num = j switch
			{
				0 => monster.monsterSkillIndex30151.skill2AlertDistance1 + monster.combatData.defense.GetAlertPositionY(), 
				1 => monster.monsterSkillIndex30151.skill2AlertDistance2 + monster.combatData.defense.GetAlertPositionY(), 
				_ => monster.monsterSkillIndex30151.skill2AlertDistance3 + monster.combatData.defense.GetAlertPositionY(), 
			};
			for (int i = 0; i < monster.monsterSkillIndex30151.skill2MonsterNum; i++)
			{
				float num2 = unityX * (float)i - 3f;
				combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30151.skill2MonsterTid, Vector2.op_Implicit(new Vector2(num2, num)), ((Component)monster).transform.parent);
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

	public override void ClearList()
	{
	}
}
