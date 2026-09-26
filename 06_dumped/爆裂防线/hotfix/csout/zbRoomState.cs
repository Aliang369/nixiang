public class zbRoomState
{
	protected ERoomState m_eState;

	protected zbRoomStateMachine m_fsm;

	public zbRoomState(zbRoomStateMachine fsm, ERoomState eState)
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
