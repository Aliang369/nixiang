public class GuideModule_ItemBonusSelect : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.showItemBonusSelectionPanelEvent.AddListener(_OnShowPanel);
		void _OnShowPanel(SelectGetItemBonus bonus)
		{
			if (bonus is GenerateCardsBonus)
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_itemBonus));
				applicationState.runningGameState.applicationState.eventManager.showItemBonusSelectionPanelEvent.RemoveListener(_OnShowPanel);
				Finish();
			}
		}
	}
}
