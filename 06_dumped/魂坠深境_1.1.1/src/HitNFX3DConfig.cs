public class HitNFX3DConfig : NFXConfig
{
	public void Play(GameEvent eve, BattleFieldTransform toBFT)
	{
		if (toBFT != null && !(setup == null))
		{
			eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, toBFT.Get3DNFXPivot(setup)));
		}
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.Hit3D;
	}
}
