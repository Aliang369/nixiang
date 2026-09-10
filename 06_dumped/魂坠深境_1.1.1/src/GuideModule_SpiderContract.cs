public sealed class GuideModule_SpiderContract : GuideModule
{
	public ContractDef spiderContract;

	public override void RunModule()
	{
		applicationState.eventManager.gameEvent.AddListener(GameEvent.Type.AfterExitLocationToMap, OnAfterExitLocationToMap);
		void OnAfterExitLocationToMap(GameEvent.Type _, GameEvent eve)
		{
			if (eve?.TargetLocation != null && eve.TargetLocation == eve.gameState?.dungeonMapManager?.currentDungeonMap?.initLocation && !(eve.gameState.contractState?.selectedContract?.contractDef != spiderContract))
			{
				EnqueueAction(new ShowGuideHintSetAction(applicationState.runningGameState.UIManager.guideUIPanel.triggerHint_spiderContract, delegate
				{
					Finish();
				}));
				applicationState.eventManager.gameEvent.RemoveListener(GameEvent.Type.AfterExitLocationToMap, OnAfterExitLocationToMap);
			}
		}
	}
}
