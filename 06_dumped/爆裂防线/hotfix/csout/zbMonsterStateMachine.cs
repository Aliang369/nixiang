public class zbMonsterStateMachine
{
	public zbMonster monster;

	private EMonsterState m_eState;

	private EMonsterState m_eNextState;

	private zbMonsterState[] m_kStates;

	public void Init(zbMonster m)
	{
		monster = m;
		m_eState = EMonsterState.Unknown;
		m_kStates = new zbMonsterState[5];
		m_kStates[1] = new zbMonsterShowState(this, EMonsterState.Show);
		m_kStates[2] = new zbMonsterMoveState(this, EMonsterState.Move);
		m_kStates[3] = new zbMonsterAttackState(this, EMonsterState.Attack);
		m_kStates[4] = new zbMonsterDieState(this, EMonsterState.Die);
	}

	public void SetState(EMonsterState eState)
	{
		if (m_eState != eState)
		{
			m_eNextState = eState;
		}
	}

	public EMonsterState GetStateId()
	{
		return m_eState;
	}

	public zbMonsterState GetState()
	{
		return GetStateById(m_eState);
	}

	public zbMonsterState GetStateById(EMonsterState eState)
	{
		if (eState < EMonsterState.Unknown || (int)eState >= m_kStates.Length)
		{
			return null;
		}
		return m_kStates[(int)eState];
	}

	public void Update()
	{
		zbMonsterState state = GetState();
		zbMonsterState stateById = GetStateById(m_eNextState);
		if (stateById != null)
		{
			state?.OnExit();
			m_eState = m_eNextState;
			m_eNextState = EMonsterState.Unknown;
			stateById.OnEnter();
		}
		else
		{
			state?.OnUpdate();
		}
	}
}
