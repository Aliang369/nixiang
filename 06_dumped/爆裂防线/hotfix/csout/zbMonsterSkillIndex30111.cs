using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30111 : zbMonsterSkillBase
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
		monster.monsterSkillIndex3011Immune = false;
		monster.monsterSkillIndex3011Penetration = false;
		int num = 0;
		monster.monsterSkillIndex30111.alertDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30111.everyTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.rangeDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30111.mass = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.monsterNum = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.giveMonsterSpeed = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30111.giveMonsterTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.monsterTid = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.skill2TimeCd = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.moveRange = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30111.moveSpeedPer = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.moveTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.hpPre30112 = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.breakHitAttackType = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30111.skillBornDistance = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.breakTotal = monster.monsterSkillIndex30111.breakHitCount;
		monster.monsterSkillIndex3011 = 1;
		monster.monsterSkillIndex3011LastHp = 100f * (float)(long)monster.curHp / (float)(long)monster.maxHp;
		monster.monsterSkillIndex30111.immBuffs.Clear();
		monster.monsterSkillIndex30111.immBuffs.Add(2);
		monster.monsterSkillIndex30111.immBuffs.Add(3);
		monster.monsterSkillIndex30111.immBuffs.Add(4);
		monster.monsterSkillIndexClass = this;
		monster.monster30111Class = this;
		monster.monsterSkillIndex30111.everyTimeKeep = 0f;
		monster.monsterSkillIndex30111.skill2TimeKeep = 0f;
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
			((Vector3)(ref position))._002Ector(((Component)monster).gameObject.transform.position.x, monster.monsterSkillIndex30111.skillBornDistance + combatData.defense.GetAlertPositionY(), ((Component)monster).gameObject.transform.position.z);
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
		monster.monsterSkillIndex3011 = 0;
		TrackEntry val = monster.anim.spineAnimation.PlayTackAnimation(1, "skill6", loop: false);
		yield return (object)new WaitForSeconds(val.AnimationEnd);
		callback?.Invoke();
	}

	public void TriggerBreakSkill(int _skillAttackType)
	{
		if (!monster.monsterSkillIndex3011BreakFlag && _skillAttackType == monster.monsterSkillIndex30111.breakHitAttackType)
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
		monster.monsterSkillIndex3011BreakFlag = true;
		if (monster.monsterSkillIndex3011 == 1)
		{
			monster.monsterSkillIndex3011BreakHitDam = monster.monsterSkillIndex30111.breakHitDam;
		}
		else if (monster.monsterSkillIndex3011 == 2)
		{
			monster.monsterSkillIndex3011BreakHitDam = monster.monsterSkillIndex30112.breakHitDam;
		}
		else if (monster.monsterSkillIndex3011 == 3)
		{
			monster.monsterSkillIndex3011BreakHitDam = monster.monsterSkillIndex30113.breakHitDam;
		}
		yield return (object)new WaitForSeconds(monster.monsterSkillIndex30111.breakHunmiTm);
		if (monster.monsterSkillIndex3011 == 3)
		{
			zbExtraProperty zbExtraProperty2 = new zbExtraProperty();
			zbExtraProperty2.attrMonsterType = 5;
			zbExtraProperty2.attrPercent = monster.monsterSkillIndex30113.breakSpeed;
			zbExtraProperty2.durationTime = monster.monsterSkillIndex30113.breakSpeedTm;
			monster.extraPropertiesList.Add(zbExtraProperty2);
			zbExtraProperty2 = new zbExtraProperty();
			zbExtraProperty2.attrMonsterType = 3;
			zbExtraProperty2.attrPercent = monster.monsterSkillIndex30113.breakAttackSpeed;
			zbExtraProperty2.durationTime = monster.monsterSkillIndex30113.breakSpeedTm;
			monster.extraPropertiesList.Add(zbExtraProperty2);
		}
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
		actioning = false;
		monster.monsterSkillIndex3011BreakFlag = false;
	}

	public override void Save(stMonsterBoss monsterBoss)
	{
		monsterBoss.monsterSkillIndex301x = monster.monsterSkillIndex3011;
		monsterBoss.monsterSkillIndex301xLastHp = monster.monsterSkillIndex3011LastHp;
	}

	public override void RestoreData(stMonsterBoss monsterBoss)
	{
		monster.monsterSkillIndex3011 = monsterBoss.monsterSkillIndex301x;
		monster.monsterSkillIndex3011LastHp = monsterBoss.monsterSkillIndex301xLastHp;
		if (monster.monsterSkillIndex3011 >= 0 && monster.monsterSkillIndex3011 <= 3)
		{
			monster.anim.spineAnimation.PlayTackAnimation(2, "skin" + monster.monsterSkillIndex3011, loop: true);
		}
		if (monster.monsterSkillIndex3011 == 1)
		{
			monster.breakTotal = monster.monsterSkillIndex30111.breakHitCount;
		}
		else if (monster.monsterSkillIndex3011 == 2)
		{
			monster.breakTotal = monster.monsterSkillIndex30112.breakHitCount;
		}
		else if (monster.monsterSkillIndex3011 == 3)
		{
			monster.breakTotal = monster.monsterSkillIndex30113.breakHitCount;
		}
	}

	public override void Update()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (IsBuffStop() || monster.monsterSkillIndex3011 != 1 || actioning)
		{
			return;
		}
		if ((float)(long)monster.curHp / (float)(long)monster.maxHp < monster.monsterSkillIndex30111.hpPre30112)
		{
			monster.monsterSkillIndex3011 = 2;
			monster.monsterSkillIndex3011LastHp = 100f * (float)(long)monster.curHp / (float)(long)monster.maxHp;
			((MonoBehaviour)monster).StartCoroutine(monster.monster30112Class.Trigger30112());
			return;
		}
		if (!actioning && ((Component)monster).transform.position.y - monster.combatData.defense.GetAlertPositionY() >= monster.monsterSkillIndex30111.alertDistance)
		{
			monster.monsterSkillIndex30111.everyTimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30111.everyTimeKeep >= monster.monsterSkillIndex30111.everyTime)
			{
				monster.monsterSkillIndex30111.everyTimeKeep = 0f;
				monster.canMoveOrAttack = false;
				((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
			}
		}
		if (!actioning)
		{
			monster.monsterSkillIndex30111.skill2TimeKeep += Time.deltaTime;
			if (monster.monsterSkillIndex30111.skill2TimeKeep >= monster.monsterSkillIndex30111.skill2TimeCd)
			{
				monster.monsterSkillIndex30111.skill2TimeKeep = 0f;
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
			if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30111.rangeDistance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 150f && monster.Value.GetMass() <= (float)base.monster.monsterSkillIndex30111.mass)
			{
				list.Add(monster.Value.GetUUID());
				if (list.Count >= base.monster.monsterSkillIndex30111.monsterNum)
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
			combatData.monsters.SpawnMonsterAtPosition(base.monster.monsterSkillIndex30111.monsterTid, ((Component)base.monster).transform.position, ((Component)base.monster).transform.parent);
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
					zbExtraDirection2.speed = base.monster.monsterSkillIndex30111.giveMonsterSpeed;
					zbExtraDirection2.duration = base.monster.monsterSkillIndex30111.giveMonsterTime;
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
		effect.InitData(combatData, monster.GetUUID(), 301112, monster.monsterSkillIndex30111.moveRange / 2.5f);
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
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (monster.Value.GetUUID() != base.monster.GetUUID() && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30111.moveRange)
			{
				zbExtraProperty zbExtraProperty2 = new zbExtraProperty();
				zbExtraProperty2.attrMonsterType = 5;
				zbExtraProperty2.attrPercent = base.monster.monsterSkillIndex30111.moveSpeedPer;
				zbExtraProperty2.durationTime = base.monster.monsterSkillIndex30111.moveTime;
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

	public override void ClearList()
	{
	}
}
