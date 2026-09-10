public class SpellNFXConfig : NFXConfig
{
	public void Play(GameEvent eve)
	{
		if (eve != null && eve.triggerType == GameEvent.TriggerType.Real && !(setup == null))
		{
			if (eve.CasterItem != null)
			{
				eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, eve.CasterItem));
			}
			else if (eve.CasterEnchant != null)
			{
				eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, eve.CasterEnchant.attachItem));
			}
			else if (eve.CasterRune != null)
			{
				eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, eve.CasterRune.attachItem));
			}
			else if (eve.CasterInfluence != null)
			{
				eve.gameState.actionManager.EnqueueAction(new DisplayNFXAction(setup, eve.CasterInfluence));
			}
		}
	}

	public override VfxType GetValidVfxType()
	{
		return VfxType.HitUI;
	}
}
