using System.Collections.Generic;

public class zbWorkerWorkSubFsm
{
	public zbWorker worker;

	private EWorkerWorkSubState m_eState;

	private EWorkerWorkSubState m_eNextState;

	private Dictionary<EWorkerWorkSubState, zbWorkerWorkSubState> m_kStates = new Dictionary<EWorkerWorkSubState, zbWorkerWorkSubState>();

	public void Init(zbWorker _worker)
	{
		worker = _worker;
		m_eState = EWorkerWorkSubState.Unknown;
		m_kStates[EWorkerWorkSubState.Idle] = new zbWorkerWorkSubIdleState(this, EWorkerWorkSubState.Idle);
		m_kStates[EWorkerWorkSubState.MoveToRoom] = new zbWorkerWorkSubMoveToRoomState(this, EWorkerWorkSubState.MoveToRoom);
		m_kStates[EWorkerWorkSubState.RoomDestory] = new zbWorkerWorkSubRoomDestroyNotifyState(this, EWorkerWorkSubState.RoomDestory);
		m_kStates[EWorkerWorkSubState.Exit] = new zbWorkerWorkSubExitState(this, EWorkerWorkSubState.Exit);
	}

	public void SetState(EWorkerWorkSubState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EWorkerWorkSubState GetStateId()
	{
		return m_eState;
	}

	public zbWorkerWorkSubState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbWorkerWorkSubState GetStateById(EWorkerWorkSubState eState)
	{
		if (m_kStates.ContainsKey(eState))
		{
			return m_kStates[eState];
		}
		return null;
	}

	public void OnEnable()
	{
		zbWorkerWorkSubState state = GetState();
		EWorkerWorkSubState stateId = GetStateId();
		if (state != null && stateId != EWorkerWorkSubState.Exit)
		{
			state.OnEnable();
		}
		else
		{
			SetState(EWorkerWorkSubState.MoveToRoom);
		}
	}

	public void Update()
	{
		zbWorkerWorkSubState state = GetState();
		zbWorkerWorkSubState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EWorkerWorkSubState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
