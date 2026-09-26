public class zbWorkerDatingSubState
{
	protected EWorkerDatingSubState m_eState;

	protected zbWorkerDatingSubFsm m_fsm;

	public zbWorkerDatingSubState(zbWorkerDatingSubFsm fsm, EWorkerDatingSubState eState)
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
