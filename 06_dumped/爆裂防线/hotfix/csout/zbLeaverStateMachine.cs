using System.Collections.Generic;

public class zbLeaverStateMachine
{
	public zbLeaver worker;

	private ELeaverState m_eState;

	private ELeaverState m_eNextState;

	private Dictionary<ELeaverState, zbLeaverState> m_kStates = new Dictionary<ELeaverState, zbLeaverState>();

	public void Init(zbLeaver _worker)
	{
		worker = _worker;
		m_eState = ELeaverState.Unknown;
		m_kStates[ELeaverState.ChangeClose] = new zbLeaverChangeClothState(this, ELeaverState.ChangeClose);
		m_kStates[ELeaverState.MoveToExit] = new zbLeaverMoveToExitState(this, ELeaverState.MoveToExit);
		m_kStates[ELeaverState.RoomDestroyNotfiy] = new zbLeaverRoomDestroyNotifyState(this, ELeaverState.RoomDestroyNotfiy);
	}

	public void OnEnable()
	{
		GetState()?.OnEnable();
	}

	public void SetState(ELeaverState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ELeaverState GetStateId()
	{
		return m_eState;
	}

	public zbLeaverState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbLeaverState GetStateById(ELeaverState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void Update()
	{
		zbLeaverState state = GetState();
		zbLeaverState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ELeaverState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
