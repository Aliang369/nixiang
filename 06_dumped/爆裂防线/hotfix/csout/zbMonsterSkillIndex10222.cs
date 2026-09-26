using Spine;
using UnityEngine;

public class zbMonsterSkillIndex10222 : zbMonsterSkillBase
{
	private float m_DurationMax;

	private float m_DurationTime;

	private bool m_bExecReturn;

	public int monsterUUID;

	public override void RefreshProperty()
	{
		base.RefreshProperty();
		monster.monsterIndex10222.lastLostHp = 0L;
		monster.monsterIndex10222.preLostHpPercent = monsterSkillRecord.param[0];
		monster.monsterIndex10222.cureHpPercent = monsterSkillRecord.param[1];
		monster.monsterIndex10222.skillIndexTake = true;
		monster.monsterIndex10222.skillBase = this;
		monsterUUID = monster.GetUUID();
		m_DurationMax = 0f;
		m_DurationTime = 0f;
		m_bExecReturn = false;
	}

	public override float GetDiameter()
	{
		return 0.5f;
	}

	public override void CallBackLostHp(long value)
	{
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.monsterIndex10222.lastLostHp -= value;
			if (!IsBuffStop())
			{
				ActionSequence();
			}
		}
	}

	public void ActionSequence()
	{
		int num = (int)(monster.monsterIndex10222.preLostHpPercent * (float)(long)monster.maxHp);
		if (monster.monsterIndex10222.lastLostHp >= num)
		{
			TrackEntry val = PlayTackAnimation(1, flag: false);
			CreateEffectSpine();
			while (monster.monsterIndex10222.lastLostHp >= num)
			{
				monster.ChangeHp(null, null, (long)(monster.monsterIndex10222.cureHpPercent * (float)(long)monster.maxHp));
				monster.monsterIndex10222.lastLostHp = monster.monsterIndex10222.lastLostHp - num;
			}
			monster.canMoveOrAttack = false;
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

	public void CleanupSequence()
	{
		FreeEffectSpine();
		if (!((Object)(object)combatData.monsters.GetMonsterById(monsterUUID) == (Object)null))
		{
			monster.anim.spineAnimation.StopAnimation(1);
			monster.canMoveOrAttack = true;
		}
	}

	public override void Update()
	{
		if (m_bExecReturn)
		{
			m_DurationTime += Time.deltaTime;
			if (m_DurationTime >= m_DurationMax)
			{
				m_bExecReturn = false;
				CleanupSequence();
			}
		}
	}

	public override void ClearList()
	{
		monster.monsterIndex10222.skillIndexTake = false;
	}
}
