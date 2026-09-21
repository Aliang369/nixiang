public class GuideModule_DungeonMap : GuideModule
{
	public override void RunModule()
	{
		applicationState.eventManager.UIDungeonMapShowedEvent.AddListener(_OnShowMap);
		void _OnShowMap()
		{
			if (!(applicationState.runningGameState?.UIManager?.dungeonMapPanel == null) && applicationState.guideManager.introFinished)
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_map));
				applicationState.eventManager.UIDungeonMapShowedEvent.RemoveListener(_OnShowMap);
				Finish();
			}
		}
	}
}
