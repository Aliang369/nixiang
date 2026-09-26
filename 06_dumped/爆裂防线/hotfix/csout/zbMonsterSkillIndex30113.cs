using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex30113 : zbMonsterSkillBase
{
	private int monsterUUID;

	public bool actioning;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		int num = 0;
		monster.monsterSkillIndex30113.selfMoveSpeed = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.attackSpeedPre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.hitDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.moveRange = monsterSkillRecord.param[num] / 100f;
		num++;
		monster.monsterSkillIndex30113.moveSpeed = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.moveTime = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.bigDamagePre = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakHitCount = (int)monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakHunmiTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakHitDam = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakSpeed = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakAttackSpeed = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.breakSpeedTm = monsterSkillRecord.param[num];
		num++;
		monster.monsterSkillIndex30113.bigActionNum = 0;
		monster.monster30113Class = this;
		monsterUUID = monster.GetUUID();
	}

	public IEnumerator Trigger30113()
	{
		monster.breakTotal = monster.monsterSkillIndex30113.breakHitCount;
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

	public IEnumerator BigDamage()
	{
		stMonsterSkillIndex30113 monsterSkillIndex = monster.monsterSkillIndex30113;
		int bigActionNum = monsterSkillIndex.bigActionNum + 1;
		monsterSkillIndex.bigActionNum = bigActionNum;
		actioning = true;
		monster.canMoveOrAttack = false;
		TrackEntry shootTrack = monster.anim.spineAnimation.PlayTackAnimation(1, "skill3", loop: false);
		yield return (object)new WaitForSeconds(0.8f);
		zbCombatSystem.Instance.combatFormula.CalcWallDamage(monster, monster.combatData.defense, monster.GetAttrValueByType(EAttrMonsterType.Damage) * monster.monsterSkillIndex30113.bigDamagePre);
		yield return (object)new WaitForSeconds(shootTrack.AnimationEnd - 0.8f);
		actioning = false;
		monster.anim.spineAnimation.StopAnimation(1);
		((MonoBehaviour)monster).StartCoroutine(PlaySkill1Action());
	}

	public override void Update()
	{
		if (!IsBuffStop() && monster.monsterSkillIndex3011 == 3)
		{
			_ = actioning;
		}
	}

	public IEnumerator PlaySkill1Action()
	{
		monster.canMoveOrAttack = false;
		zbMonsterEffect effect = combatData.monsterEffectPoolMgr.GetMonsterSkillEffect(301112);
		effect.InitData(combatData, monster.GetUUID(), 301112, monster.monsterSkillIndex30113.moveRange / 5f);
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
			if (monster.Value.GetUUID() != base.monster.GetUUID() && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterSkillIndex30113.moveRange)
			{
				zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
				zbExtraDirection2.direction = Vector3.down;
				zbExtraDirection2.duration = base.monster.monsterSkillIndex30113.moveTime;
				zbExtraDirection2.speed = base.monster.monsterSkillIndex30113.moveSpeed;
				monster.Value.AddExtraDirectionList(zbExtraDirection2);
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
