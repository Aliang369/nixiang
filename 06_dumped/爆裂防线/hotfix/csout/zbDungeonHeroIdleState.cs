using UnityEngine;

public class zbDungeonHeroIdleState : zbDungeonHeroState
{
	public zbDungeonHeroIdleState(zbDungeonHeroStateMachine fsm, EDungeonHeroState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		m_fsm.hero.SetAnimation("idle", loop: true);
	}

	public override void ProcessMoveInput(Vector2 joystickInput)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.ProcessMoveInput(joystickInput);
		if (!(Vector2.op_Implicit(joystickInput) == Vector3.zero))
		{
			m_fsm.SetState(EDungeonHeroState.Run);
		}
	}
}
