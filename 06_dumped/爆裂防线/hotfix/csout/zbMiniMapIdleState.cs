public class zbMiniMapIdleState : zbMiniMapState
{
	public zbMiniMapIdleState(zbMiniMapStateMachine fsm, EMiniMapState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		m_fsm.m_lastX = m_fsm.miniMap.nCurPlayerGridX;
		m_fsm.m_lastY = m_fsm.miniMap.nCurPlayerGridY;
		m_fsm.miniMap.RefreshCurGridPlayerPos();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (m_fsm.m_lastX != m_fsm.miniMap.nCurPlayerGridX || m_fsm.m_lastY != m_fsm.miniMap.nCurPlayerGridY)
		{
			m_fsm.SetState(EMiniMapState.MoveToGrid);
		}
	}
}
