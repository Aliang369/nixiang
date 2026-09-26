public class zbFloorIdleState : zbFloorState
{
	public zbFloorIdleState(zbFloorStateMachine fsm, EFloorState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.floor.Refresh();
	}
}
