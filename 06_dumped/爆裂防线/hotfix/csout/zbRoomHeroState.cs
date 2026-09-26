public class zbRoomHeroState
{
	protected ERoomHeroState m_eState;

	protected zbRoomHeroStateMachine m_fsm;

	public zbRoomHeroState(zbRoomHeroStateMachine fsm, ERoomHeroState eState)
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
