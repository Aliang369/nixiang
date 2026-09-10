using System;
using Common.UI;
using TMPro;

public class ItemSkinConvertInspectCell : CellUI<ItemSkinDef>
{
	public ItemDisplayBase originalItemDisplay;

	public ItemDisplayBase skinItemDisplay;

	public TextMeshProUGUI itemSkinSetNameText;

	[NonSerialized]
	private Item originalItem;

	[NonSerialized]
	private Item skinItem;

	protected override void Init()
	{
		InitUI(originalItemDisplay);
		InitUI(skinItemDisplay);
	}

	protected override void OnSetup(ItemSkinDef skinDef)
	{
		originalItem = new Item(skinDef.attachItem);
		originalItemDisplay.Setup(originalItem);
		skinItem = skinDef.GenerateSkinedItem();
		skinItemDisplay.Setup(skinItem);
		if (skinDef.attachSkinSet != null)
		{
			itemSkinSetNameText.text = skinDef.attachSkinSet.SkinSetNameWithColor;
		}
		else
		{
			itemSkinSetNameText.text = string.Empty;
		}
	}

	protected override void OnUnset()
	{
		originalItemDisplay.Unset();
		skinItemDisplay.Unset();
		originalItem = null;
		skinItem = null;
	}

	public void SetInfoPanel(ICommonInfoPanel panel)
	{
		originalItemDisplay.SetInfoPanel(panel);
		skinItemDisplay.SetInfoPanel(panel);
	}
}
