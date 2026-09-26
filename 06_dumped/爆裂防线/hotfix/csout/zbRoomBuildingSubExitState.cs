using Proto.Types;

public class zbRoomBuildingSubExitState : zbRoomBuildingSubState
{
	public zbRoomBuildingSubExitState(zbRoomBuildingSubFsm fsm, ERoomBuildingSubState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		WarRoomDetail warRoomDetail = zbSelf.Instance.homelandHandler.GetWarRoomDetail(m_fsm.room.roomData.RoomId);
		if (warRoomDetail != null)
		{
			zbSelf.Instance.homelandHandler.warRoomTypeList.Remove(warRoomDetail.SkillType);
		}
		m_fsm.room.Refresh(m_fsm.room.roomData);
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
