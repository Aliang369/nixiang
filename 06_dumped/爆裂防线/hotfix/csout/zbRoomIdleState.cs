public class zbRoomIdleState : zbRoomState
{
	public zbRoomIdleState(zbRoomStateMachine fsm, ERoomState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.room.RefreshRoomState();
	}
}
