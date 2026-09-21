using System.Collections.Generic;

public class AllCurseGenerator : ItemDefGenerator
{
	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		return eve.gameState?.applicationState.assetCatalog.itemDefCatalog.ValidAssets.FindAll((ItemDefinition t) => t.HasTag(ScriptableObjectSingleton<GameDataManager>.instance.curseTag));
	}
}
