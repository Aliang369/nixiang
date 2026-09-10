using UnityEngine;

public class AddItemToDeckRecord : GameJournalRecord
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

	private AddItemToDeckRecord()
	{
	}

	public AddItemToDeckRecord(Item item)
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
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_add_item_to_deck_arg0.FormatWith(itemDefinition.itemName.ColorText(Color.yellow));
	}
}
