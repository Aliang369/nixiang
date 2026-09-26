using System.Collections.Generic;

public class zbSeasonCityStateMachine
{
	private ESeasonCityState m_eState;

	private ESeasonCityState m_eNextState;

	private Dictionary<ESeasonCityState, zbSeasonCityState> m_kStates = new Dictionary<ESeasonCityState, zbSeasonCityState>();

	public void Init()
	{
		m_eState = ESeasonCityState.Unknown;
		m_kStates[ESeasonCityState.Idle] = new zbSeasonCityIdleState(this, ESeasonCityState.Idle);
		m_kStates[ESeasonCityState.UnderAttack] = new zbSeasonCityUnderAttackState(this, ESeasonCityState.UnderAttack);
		m_kStates[ESeasonCityState.Fallen] = new zbSeasonCityIdleState(this, ESeasonCityState.Fallen);
	}

	public zbSeasonCityState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbSeasonCityState GetStateById(ESeasonCityState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void SetState(ESeasonCityState eState)
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
		zbSeasonCityState state = GetState();
		zbSeasonCityState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ESeasonCityState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
