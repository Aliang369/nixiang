public class zbFloorBuildingSubState
{
	protected EFloorBuildingSubState m_eState;

	protected zbFloorBuildingSubFsm m_fsm;

	public zbFloorBuildingSubState(zbFloorBuildingSubFsm fsm, EFloorBuildingSubState eState)
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
