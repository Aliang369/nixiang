using UnityEngine;

public class zbRoomBuildingState : zbRoomState
{
	protected zbRoomBuildingSubFsm m_subFsm;

	public zbRoomBuildingState(zbRoomStateMachine fsm, ERoomState eState)
		: base(fsm, eState)
	{
		m_subFsm = new zbRoomBuildingSubFsm();
		m_subFsm.Init(fsm.room);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
		((Component)m_fsm.room.m_spriteRenderer).gameObject.SetActive(false);
		m_fsm.room.EnsureSpineState("animation/home/room_building/room_building_SkeletonData.asset");
	}

	public override void OnEnable()
	{
		base.OnEnable();
		m_subFsm.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_subFsm.SetState(ERoomBuildingSubState.Exit);
		m_subFsm.Update();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_subFsm.Update();
		if (m_subFsm.GetStateId() == ERoomBuildingSubState.Exit)
		{
			m_fsm.SetState(ERoomState.Idle);
		}
	}
}
