using UnityEngine;

public class zbMonsterShowState : zbMonsterState
{
	public zbMonsterShowState(zbMonsterStateMachine fsm, EMonsterState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.SetState(EMonsterState.Move);
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		if (zbCombatSystem.Instance.scrollSpeed > 0f && ((Component)m_fsm.monster).transform.position.y > m_fsm.monster.combatData.defense.GetAlertPositionY() + 0.5f)
		{
			Transform transform = ((Component)m_fsm.monster).transform;
			transform.position += Vector3.down * zbCombatSystem.Instance.scrollSpeed * Time.deltaTime;
		}
	}
}
