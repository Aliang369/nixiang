public class zbFloorBuildingState : zbFloorState
{
	protected zbFloorBuildingSubFsm m_subFsm;

	public zbFloorBuildingState(zbFloorStateMachine fsm, EFloorState eState)
		: base(fsm, eState)
	{
		m_subFsm = new zbFloorBuildingSubFsm();
		m_subFsm.Init(fsm.floor);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		m_subFsm.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_subFsm.SetState(EFloorBuildingSubState.Exit);
		m_subFsm.Update();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_subFsm.Update();
		if (m_subFsm.GetStateId() == EFloorBuildingSubState.Exit)
		{
			m_fsm.SetState(EFloorState.Idle);
		}
	}
}
