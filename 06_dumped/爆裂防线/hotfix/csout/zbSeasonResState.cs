public class zbSeasonResState
{
	protected ESeasonResState m_eState;

	protected zbSeasonResStateMachine m_fsm;

	public zbSeasonResState(zbSeasonResStateMachine fsm, ESeasonResState eState)
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
