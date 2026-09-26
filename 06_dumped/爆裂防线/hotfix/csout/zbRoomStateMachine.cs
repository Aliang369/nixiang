using System.Collections.Generic;

public class zbRoomStateMachine
{
	public zbRoom room;

	private ERoomState m_eState;

	private ERoomState m_eNextState;

	private Dictionary<ERoomState, zbRoomState> m_kStates = new Dictionary<ERoomState, zbRoomState>();

	public void Init(zbRoom _room)
	{
		room = _room;
		m_eState = ERoomState.Unknown;
		m_kStates[ERoomState.Idle] = new zbRoomIdleState(this, ERoomState.Idle);
		m_kStates[ERoomState.Building] = new zbRoomBuildingState(this, ERoomState.Building);
		m_kStates[ERoomState.Destroy] = new zbRoomDestroyState(this, ERoomState.Destroy);
	}

	public void SetState(ERoomState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ERoomState GetStateId()
	{
		return m_eState;
	}

	public zbRoomState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbRoomState GetStateById(ERoomState eState)
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
		zbRoomState state = GetState();
		zbRoomState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ERoomState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
