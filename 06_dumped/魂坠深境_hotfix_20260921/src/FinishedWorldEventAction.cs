public class FinishedWorldEventAction : ExecuteAction
{
	public bool exitRoom;

	public WorldEvent worldEvent;

	public WorldEventChoice choice;

	public FinishedWorldEventAction(WorldEvent worldEvent, bool exitRoom = true)
	{
		this.worldEvent = worldEvent;
		this.exitRoom = exitRoom;
		choice = worldEvent.lastSelectedChoice;
	}

	protected override void Update()
	{
		ThinkingAnalyticsExtends.TrackWorldEvent(gameState, worldEvent);
		gameState.gameEventManager.TriggerFinishWorldEvent(worldEvent, choice);
		if (exitRoom && gameState.currentEncounter != null && worldEvent == gameState.currentEncounter.triggerEvent)
		{
			BattleBonus roomBonus = gameState.currentEncounter.roomBonus;
			if (roomBonus != null)
			{
				roomBonus.title = ScriptableObjectSingleton<TextDataPreset>.instance.term_bonus;
				EnqueueAction(new TriggerBonusAction(roomBonus));
			}
			else
			{
				EnqueueAction(new WaitAction(ScriptableObjectSingleton<VFXDataPreset>.instance.normalEventPanelAnimConfig.leaveWaitTime / base.actionTimeScale));
				EnqueueAction(new EndCurrentEncounterAction(finish: true));
			}
		}
		worldEvent.finished = true;
	}
}
