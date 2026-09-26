public class zbMiniMapState
{
	protected EMiniMapState m_eState;

	protected zbMiniMapStateMachine m_fsm;

	public zbMiniMapState(zbMiniMapStateMachine fsm, EMiniMapState eState)
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
