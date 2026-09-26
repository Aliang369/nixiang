public class zbRoomBuildingSubState
{
	protected ERoomBuildingSubState m_eState;

	protected zbRoomBuildingSubFsm m_fsm;

	public zbRoomBuildingSubState(zbRoomBuildingSubFsm fsm, ERoomBuildingSubState eState)
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
