public class EnterGameInitLocationAction : ExecuteAction
{
	protected override void Update()
	{
		EnqueueAction(new EnterLocationImplAction(gameState.dungeonMapManager.currentDungeonMap.initLocation));
		EnqueueAction(new EnterRoomTransitAnimAction());
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAfterEnterLocationScreenFade(gameState.dungeonMapManager.currentDungeonMap.initLocation);
		}));
	}
}
