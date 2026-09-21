internal class HintOverloadAction : ExecuteAction
{
	public HintOverloadAction()
	{
		Ignore(ActionLock.Type.HintOverload);
	}

	protected override void Update()
	{
		gameState.UIManager.playerItemArea.OnHintOverload();
	}
}
