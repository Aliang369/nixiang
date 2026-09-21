public class SetFixedRouteEndpointType : DraftLocationModifier
{
	public PathLocation.LocationType finalStageType = PathLocation.LocationType.Boss;

	public PathLocation.LocationType nextStageType = PathLocation.LocationType.SectionEnd;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		FixedContractRouteDef fixedContractRouteDef = gs.newGameConfigData?.ResolvedFixedContractRoute;
		if (!(fixedContractRouteDef == null))
		{
			location.type = (fixedContractRouteDef.IsCompletedAfterStage(gs.contractState.stage) ? finalStageType : nextStageType);
			location.isNodePoint = true;
			location.attachEncounterSetupInfo = ScriptableObjectSingleton<DungeonGenerationManager>.instance.GetLocationGenerator(location.type)?.GenerateRoomSetupInfo(gs, map, location);
		}
	}
}
