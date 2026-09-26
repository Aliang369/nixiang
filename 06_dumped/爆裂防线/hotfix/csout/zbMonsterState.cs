using System;
using System.Collections.Generic;

public class zbMonsterState
{
	protected EMonsterState m_eState;

	protected zbMonsterStateMachine m_fsm;

	protected zbMonster m_monster;

	protected zbCombatSystem m_combatSystem;

	[NonSerialized]
	public List<zbMonster> monsterRandomList = new List<zbMonster>();

	public zbMonsterState(zbMonsterStateMachine fsm, EMonsterState eState)
	{
		m_fsm = fsm;
		m_eState = eState;
	}

	public virtual void OnEnter()
	{
		m_monster = m_fsm.monster;
		m_combatSystem = zbCombatSystem.Instance;
	}

	public virtual void OnExit()
	{
	}

	public virtual void OnUpdate()
	{
	}
}
