using System.Collections.Generic;

public class zbRoomBuildingSubFsm
{
	public zbRoom room;

	private ERoomBuildingSubState m_eState;

	private ERoomBuildingSubState m_eNextState;

	private Dictionary<ERoomBuildingSubState, zbRoomBuildingSubState> m_kStates = new Dictionary<ERoomBuildingSubState, zbRoomBuildingSubState>();

	public void Init(zbRoom _room)
	{
		room = _room;
		m_eState = ERoomBuildingSubState.Unknown;
		m_kStates[ERoomBuildingSubState.In] = new zbRoomBuildingSubInState(this, ERoomBuildingSubState.In);
		m_kStates[ERoomBuildingSubState.Loop] = new zbRoomBuildingSubLoopState(this, ERoomBuildingSubState.Loop);
		m_kStates[ERoomBuildingSubState.Out] = new zbRoomBuildingSubOutState(this, ERoomBuildingSubState.Out);
		m_kStates[ERoomBuildingSubState.Exit] = new zbRoomBuildingSubExitState(this, ERoomBuildingSubState.Exit);
	}

	public void SetState(ERoomBuildingSubState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public ERoomBuildingSubState GetStateId()
	{
		return m_eState;
	}

	public zbRoomBuildingSubState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbRoomBuildingSubState GetStateById(ERoomBuildingSubState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		zbRoomBuildingSubState state = GetState();
		ERoomBuildingSubState stateId = GetStateId();
		if (state != null && stateId != ERoomBuildingSubState.Exit)
		{
			state.OnEnable();
		}
		else
		{
			SetState(ERoomBuildingSubState.In);
		}
	}

	public void Update()
	{
		zbRoomBuildingSubState state = GetState();
		zbRoomBuildingSubState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = ERoomBuildingSubState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
