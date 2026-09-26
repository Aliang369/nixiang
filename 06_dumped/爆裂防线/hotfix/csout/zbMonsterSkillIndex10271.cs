using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10271 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	private int monsterUUID;

	public stMonsterSkillIndex10271 monsterIndex10271 = new stMonsterSkillIndex10271();

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monsterIndex10271.everyTime = monsterSkillRecord.param[0];
		monsterIndex10271.attrPercent = monsterSkillRecord.param[1];
		monsterIndex10271.everyTimeKeep = 0f;
		monsterUUID = monster.GetUUID();
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override void Update()
	{
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				ActionCoroutine();
			}
		}
		if (IsBuffStop() || (Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			return;
		}
		monsterIndex10271.everyTimeKeep += Time.deltaTime;
		if (monster.curHp >= monster.maxHp)
		{
			return;
		}
		if (monsterIndex10271.everyTimeKeep >= monsterIndex10271.everyTime)
		{
			CreateEffectSpine();
			monsterIndex10271.everyTimeKeep = 0f;
			monster.canMoveOrAttack = false;
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
			monster.ChangeHp(null, null, (long)(monsterIndex10271.attrPercent * (float)(long)monster.maxHp));
		}
		else
		{
			FreeEffectSpine();
		}
	}

	public void ActionCoroutine()
	{
		if ((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null)
		{
			FreeEffectSpine();
			return;
		}
		monster.anim.spineAnimation.StopAnimation(1);
		monster.canMoveOrAttack = true;
	}

	public override void ClearList()
	{
	}
}
