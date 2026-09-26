public class zbSeasonCityState
{
	protected ESeasonCityState m_eState;

	protected zbSeasonCityStateMachine m_fsm;

	public zbSeasonCityState(zbSeasonCityStateMachine fsm, ESeasonCityState eState)
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
