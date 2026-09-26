using System.Collections.Generic;

public class zbSeasonResStateMachine
{
	private ESeasonResState m_eState;

	private ESeasonResState m_eNextState;

	private Dictionary<ESeasonResState, zbSeasonResState> m_kStates = new Dictionary<ESeasonResState, zbSeasonResState>();

	public void Init()
	{
		m_eState = ESeasonResState.Unknown;
		m_kStates[ESeasonResState.Locked] = new zbSeasonResLockedState(this, ESeasonResState.Locked);
		m_kStates[ESeasonResState.Unoccupied] = new zbSeasonResUnoccupiedState(this, ESeasonResState.Unoccupied);
		m_kStates[ESeasonResState.Occupying] = new zbSeasonResOccupyingState(this, ESeasonResState.Occupying);
		m_kStates[ESeasonResState.Occupied] = new zbSeasonResOccupiedState(this, ESeasonResState.Occupied);
	}

	public zbSeasonResState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbSeasonResState GetStateById(ESeasonResState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void SetState(ESeasonResState eState)
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
		zbSeasonResState state = GetState();
		zbSeasonResState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ESeasonResState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
