using System.Collections.Generic;

public class zbSeasonStateMachine
{
	public ESeasonState m_eState;

	private ESeasonState m_eNextState;

	private Dictionary<ESeasonState, zbSeasonState> m_kStates = new Dictionary<ESeasonState, zbSeasonState>();

	public void Init()
	{
		m_eState = ESeasonState.Unknown;
		m_kStates[ESeasonState.Running] = new zbSeasonRunningState(this, ESeasonState.Running);
		m_kStates[ESeasonState.CityWar] = new zbSeasonCityWarState(this, ESeasonState.CityWar);
		m_kStates[ESeasonState.Settlement] = new zbSeasonSettlementState(this, ESeasonState.Settlement);
	}

	public zbSeasonState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbSeasonState GetStateById(ESeasonState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void SetState(ESeasonState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public void OnEnable()
	{
		GetState()?.OnEnable();
	}

	public void Update()
	{
		zbSeasonState state = GetState();
		zbSeasonState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ESeasonState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
