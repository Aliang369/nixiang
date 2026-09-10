using System.Collections.Generic;

public class AllCommonLootGenerator : ItemDefGenerator
{
	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		return eve.gameState?.generationManager?.GetAllGenerateAbleLoots();
	}
}
