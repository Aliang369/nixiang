using System.Collections.Generic;

public class zbCombatStateMachine
{
	private ECombatState m_eState;

	private ECombatState m_eNextState;

	private Dictionary<ECombatState, zbCombatState> m_kStates = new Dictionary<ECombatState, zbCombatState>();

	public void Init()
	{
		m_eState = ECombatState.Unknown;
		m_kStates[ECombatState.Ready] = new zbCombatState_Ready(this, ECombatState.Ready);
		m_kStates[ECombatState.Fight] = new zbCombatState_Fight(this, ECombatState.Fight);
		m_kStates[ECombatState.Balance] = new zbCombatState_Balance(this, ECombatState.Balance);
	}

	public void Free()
	{
	}

	public void SetState(ECombatState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ECombatState GetStateId()
	{
		return m_eState;
	}

	public zbCombatState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbCombatState GetStateById(ECombatState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbCombatState state = GetState();
		zbCombatState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ECombatState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
