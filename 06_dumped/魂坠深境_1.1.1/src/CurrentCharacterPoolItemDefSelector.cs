using System.Collections.Generic;

public class CurrentCharacterPoolItemDefSelector : ItemDefGenerator
{
	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		if (eve.gameState?.playerState == null)
		{
			return null;
		}
		return eve.gameState.playerState.CharacterDef.attachClass.GetPoolItems(eve.gameState.applicationState);
	}
}
