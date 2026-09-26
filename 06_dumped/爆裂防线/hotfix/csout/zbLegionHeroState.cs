public class zbLegionHeroState
{
	protected zbLegionHeroMachine m_fsm;

	public zbLegionHero legionHero;

	public zbLegionHeroState(zbLegionHeroMachine fsm)
	{
		m_fsm = fsm;
	}

	public virtual void OnEnter()
	{
		legionHero = m_fsm.legionHero;
		legionHero.SetPlayerVisible(visible: true);
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}
}
