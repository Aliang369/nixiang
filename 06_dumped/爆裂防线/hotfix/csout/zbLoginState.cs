public class zbLoginState : zbBaseState
{
	public zbLoginState(EGameState eState)
		: base(eState)
	{
	}

	public override void OnEnter()
	{
		base.OnEnter();
		zbAssetLoader.Instance.LoadScene("scene/login.unity3d");
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
