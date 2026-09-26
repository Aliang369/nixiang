public class zbLeaverState
{
	protected ELeaverState m_eState;

	protected zbLeaverStateMachine m_fsm;

	public zbLeaverState(zbLeaverStateMachine fsm, ELeaverState eState)
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
