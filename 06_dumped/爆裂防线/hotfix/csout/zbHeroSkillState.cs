public class zbHeroSkillState : zbHeroState
{
	public zbHeroSkillState(zbHeroStateMachine fsm, EHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		m_fsm.hero.OnSkillUpdate();
	}
}
