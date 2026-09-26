using System.Collections.Generic;
using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10061 : zbMonsterSkillBase
{
	public stMonsterSkillIndex10061 data = new stMonsterSkillIndex10061();

	public List<int> callMonsterUUIDList = new List<int>();

	private int monsterUUID;

	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		data = new stMonsterSkillIndex10061();
		data.everyTime = monsterSkillRecord.param[0];
		data.distance = monsterSkillRecord.param[1] / 100f;
		data.attrPercent = monsterSkillRecord.param[2];
		data.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override float GetDiameter()
	{
		return data.distance * 2f;
	}

	public override void Update()
	{
		if (IsBuffStop() || (Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			return;
		}
		data.everyTimeKeep += Time.deltaTime;
		if (data.everyTimeKeep >= data.everyTime)
		{
			CreateEffectSpine();
			monster.canMoveOrAttack = false;
			data.everyTimeKeep = 0f;
			m_bExecReturn = true;
			TrackEntry val = PlayTackAnimation(1, flag: false);
			if (val != null)
			{
				m_DurationMax = val.AnimationEnd;
			}
			else
			{
				m_DurationMax = 1f / 3f;
			}
			m_DurationTime = 0f;
		}
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine();
			}
		}
	}

	public void ActionCoroutine()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		combatData.monsters.GetRangeAllMonsterList(((Component)monster).transform.position, monsterRangeAllList, data.distance);
		foreach (zbMonster monsterRangeAll in monsterRangeAllList)
		{
			monsterRangeAll.ChangeHp(null, null, (int)(data.attrPercent * (float)(long)monsterRangeAll.maxHp));
		}
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
		FreeEffectSpine();
	}
}
