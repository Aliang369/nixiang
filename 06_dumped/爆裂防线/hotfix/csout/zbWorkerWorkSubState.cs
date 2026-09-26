public class zbWorkerWorkSubState
{
	protected EWorkerWorkSubState m_eState;

	protected zbWorkerWorkSubFsm m_fsm;

	public zbWorkerWorkSubState(zbWorkerWorkSubFsm fsm, EWorkerWorkSubState eState)
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
}
