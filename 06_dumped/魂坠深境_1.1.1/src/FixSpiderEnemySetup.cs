public sealed class FixSpiderEnemySetup : FixEnemy
{
	public int setupIndex;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		base.ModifyLocation(location, pivot, map, gs);
		EnemyInfoData fixedEnemyInfoData = location.attachEncounterSetupInfo.fixedEnemyInfoData;
		if (fixedEnemyInfoData?.setups != null && setupIndex >= 0 && setupIndex < fixedEnemyInfoData.setups.Count)
		{
			location.attachEncounterSetupInfo.fixedEnemySetupIndex = setupIndex;
			location.attachEncounterSetupInfo.fixedEnemySetup = fixedEnemyInfoData.setups[setupIndex];
		}
	}
}
