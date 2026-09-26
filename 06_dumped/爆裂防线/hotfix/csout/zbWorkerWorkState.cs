using UnityEngine;

public class zbWorkerWorkState : zbWorkerState
{
	protected zbWorkerWorkSubFsm m_subFsm;

	private int m_nLastWorkRoomId;

	private zbRoom m_nLastWorkRoom;

	public zbWorkerWorkState(zbWorkerStateMachine fsm, EWorkerState eState)
		: base(fsm, eState)
	{
		m_subFsm = new zbWorkerWorkSubFsm();
		m_subFsm.Init(fsm.worker);
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
		m_nLastWorkRoomId = m_fsm.worker.m_homelandWorkerDetail.RoomId;
		m_nLastWorkRoom = zbHomeMgr.Instance.roomMgr.GetRoom(m_nLastWorkRoomId);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		m_subFsm.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
		m_subFsm.SetState(EWorkerWorkSubState.Exit);
		m_subFsm.Update();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_subFsm.Update();
		if (m_nLastWorkRoomId != m_fsm.worker.m_homelandWorkerDetail.RoomId)
		{
			m_nLastWorkRoomId = m_fsm.worker.m_homelandWorkerDetail.RoomId;
			m_subFsm.SetState(EWorkerWorkSubState.RoomDestory);
		}
		else if ((Object)(object)m_nLastWorkRoom != (Object)null && !m_nLastWorkRoom.IsSameRoom(zbHomeMgr.Instance.roomMgr.GetRoom(m_fsm.worker.m_homelandWorkerDetail.RoomId)))
		{
			m_nLastWorkRoom = zbHomeMgr.Instance.roomMgr.GetRoom(m_fsm.worker.m_homelandWorkerDetail.RoomId);
			m_subFsm.SetState(EWorkerWorkSubState.RoomDestory);
		}
	}

	public override void OnRoomDestroyNotfiy()
	{
		base.OnRoomDestroyNotfiy();
		m_subFsm.SetState(EWorkerWorkSubState.RoomDestory);
	}
}
