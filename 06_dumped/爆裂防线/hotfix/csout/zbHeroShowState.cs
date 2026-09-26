public class zbHeroShowState : zbHeroState
{
	public zbHeroShowState(zbHeroStateMachine fsm, EHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.SetState(EHeroState.Skill);
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
