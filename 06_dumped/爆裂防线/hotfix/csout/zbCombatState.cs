public class zbCombatState
{
	protected ECombatState m_eState;

	protected zbCombatStateMachine m_fsm;

	public zbCombatState(zbCombatStateMachine fsm, ECombatState eState)
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
