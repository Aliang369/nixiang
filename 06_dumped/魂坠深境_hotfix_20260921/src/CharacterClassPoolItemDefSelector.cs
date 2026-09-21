using System.Collections.Generic;

public class CharacterClassPoolItemDefSelector : ItemDefGenerator
{
	public CharacterClassDef characterClassDef;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		if (characterClassDef == null || eve.gameState?.applicationState?.assetCatalog == null)
		{
			return null;
		}
		return characterClassDef.GetPoolItems(eve.gameState.applicationState);
	}
}
