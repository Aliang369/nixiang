public class zbFloorBuildingSubExitState : zbFloorBuildingSubState
{
	public zbFloorBuildingSubExitState(zbFloorBuildingSubFsm fsm, EFloorBuildingSubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.floor.Refresh();
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
