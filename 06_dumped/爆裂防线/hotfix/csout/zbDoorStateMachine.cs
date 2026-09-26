using System.Collections.Generic;

public class zbDoorStateMachine
{
	public zbRoom room;

	public bool isLeftDoor = true;

	private EDoorState m_eState;

	private EDoorState m_eNextState;

	private Dictionary<EDoorState, zbDoorState> m_kStates = new Dictionary<EDoorState, zbDoorState>();

	public void Init(zbRoom _room, bool _isLeftDoor)
	{
		room = _room;
		isLeftDoor = _isLeftDoor;
		m_eState = EDoorState.Unknown;
		m_kStates[EDoorState.Init] = new zbDoorInitState(this, EDoorState.Init);
		m_kStates[EDoorState.Open] = new zbDoorOpenState(this, EDoorState.Open);
		m_kStates[EDoorState.Close] = new zbDoorCloseState(this, EDoorState.Close);
		SetState(EDoorState.Init);
	}

	public void SetState(EDoorState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EDoorState GetStateId()
	{
		return m_eState;
	}

	public zbDoorState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbDoorState GetStateById(EDoorState eState)
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
		zbDoorState state = GetState();
		zbDoorState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EDoorState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
