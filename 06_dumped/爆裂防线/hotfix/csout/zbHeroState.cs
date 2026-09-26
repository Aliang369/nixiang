public class zbHeroState
{
	protected EHeroState m_eState;

	protected zbHeroStateMachine m_fsm;

	public zbHeroState(zbHeroStateMachine fsm, EHeroState eState)
	{
		m_fsm = fsm;
		m_eState = eState;
	}

	public virtual void OnEnter()
	{
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}
}
