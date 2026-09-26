using UnityEngine;

public class zbRoomDestroyState : zbRoomState
{
	protected zbRoomDestroySubFsm m_subFsm;

	public zbRoomDestroyState(zbRoomStateMachine fsm, ERoomState eState)
		: base(fsm, eState)
	{
		m_subFsm = new zbRoomDestroySubFsm();
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
		m_subFsm.SetState(ERoomDestroySubState.Exit);
		m_subFsm.Update();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_subFsm.Update();
	}
}
