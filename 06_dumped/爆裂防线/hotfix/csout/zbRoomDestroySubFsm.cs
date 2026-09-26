using System.Collections.Generic;

public class zbRoomDestroySubFsm
{
	public zbRoom room;

	private ERoomDestroySubState m_eState;

	private ERoomDestroySubState m_eNextState;

	private Dictionary<ERoomDestroySubState, zbRoomDestroySubState> m_kStates = new Dictionary<ERoomDestroySubState, zbRoomDestroySubState>();

	public void Init(zbRoom _room)
	{
		room = _room;
		m_eState = ERoomDestroySubState.Unknown;
		m_kStates[ERoomDestroySubState.In] = new zbRoomDestroySubInState(this, ERoomDestroySubState.In);
		m_kStates[ERoomDestroySubState.Loop] = new zbRoomDestroySubLoopState(this, ERoomDestroySubState.Loop);
		m_kStates[ERoomDestroySubState.Out] = new zbRoomDestroySubOutState(this, ERoomDestroySubState.Out);
		m_kStates[ERoomDestroySubState.Exit] = new zbRoomDestroySubExitState(this, ERoomDestroySubState.Exit);
	}

	public void SetState(ERoomDestroySubState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ERoomDestroySubState GetStateId()
	{
		return m_eState;
	}

	public zbRoomDestroySubState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbRoomDestroySubState GetStateById(ERoomDestroySubState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		zbRoomDestroySubState state = GetState();
		ERoomDestroySubState stateId = GetStateId();
		if (state != null && stateId != ERoomDestroySubState.Exit)
		{
			state.OnEnable();
		}
		else
		{
			SetState(ERoomDestroySubState.In);
		}
	}

	public void Update()
	{
		zbRoomDestroySubState state = GetState();
		zbRoomDestroySubState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ERoomDestroySubState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
