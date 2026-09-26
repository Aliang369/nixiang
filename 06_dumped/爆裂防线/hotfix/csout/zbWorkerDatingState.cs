public class zbWorkerDatingState : zbWorkerState
{
	protected zbWorkerDatingSubFsm m_subFsm;

	public zbWorkerDatingState(zbWorkerStateMachine fsm, EWorkerState eState)
		: base(fsm, eState)
	{
		m_subFsm = new zbWorkerDatingSubFsm();
		m_subFsm.Init(fsm.worker);
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
		m_subFsm.SetState(EWorkerDatingSubState.Exit);
		m_subFsm.Update();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_subFsm.Update();
	}

	public override void OnRoomDestroyNotfiy()
	{
		base.OnRoomDestroyNotfiy();
		m_subFsm.SetState(EWorkerDatingSubState.RoomDestory);
	}
}
