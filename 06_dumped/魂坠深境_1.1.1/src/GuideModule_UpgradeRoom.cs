public class GuideModule_UpgradeRoom : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.enterWorldEventEvent.AddListener(_OnEnterWorldEvent);
		void _OnEnterWorldEvent()
		{
			if (!(applicationState.runningGameState?.UIManager?.currentEventPanel as UpgradeRoomEventPanel == null))
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_upgradeRoom));
				applicationState.eventManager.enterWorldEventEvent.RemoveListener(_OnEnterWorldEvent);
				Finish();
			}
		}
	}
}
