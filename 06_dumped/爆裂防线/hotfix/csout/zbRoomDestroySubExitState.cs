public class zbRoomDestroySubExitState : zbRoomDestroySubState
{
	public zbRoomDestroySubExitState(zbRoomDestroySubFsm fsm, ERoomDestroySubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.room.RefreshRoomState();
		zbHomeMgr.Instance.roomMgr.RemoveRoom(m_fsm.room.roomData.RoomId);
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
