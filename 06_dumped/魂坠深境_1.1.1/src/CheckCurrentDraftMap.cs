public class CheckCurrentDraftMap : CheckNode
{
	public DraftMap draftMap;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState.dungeonMapManager.currentDungeonMap.sourceDraftMap?.CTAssetId == draftMap?.FullId)
		{
			return true;
		}
		return false;
	}
}
