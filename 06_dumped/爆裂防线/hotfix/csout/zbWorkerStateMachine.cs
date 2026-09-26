using System.Collections.Generic;

public class zbWorkerStateMachine
{
	public zbWorker worker;

	private EWorkerState m_eState;

	private EWorkerState m_eNextState;

	private Dictionary<EWorkerState, zbWorkerState> m_kStates = new Dictionary<EWorkerState, zbWorkerState>();

	public void Init(zbWorker _worker)
	{
		worker = _worker;
		m_eState = EWorkerState.Unknown;
		m_kStates[EWorkerState.Work] = new zbWorkerWorkState(this, EWorkerState.Work);
		m_kStates[EWorkerState.Dating] = new zbWorkerDatingState(this, EWorkerState.Dating);
	}

	public void SetState(EWorkerState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EWorkerState GetStateId()
	{
		return m_eState;
	}

	public zbWorkerState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbWorkerState GetStateById(EWorkerState eState)
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
		zbWorkerState state = GetState();
		zbWorkerState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EWorkerState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}

	public void OnRoomDestroyNotfiy()
	{
		GetState()?.OnRoomDestroyNotfiy();
	}
}
