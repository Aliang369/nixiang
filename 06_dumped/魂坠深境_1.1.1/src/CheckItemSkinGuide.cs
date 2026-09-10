using System.Linq;

public class CheckItemSkinGuide : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		PathLocation location = CheckLocationType.GetLocation(eve, CheckLocationType.CheckType.GameEventTargetLocation);
		if (location == null)
		{
			return null;
		}
		if (location.type != PathLocation.LocationType.UpgradeRoom)
		{
			return false;
		}
		if (location.variantType == PathLocation.LocationVariantType.HealthCostUpgradeRoom)
		{
			return false;
		}
		return eve.gameState.playerState.DeckContainer.items.Any((Item it) => eve.gameState.applicationState.itemSkinManager.GetAllUnlockedSkinsOfItem(it.def).Any());
	}
}
