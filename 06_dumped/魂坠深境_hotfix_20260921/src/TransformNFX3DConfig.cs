public class TransformNFX3DConfig : NFXConfig
{
	public void Play(GameEvent eve, BattleFieldTransform fromBFT, BattleFieldTransform toBFT)
	{
		if (fromBFT != null && toBFT != null && !(setup == null))
		{
			ConvertablePivot fromPivot = fromBFT.Get3DNFXPivot(setup);
			ConvertablePivot toPivot = toBFT.Get3DNFXPivot(setup);
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				eve.gameState.actionManager.EnqueueAction(new PlayTransformVFXAction(setup, fromPivot, toPivot, is3d: true));
			}));
		}
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.Transform3D;
	}
}
