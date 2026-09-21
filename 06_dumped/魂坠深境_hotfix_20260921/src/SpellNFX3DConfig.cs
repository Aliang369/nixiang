public class SpellNFX3DConfig : NFXConfig
{
	public void Play(GameEvent eve)
	{
		if (eve?.gameState != null && eve.triggerType == GameEvent.TriggerType.Real && !(setup == null))
		{
			BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
			if (eventBFT != null)
			{
				eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, eventBFT.Get3DNFXPivot(setup)));
			}
		}
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.Hit3D;
	}
}
