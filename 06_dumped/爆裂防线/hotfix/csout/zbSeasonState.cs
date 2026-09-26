public class zbSeasonState
{
	protected ESeasonState m_eState;

	protected zbSeasonStateMachine m_fsm;

	public zbSeasonState(zbSeasonStateMachine fsm, ESeasonState eState)
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

	public virtual void OnEnable()
	{
	}
}
