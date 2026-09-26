public class zbPaotaiState
{
	protected EPaotaiState m_eState;

	protected zbPaotaiStateMachine m_fsm;

	public zbPaotaiState(zbPaotaiStateMachine fsm, EPaotaiState eState)
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
