public class GuideModule_MintageRune : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.enterWorldEventEvent.AddListener(_OnEnterWorldEvent);
		void _OnEnterWorldEvent()
		{
			if (!(applicationState.runningGameState?.UIManager?.currentEventPanel as MintageRuneRoomEventPanel == null))
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_mintageRune));
				applicationState.eventManager.enterWorldEventEvent.RemoveListener(_OnEnterWorldEvent);
				Finish();
			}
		}
	}
}
