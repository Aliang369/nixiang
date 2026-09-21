using UnityEngine;

public class MintageRuneRecord : GameJournalRecord
{
	[SaveField(null, null)]
	public string itemId;

	[SaveField(null, null)]
	public string runeId;

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

	public override string NetworkSaveArg1
	{
		get
		{
			return runeId;
		}
		set
		{
			runeId = value;
		}
	}

	private MintageRuneRecord()
	{
	}

	public MintageRuneRecord(Item item, Rune rune)
	{
		itemId = item.def.FullID;
		runeId = rune.def.FullID;
	}

	public override bool IsVisibleToPlayer(ApplicationStateInfo applicationState)
	{
		return true;
	}

	public override string GenerateDescription(ApplicationStateInfo applicationState)
	{
		RuneDef runeDef = applicationState.assetCatalog.FullIdToCTAsset<RuneDef>(runeId);
		ItemDefinition itemDefinition = applicationState.assetCatalog.FullIdToCTAsset<ItemDefinition>(itemId);
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_mintage_rune_arg0_to_item_arg1.FormatWith(runeDef.Name.ColorText(Color.yellow), itemDefinition.itemName.ColorText(Color.yellow));
	}
}
