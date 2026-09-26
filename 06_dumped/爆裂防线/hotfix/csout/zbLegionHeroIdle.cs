using UnityEngine;

public class zbLegionHeroIdle : zbLegionHeroState
{
	private float m_idleTimer;

	private float m_nextIdleDuration;

	public zbLegionHeroIdle(zbLegionHeroMachine fsm)
		: base(fsm)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		legionHero.PlayAnimation("idle");
		m_idleTimer = 0f;
		m_nextIdleDuration = Random.Range(2f, 5f);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_idleTimer += Time.unscaledDeltaTime;
		if (!(m_idleTimer < m_nextIdleDuration))
		{
			m_idleTimer = 0f;
			m_nextIdleDuration = Random.Range(2f, 5f);
			float value = Random.value;
			if (value < 0.5f)
			{
				m_fsm.SetState(ELegionHeroState.Walk);
			}
			else if (value < 0.6f && !legionHero.m_isLeader && !m_fsm.exitChangeFloor)
			{
				m_fsm.exitChangeFloor = true;
				m_fsm.SetState(ELegionHeroState.Floor);
			}
			else if (value < 0.65f && legionHero.curFloor == 0 && !m_fsm.exitLeaveFloor)
			{
				m_fsm.exitLeaveFloor = true;
				m_fsm.SetState(ELegionHeroState.Leave);
			}
		}
	}
}
