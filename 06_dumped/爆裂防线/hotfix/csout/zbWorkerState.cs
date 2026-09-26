public class zbWorkerState
{
	protected EWorkerState m_eState;

	protected zbWorkerStateMachine m_fsm;

	public zbWorkerState(zbWorkerStateMachine fsm, EWorkerState eState)
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
