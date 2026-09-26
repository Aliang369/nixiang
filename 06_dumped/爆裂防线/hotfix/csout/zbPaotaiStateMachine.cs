using System.Collections.Generic;

public class zbPaotaiStateMachine
{
	public zbPaotai paotai;

	private EPaotaiState m_eState;

	private EPaotaiState m_eNextState;

	private Dictionary<EPaotaiState, zbPaotaiState> m_kStates = new Dictionary<EPaotaiState, zbPaotaiState>();

	public void Init(zbPaotai _worker)
	{
		paotai = _worker;
		m_eState = EPaotaiState.Unknown;
		m_kStates[EPaotaiState.Idle] = new zbPaotaiIdleState(this, EPaotaiState.Idle);
		m_kStates[EPaotaiState.Shoot] = new zbPaotaiShootState(this, EPaotaiState.Shoot);
		m_kStates[EPaotaiState.ShootSweep] = new zbPaotaiShootSweepState(this, EPaotaiState.ShootSweep);
		SetState(EPaotaiState.Idle);
	}

	public void SetState(EPaotaiState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EPaotaiState GetStateId()
	{
		return m_eState;
	}

	public zbPaotaiState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbPaotaiState GetStateById(EPaotaiState eState)
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
		zbPaotaiState state = GetState();
		zbPaotaiState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EPaotaiState.Unknown;
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
