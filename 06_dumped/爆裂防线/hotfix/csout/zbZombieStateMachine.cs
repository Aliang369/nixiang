using System.Collections.Generic;

public class zbZombieStateMachine
{
	public zbZombie zombie;

	private EZombieState m_eState;

	private EZombieState m_eNextState;

	private Dictionary<EZombieState, zbZombieState> m_kStates = new Dictionary<EZombieState, zbZombieState>();

	public void Init(zbZombie _worker)
	{
		zombie = _worker;
		m_eState = EZombieState.Unknown;
		m_kStates[EZombieState.Show] = new zbZombieShowState(this, EZombieState.Show);
		m_kStates[EZombieState.Idle] = new zbZombieIdleState(this, EZombieState.Idle);
		m_kStates[EZombieState.Move] = new zbZombieMoveState(this, EZombieState.Move);
		m_kStates[EZombieState.Run] = new zbZombieRunState(this, EZombieState.Run);
		m_kStates[EZombieState.Die] = new zbZombieDieState(this, EZombieState.Die);
		m_kStates[EZombieState.Exit] = new zbZombieExitState(this, EZombieState.Exit);
	}

	public void SetState(EZombieState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EZombieState GetStateId()
	{
		return m_eState;
	}

	public zbZombieState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbZombieState GetStateById(EZombieState eState)
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
		zbZombieState state = GetState();
		zbZombieState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EZombieState.Unknown;
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
