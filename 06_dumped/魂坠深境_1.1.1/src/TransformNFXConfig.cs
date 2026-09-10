public class TransformNFXConfig : NFXConfig
{
	public void Play(GameEvent eve, PivotManager.IWithPivot source, PivotManager.IWithPivot target)
	{
		eve.gameState.actionManager.EnqueueAction(new PlayTransformVFXAction(setup, source, target, is3d: false));
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.TransformUI;
	}
}
