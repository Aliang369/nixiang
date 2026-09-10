using System.Collections.Generic;
using UnityEngine;

public class UnlockCharacterPurchase : PurchaseInfo
{
	public PlayerCharacterDef character;

	public override string GlobalItemCellName => character.characterName;

	public override Sprite GlobalItemCellIcon => character.globalItemIcon;

	public override string GlobalItemTitle => ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession_of_character_arg0.FormatWith(character.characterName);

	public override PlayerCharacterDef GlobalItemRefPlayerCharacter => character;

	public override string PurchaseTitleText => character.characterName;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession_of_character_arg0_use_it_to_game_character_description_arg1.FormatWith(character.characterName, character.description);
	}

	private UnlockCharacterPurchase()
	{
	}

	public UnlockCharacterPurchase(PlayerCharacterDef character, MatterInfo cost)
	{
		this.character = character;
		mattercost = cost;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			matter = new MatterInfo(character.obsession, 1)
		};
		return new List<GlobalRewardInfo> { item };
	}
}
