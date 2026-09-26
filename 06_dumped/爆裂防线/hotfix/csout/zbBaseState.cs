public class zbBaseState
{
	protected EGameState m_eState;

	public zbBaseState(EGameState eState)
	{
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
