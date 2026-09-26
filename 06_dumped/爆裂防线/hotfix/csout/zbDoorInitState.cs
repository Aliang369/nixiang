public class zbDoorInitState : zbDoorState
{
	public zbDoorInitState(zbDoorStateMachine fsm, EDoorState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (IsWorkerNearDoor())
		{
			m_fsm.SetState(EDoorState.Open);
		}
		else
		{
			m_fsm.SetState(EDoorState.Close);
		}
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
