public class zbWsHeartStateBase
{
	protected zbWsHeartStateMachine m_fsm;

	protected EHeartState m_eState;

	public zbWsHeartStateBase(zbWsHeartStateMachine fsm, EHeartState eState)
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

	public virtual void OnNotifyHeartAck()
	{
	}

	public virtual void OnNotifySocketOpen()
	{
	}
}
