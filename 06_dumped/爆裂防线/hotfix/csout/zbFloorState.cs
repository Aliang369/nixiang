public class zbFloorState
{
	protected EFloorState m_eState;

	protected zbFloorStateMachine m_fsm;

	public zbFloorState(zbFloorStateMachine fsm, EFloorState eState)
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
