public class GuideModule_GoldStore : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.enterWorldEventEvent.AddListener(_OnEnterStore);
		void _OnEnterStore()
		{
			if (!(applicationState.runningGameState?.UIManager?.currentEventPanel as GoldStoreEventPanel == null))
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_goldStore));
				applicationState.runningGameState.applicationState.eventManager.enterWorldEventEvent.RemoveListener(_OnEnterStore);
				Finish();
			}
		}
	}
}
