public class HitNFXConfig : NFXConfig
{
	public void Play(GameEvent eve, PivotManager.IWithPivot targetPivot)
	{
		Play(eve.gameState, targetPivot);
	}

	public void Play(GameStateInfo gs, PivotManager.IWithPivot targetPivot)
	{
		if (!(setup == null))
		{
			gs.actionManager.EnqueueAction(new DisplayNFXAction(setup, targetPivot));
		}
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.HitUI;
	}
}
