using System.Collections.Generic;

public class zbLegionHeroMachine
{
	public zbLegionHero legionHero;

	public bool exitChangeFloor;

	public bool exitLeaveFloor;

	private ELegionHeroState m_eState;

	private ELegionHeroState m_eNextState;

	private Dictionary<ELegionHeroState, zbLegionHeroState> m_kStates = new Dictionary<ELegionHeroState, zbLegionHeroState>();

	public void Init(zbLegionHero _hero)
	{
		legionHero = _hero;
		m_eState = ELegionHeroState.Unknown;
		m_kStates[ELegionHeroState.Idle] = new zbLegionHeroIdle(this);
		m_kStates[ELegionHeroState.Walk] = new zbLegionHeroWalk(this);
		m_kStates[ELegionHeroState.Floor] = new zbLegionHeroFloor(this);
		m_kStates[ELegionHeroState.Leave] = new zbLegionHeroLeave(this);
	}

	public void SetState(ELegionHeroState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ELegionHeroState GetStateId()
	{
		return m_eState;
	}

	public zbLegionHeroState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbLegionHeroState GetStateById(ELegionHeroState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbLegionHeroState state = GetState();
		zbLegionHeroState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ELegionHeroState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
