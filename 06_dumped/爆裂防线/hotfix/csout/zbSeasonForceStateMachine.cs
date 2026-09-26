using System.Collections.Generic;

public class zbSeasonForceStateMachine
{
	private ESeasonForceState m_eState;

	private ESeasonForceState m_eNextState;

	private Dictionary<ESeasonForceState, zbSeasonForceState> m_kStates = new Dictionary<ESeasonForceState, zbSeasonForceState>();

	public void Init()
	{
		m_eState = ESeasonForceState.Unknown;
	}

	public zbSeasonForceState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbSeasonForceState GetStateById(ESeasonForceState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void SetState(ESeasonForceState eState)
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
		zbSeasonForceState state = GetState();
		zbSeasonForceState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ESeasonForceState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
