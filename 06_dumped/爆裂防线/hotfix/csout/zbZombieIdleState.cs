public class zbZombieIdleState : zbZombieState
{
	public zbZombieIdleState(zbZombieStateMachine fsm, EZombieState eState)
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
