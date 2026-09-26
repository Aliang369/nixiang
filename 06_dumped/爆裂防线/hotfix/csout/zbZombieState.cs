public class zbZombieState
{
	protected EZombieState m_eState;

	protected zbZombieStateMachine m_fsm;

	public zbZombieState(zbZombieStateMachine fsm, EZombieState eState)
	{
		m_fsm = fsm;
		m_eState = eState;
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}

	public virtual void OnEnable()
	{
	}

	public virtual void OnRoomDestroyNotfiy()
	{
	}
}
