using UnityEngine;

public class RemoveFromDeckRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string itemId;

	public override string NetworkSaveArg0
	{
		get
		{
			return itemId;
		}
		set
		{
			itemId = value;
		}
	}

	private RemoveFromDeckRecord()
	{
	}

	public RemoveFromDeckRecord(Item item)
	{
		itemId = item.def.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		ItemDefinition itemDefinition = applicationState.assetCatalog.FullIdToCTAsset<ItemDefinition>(itemId);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_remove_item_from_deck_arg0.FormatWith(itemDefinition.itemName.ColorText(Color.yellow));
	}
}
