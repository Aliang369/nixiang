public class zbMainState : zbBaseState
{
	public zbMainState(EGameState eState)
		: base(eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		zbWsHeartStateMachine.Instance.SetState(EHeartState.Unknown);
		zbWsHeartStateMachine.Instance.SetState(EHeartState.Heart);
		zbSelf.CreateInstatnce();
		zbSelf.Instance.Init();
		zbGuide.CreateInstatnce();
		zbGuide.Instance.Init();
		zbProgressEventHelper.Instance.Init();
		zbAssetLoader.Instance.LoadScene("scene/main.unity3d");
	}

	public override void OnExit()
	{
		zbSelf.Instance.Free();
		base.OnExit();
	}

	public override void OnUpdate()
	{
		zbSelf.Instance.Update();
		zbChannel_Pay.Update();
		zbGuide.Instance.Update();
	}
}
