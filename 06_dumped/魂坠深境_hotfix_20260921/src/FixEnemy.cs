public class FixEnemy : DraftLocationModifier
{
	public EnemyAssetRef enemyRef;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		location.attachEncounterSetupInfo.fixedEnemyInfoData = enemyRef.GetAssetPreData(gs.applicationState.assetCatalog);
	}
}
