using System;
using System.Collections.Generic;
using Proto.Enums;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10223 : zbMonsterSkillBase
{
	private int monsterUUID;

	private int targetMonsterTid;

	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10223.monsterSkill = this;
		monsterUUID = monster.GetUUID();
		monster.monsterIndex10223.defenseDistance = monsterSkillRecord.param[0] / 100f;
		monster.monsterIndex10223.distance = monsterSkillRecord.param[1] / 100f;
		monster.monsterIndex10223.mass = monsterSkillRecord.param[2];
		targetMonsterTid = 0;
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override void Update()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine();
			}
		}
		if (IsBuffStop() || ((Component)monster).transform.position.y >= monster.monsterIndex10223.defenseDistance + combatData.defense.GetAlertPositionY() || targetMonsterTid > 0)
		{
			return;
		}
		foreach (KeyValuePair<int, zbMonster> monster in combatData.monsters.monsterList)
		{
			if ((long)monster.Value.curHp > 0 && !monster.Value.isEliteBoss && monster.Value.GetMass() <= base.monster.monsterIndex10223.mass && base.monster.GetCurPositionXY().y > monster.Value.GetCurPositionXY().y && zbUtil.GetVector2Distance(((Component)base.monster).transform.position, monster.Value.GetCurPosition()) < base.monster.monsterIndex10223.distance && Math.Abs(base.monster.GetCurPositionXY().x - monster.Value.GetCurPositionXY().x) <= base.monster.GetAttrValueByType(EAttrMonsterType.BodyScale) * (float)base.monster.monsterAvatarRecord.boxWidth / 100f)
			{
				targetMonsterTid = monster.Value.GetTid();
				monster.Value.SetCurHp(0L);
				break;
			}
		}
		if (targetMonsterTid != 0)
		{
			base.monster.canMoveOrAttack = false;
			TrackEntry val = PlayTackAnimation(1, flag: false);
			if (val != null)
			{
				m_DurationMax = val.AnimationEnd;
			}
			else
			{
				m_DurationMax = 1.2f;
			}
			m_bExecReturn = true;
			m_DurationTime = 0f;
		}
	}

	public void ActionCoroutine()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
		((Component)monster).transform.localScale = Vector3.one * 1.5f * monster.GetAttrValueByType(EAttrMonsterType.BodyScale);
	}

	public override void TriggerDieSkill(Action callback)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (targetMonsterTid > 0)
		{
			combatData.monsters.SpawnMonsterAtPosition(targetMonsterTid, ((Component)monster).transform.position, ((Component)monster).transform.parent);
		}
		callback?.Invoke();
	}
}
