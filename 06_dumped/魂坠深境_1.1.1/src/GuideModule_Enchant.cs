public class GuideModule_Enchant : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterAddEnchant, _OnAfterAddEnchant);
		void _OnAfterAddEnchant(GameEvent.Type type, GameEvent gameEvent)
		{
			Enchant targetEnchant = gameEvent.TargetEnchant;
			Item targetItem = gameEvent.TargetItem;
			if (targetEnchant != null && targetItem != null)
			{
				BattleFieldTransform ownerBft = targetItem.OwnerBft;
				if (ownerBft != null && ownerBft.isPlayer)
				{
					EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_enchant));
					Finish();
				}
			}
		}
	}
}
