using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex20163 : zbMonsterSkillBase
{
	private int monsterUUID;

	private int targetMonsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterUUID = monster.GetUUID();
		monster.monsterIndex20163.everyTime = monsterSkillRecord.param[0];
		monster.monsterIndex20163.distance = monsterSkillRecord.param[1] / 100f;
		monster.monsterIndex20163.durationTime = monsterSkillRecord.param[2];
		monster.monsterIndex20163.flyDistance = monsterSkillRecord.param[3] / 100f;
		targetMonsterUUID = 0;
		monster.monsterIndex20163.everyTimeKeep = 0f;
	}

	public override float GetDiameter()
	{
		return monster.monsterIndex20163.distance * 2f;
	}

	public override void Update()
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		if (monster.fsm.GetStateId() != EMonsterState.Move || IsBuffStop())
		{
			return;
		}
		monster.monsterIndex20163.everyTimeKeep += Time.deltaTime;
		if (!(monster.monsterIndex20163.everyTimeKeep >= monster.monsterIndex20163.everyTime))
		{
			return;
		}
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) > 0.5f && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterIndex20163.distance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 150f)
			{
				targetMonsterUUID = monster.Value.GetUUID();
				monster.Value.canMoveOrAttack = false;
				break;
			}
		}
		if (targetMonsterUUID != 0)
		{
			base.monster.monsterIndex20163.everyTimeKeep = 0f;
			base.monster.canMoveOrAttack = false;
			((MonoBehaviour)base.monster).StartCoroutine(ActionCoroutine());
		}
	}

	public IEnumerator ActionCoroutine()
	{
		CreateEffectSpine();
		TrackEntry val = PlayTackAnimation(1, flag: false);
		if (val != null)
		{
			yield return (object)new WaitForSeconds(val.AnimationEnd);
		}
		else
		{
			yield return (object)new WaitForSeconds(1f);
		}
		zbMonster monsterById = combatData.monsters.GetMonsterById(targetMonsterUUID);
		if ((Object)(object)monsterById == (Object)null)
		{
			monster.monsterIndex20163.everyTimeKeep = monster.monsterIndex20163.everyTime;
			targetMonsterUUID = 0;
			FreeEffectSpine();
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
			yield break;
		}
		zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
		zbExtraDirection2.direction = Vector3.down;
		zbExtraDirection2.duration = monster.monsterIndex20163.durationTime;
		zbExtraDirection2.speed = monster.monsterIndex20163.flyDistance / zbExtraDirection2.duration;
		monsterById.AddExtraDirectionList(zbExtraDirection2);
		monsterById.canMoveOrAttack = true;
		targetMonsterUUID = 0;
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}
}
