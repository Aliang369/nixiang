using System.Collections.Generic;

public class zbFloorStateMachine
{
	public zbFloor floor;

	private EFloorState m_eState;

	private EFloorState m_eNextState;

	private Dictionary<EFloorState, zbFloorState> m_kStates = new Dictionary<EFloorState, zbFloorState>();

	public void Init(zbFloor _floor)
	{
		floor = _floor;
		m_eState = EFloorState.Unknown;
		m_kStates[EFloorState.Idle] = new zbFloorIdleState(this, EFloorState.Idle);
		m_kStates[EFloorState.Building] = new zbFloorBuildingState(this, EFloorState.Building);
	}

	public void SetState(EFloorState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EFloorState GetStateId()
	{
		return m_eState;
	}

	public zbFloorState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbFloorState GetStateById(EFloorState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		GetState()?.OnEnable();
	}

	public void Update()
	{
		zbFloorState state = GetState();
		zbFloorState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EFloorState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
