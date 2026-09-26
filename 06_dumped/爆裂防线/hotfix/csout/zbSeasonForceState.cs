public class zbSeasonForceState
{
	protected ESeasonForceState m_eState;

	protected zbSeasonForceStateMachine m_fsm;

	public zbSeasonForceState(zbSeasonForceStateMachine fsm, ESeasonForceState eState)
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
