using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30112 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int num = 0;
		monster.monsterSkillIndex30112.alertDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30112.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30112.mass = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.giveMonsterSpeed = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30112.giveMonsterTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.skill2TimeCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.moveRange = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30112.moveSpeedPer = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.moveTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.monsterTid2Num = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.monsterTid2 = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.shieldNum = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.jianshangPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.dimianPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30112.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monster30112Class = this;
		monster.monsterSkillIndex30112.everyTimeKeep = 0f;
		monster.monsterSkillIndex30112.skill2TimeKeep = 0f;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30112()
	{
		monster.breakTotal = monster.monsterSkillIndex30112.breakHitCount;
		monster.monsterSkillIndexBianShen = true;
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(2, "skin1toskin2", loop: false);
		monster.anim.spineAnimation.skeletonAnimation.AnimationState.AddAnimation(2, "skin2", true, 0f);
		monster.AddShieldHp((long)(monster.monsterSkillIndex30112.shieldNum * (float)(long)monster.maxHp));
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		monster.monsterSkillIndexBianShen = false;
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void Update()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuffStop() || monster.monsterSkillIndex3011 != 2 || actioning)
		{
			return;
		}
		if ((long)monster.shieldHp <= 0)
		{
			monster.monsterSkillIndex3011 = 3;
			monster.monsterSkillIndex3011LastHp = 100f * (float)(long)monster.curHp / (float)(long)monster.maxHp;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30113Class.Trigger30113());
			return;
		}
		if (!actioning && ((Component)monster).transform.position.y - monster.combatData.defense.GetAlertPositionY() >= monster.monsterSkillIndex30112.alertDistance)
		{
			monster.monsterSkillIndex30112.everyTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30112.everyTimeKeep >= monster.monsterSkillIndex30112.everyTime)
			{
				monster.monsterSkillIndex30112.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
		}
		if (!actioning)
		{
			monster.monsterSkillIndex30112.skill2TimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30112.skill2TimeKeep >= monster.monsterSkillIndex30112.skill2TimeCd)
			{
				monster.monsterSkillIndex30112.skill2TimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill2Action());
			}
		}
	}

	public IEnumerator PlaySkill1Action()
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30112.rangeDistance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 150f && monster.Value.GetMass() <= (float)base.monster.monsterSkillIndex30112.mass)
			{
				list.Add(monster.Value.GetUUID());
				if (list.Count >= base.monster.monsterSkillIndex30112.monsterNum)
				{
					break;
				}
			}
		}
		if (list.Count <= 0)
		{
			base.monster.monsterSkillIndex3011Immune = true;
			base.monster.monsterSkillIndex3011Penetration = true;
		}
		actioning = true;
		TrackEntry shootTrack = base.monster.anim.spineAnimation.PlayTackAnimation(1, "skill1", loop: false);
		yield return (object)new WaitForSeconds(2f / 3f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		if (list.Count <= 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(base.monster.monsterSkillIndex30112.monsterTid, ((Component)base.monster).transform.position, ((Component)base.monster).transform.parent);
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
					zbExtraDirection2.speed = base.monster.monsterSkillIndex30112.giveMonsterSpeed;
					zbExtraDirection2.duration = base.monster.monsterSkillIndex30112.giveMonsterTime;
					monsterById.AddExtraDirectionList(zbExtraDirection2);
				}
			}
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		base.monster.monsterSkillIndex3011Immune = false;
		base.monster.monsterSkillIndex3011Penetration = false;
		actioning = false;
		base.monster.anim.spineAnimation.StopAnimation(1);
		base.monster.canMoveOrAttack = true;
	}

	public IEnumerator PlaySkill2Action()
	{
		zbMonsterEffect effect = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(301112);
		effect.InitData(combatData, monster.GetUUID(), 301112, monster.monsterSkillIndex30112.moveRange / 2.5f);
		if ((Object)(object)effect != (Object)null)
		{
			effect.PlayEffectAnim("animation", loop: false);
		}
		actioning = true;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill2", loop: false);
		yield return (object)new WaitForSeconds(2f / 3f);
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			yield break;
		}
		((MonoBehaviour)monster).StartCoroutine(SpawnMonsterAtPosition());
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (monster.Value.GetUUID() != base.monster.GetUUID() && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30112.moveRange)
			{
				zbExtraProperty zbExtraProperty2 = new zbExtraProperty();
				zbExtraProperty2.attrMonsterType = 5;
				zbExtraProperty2.attrPercent = base.monster.monsterSkillIndex30112.moveSpeedPer;
				zbExtraProperty2.durationTime = base.monster.monsterSkillIndex30112.moveTime;
				monster.Value.extraPropertiesList.Add(zbExtraProperty2);
			}
		}
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 2f / 3f);
		actioning = false;
		base.monster.anim.spineAnimation.StopAnimation(1);
		base.monster.canMoveOrAttack = true;
		if ((Object)(object)effect != (Object)null)
		{
			effect.Free();
		}
	}

	public IEnumerator SpawnMonsterAtPosition()
	{
		int callCount = 0;
		for (int i = 0; i < monster.monsterSkillIndex30112.monsterTid2Num; i++)
		{
			callCount = ((callCount >= 8) ? 7 : callCount);
			((MonoBehaviour)monster).StartCoroutine(SpawnMonsterMonster(zbMonsterSkillBase.vector3s[callCount] * 2f + ((Component)monster).transform.position));
			int num = callCount + 1;
			callCount = num;
			yield return (object)new WaitForSeconds(1f / 3f);
		}
	}

	public IEnumerator SpawnMonsterMonster(Vector3 vector3)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		combatData.monsterOnceEffectPoolMgr.GetMonsterSkillEffect(301111).InitData(combatData, 301111, vector3, "in");
		yield return (object)new WaitForSeconds(1f);
		combatData.monsters.SpawnMonsterAtPosition(monster.monsterSkillIndex30112.monsterTid2, vector3, ((Component)monster).transform.parent);
	}

	public override void ClearList()
	{
	}
}
