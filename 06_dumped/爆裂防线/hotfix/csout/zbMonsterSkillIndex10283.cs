using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10283 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10283 monsterIndex10283 = new stMonsterSkillIndex10283();

	private bool m_bExecReturn1;

	private float m_DurationMax;

	private float m_DurationTime1;

	private bool m_bExecReturn;

	private float m_DurationTime2;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterIndex10283.everyTime = monsterSkillRecord.param[0];
		monsterIndex10283.distance = monsterSkillRecord.param[1] / 100f;
		monsterIndex10283.durationTime = monsterSkillRecord.param[2];
		monsterIndex10283.flyDistance = monsterSkillRecord.param[3] / 100f;
		monsterIndex10283.everyTimeKeep = 0f;
		m_DurationMax = 0f;
		m_DurationTime1 = 0f;
		m_DurationTime2 = 0f;
		m_bExecReturn1 = false;
		m_bExecReturn = false;
	}

	public override float GetDiameter()
	{
		return monsterIndex10283.distance * 2f;
	}

	public override void Update()
	{
		if (m_bExecReturn)
		{
			m_DurationTime1 += Time.deltaTime;
			m_DurationTime2 += Time.deltaTime;
			if (m_DurationTime1 >= 2f / 3f && !m_bExecReturn1)
			{
				m_bExecReturn1 = true;
				ActionCoroutine1();
			}
			if (m_DurationTime2 >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine2();
			}
		}
		if (monster.fsm.GetStateId() != EMonsterState.Move || IsBuffStop())
		{
			return;
		}
		monsterIndex10283.everyTimeKeep += Time.deltaTime;
		if (monsterIndex10283.everyTimeKeep >= monsterIndex10283.everyTime)
		{
			monsterIndex10283.everyTimeKeep = 0f;
			monster.canMoveOrAttack = false;
			TrackEntry val = PlayTackAnimation(1, flag: false);
			if (val != null)
			{
				m_DurationMax = val.AnimationEnd;
			}
			else
			{
				m_DurationMax = 1.8333334f;
			}
			m_bExecReturn = true;
			m_DurationTime1 = 0f;
			m_DurationTime2 = 0f;
			m_bExecReturn1 = false;
			CreateEffectSpine();
		}
	}

	public void ActionCoroutine1()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if (!monster.Value.isEliteBoss && base.monster.GetTid() != monster.Value.GetTid() && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) > 0.5f && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < monsterIndex10283.distance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 150f)
			{
				zbExtraDirection zbExtraDirection2 = new zbExtraDirection();
				zbExtraDirection2.direction = Vector3.down;
				zbExtraDirection2.duration = monsterIndex10283.durationTime;
				zbExtraDirection2.speed = monsterIndex10283.flyDistance / zbExtraDirection2.duration;
				monster.Value.AddExtraDirectionList(zbExtraDirection2);
				break;
			}
		}
	}

	public void ActionCoroutine2()
	{
		FreeEffectSpine();
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}
}
