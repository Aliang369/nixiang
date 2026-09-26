public class zbZombieShowState : zbZombieState
{
	public zbZombieShowState(zbZombieStateMachine fsm, EZombieState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		OnEnable();
		m_fsm.zombie.SetAnimation("idle", loop: true);
		m_fsm.SetState(EZombieState.Move);
	}

	public override void OnEnable()
	{
		base.OnEnable();
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
