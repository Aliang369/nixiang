public class zbRoomDestroySubState
{
	protected ERoomDestroySubState m_eState;

	protected zbRoomDestroySubFsm m_fsm;

	public zbRoomDestroySubState(zbRoomDestroySubFsm fsm, ERoomDestroySubState eState)
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
