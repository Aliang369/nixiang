public class EnterLocationImplAction : ExecuteAction
{
	private readonly PathLocation targetLocation;

	private bool saveOnEnter;

	public EnterLocationImplAction(PathLocation targetLocation, bool saveOnEnter = true)
	{
		this.targetLocation = targetLocation;
		this.saveOnEnter = saveOnEnter;
	}

	protected override void Update()
	{
		DungeonMap map = gameState.dungeonMapManager.currentDungeonMap;
		gameState.dungeonMapManager.currentGlobalProgress = targetLocation.globalProgress;
		map.currentLocation = targetLocation;
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ForceAmbushFight) && IsBattleLocation(targetLocation.type))
		{
			targetLocation.variantType = PathLocation.LocationVariantType.AmbushFight;
		}
		targetLocation.visited = true;
		if (saveOnEnter)
		{
			EnqueueAction(new SaveRunningGameAction());
		}
		int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCoinPerLocationUntilSpend);
		if (variableValue > 0)
		{
			EnqueueAction(new ChangeCoinAction(variableValue));
		}
		int variableValue2 = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.BonusCrystalPerLocationUntilSpend);
		if (variableValue2 > 0)
		{
			EnqueueAction(new ChangeCrystalAction(variableValue2));
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerBeforeEnterLocation(map.currentLocation);
		});
		EnqueueAction(new SetupEncounterAction(map.currentLocation.attachEncounterSetupInfo));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerEnterLocation(map.currentLocation);
		});
		gameState.recordManager.TrackJournalRecord(new EnterLocationRecord(targetLocation));
		ThinkingAnalyticsExtends.TrackEnterLocation(gameState, targetLocation);
	}

	private static bool IsBattleLocation(PathLocation.LocationType locationType)
	{
		if (locationType != PathLocation.LocationType.NormalFight && locationType != PathLocation.LocationType.EliteFight)
		{
			return locationType == PathLocation.LocationType.Boss;
		}
		return true;
	}
}
