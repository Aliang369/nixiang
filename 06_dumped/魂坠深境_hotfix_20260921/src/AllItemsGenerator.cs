using System.Collections.Generic;

public class AllItemsGenerator : ItemDefGenerator
{
	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		return eve.gameState?.applicationState.assetCatalog.itemDefCatalog.ValidAssets;
	}
}
