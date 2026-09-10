using System.Collections.Generic;
using UnityEngine;

public class CurrentCharacterCompanionItemDefSelector : ItemDefGenerator
{
	[SerializeField]
	private List<CharacterCompanionOverride> classCompanionOverrides = new List<CharacterCompanionOverride>();

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		PlayerGameStateInfo playerGameStateInfo = eve?.gameState?.playerState;
		if (playerGameStateInfo?.CharacterDef?.attachClass == null)
		{
			return null;
		}
		CharacterClassType characterClass = playerGameStateInfo.CharacterDef.attachClass.mClass;
		ItemDefinition itemDefinition = classCompanionOverrides?.Find((CharacterCompanionOverride entry) => entry != null && entry.characterClass == characterClass)?.itemDef;
		if (itemDefinition != null)
		{
			return new List<ItemDefinition> { itemDefinition };
		}
		return playerGameStateInfo.CharacterDef.attachClass.GetPoolItems(eve.gameState.applicationState).FindAll((ItemDefinition item) => item != null && item.Companion);
	}
}
