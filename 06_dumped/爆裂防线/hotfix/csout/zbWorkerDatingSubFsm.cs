using System.Collections.Generic;

public class zbWorkerDatingSubFsm
{
	public zbWorker worker;

	private EWorkerDatingSubState m_eState;

	private EWorkerDatingSubState m_eNextState;

	private Dictionary<EWorkerDatingSubState, zbWorkerDatingSubState> m_kStates = new Dictionary<EWorkerDatingSubState, zbWorkerDatingSubState>();

	public void Init(zbWorker _worker)
	{
		worker = _worker;
		m_eState = EWorkerDatingSubState.Unknown;
		m_kStates[EWorkerDatingSubState.Idle] = new zbWorkerDatingSubIdleState(this, EWorkerDatingSubState.Idle);
		m_kStates[EWorkerDatingSubState.MoveToRoom] = new zbWorkerDatingSubMoveToRoomState(this, EWorkerDatingSubState.MoveToRoom);
		m_kStates[EWorkerDatingSubState.RoomDestory] = new zbWorkerDatingSubRoomDestroyNotifyState(this, EWorkerDatingSubState.RoomDestory);
		m_kStates[EWorkerDatingSubState.Exit] = new zbWorkerDatingSubExitState(this, EWorkerDatingSubState.Exit);
	}

	public void SetState(EWorkerDatingSubState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EWorkerDatingSubState GetStateId()
	{
		return m_eState;
	}

	public zbWorkerDatingSubState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbWorkerDatingSubState GetStateById(EWorkerDatingSubState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		zbWorkerDatingSubState state = GetState();
		EWorkerDatingSubState stateId = GetStateId();
		if (state != null && stateId != EWorkerDatingSubState.Exit)
		{
			state.OnEnable();
		}
		else
		{
			SetState(EWorkerDatingSubState.MoveToRoom);
		}
	}

	public void Update()
	{
		zbWorkerDatingSubState state = GetState();
		zbWorkerDatingSubState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EWorkerDatingSubState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
