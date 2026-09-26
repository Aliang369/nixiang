public class zbPaotaiIdleState : zbPaotaiState
{
	public zbPaotaiIdleState(zbPaotaiStateMachine fsm, EPaotaiState eState)
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
		m_fsm.paotai.SetAnimation("animation", loop: true);
		m_fsm.SetState(EPaotaiState.Shoot);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
