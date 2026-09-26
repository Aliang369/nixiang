public class zbSeasonRunningState : zbSeasonState
{
	public zbSeasonRunningState(zbSeasonStateMachine fsm, ESeasonState eState)
		: base(fsm, eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
	}

	public override void OnEnable()
	{
		base.OnEnable();
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
