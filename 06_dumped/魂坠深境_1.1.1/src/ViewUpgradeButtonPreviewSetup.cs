using System.Collections.Generic;

public class ViewUpgradeButtonPreviewSetup
{
	public List<ViewUpgradeButtonPreviewOption> options;

	public static ViewUpgradeButtonPreviewSetup CreateItemSkinPreviewSetup(ItemSkinDef itemSkin, ApplicationStateInfo applicationState)
	{
		if (itemSkin?.attachItem == null || applicationState?.itemSkinManager == null)
		{
			return null;
		}
		ViewUpgradeButtonPreviewSetup viewUpgradeButtonPreviewSetup = new ViewUpgradeButtonPreviewSetup
		{
			options = new List<ViewUpgradeButtonPreviewOption>()
		};
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		string[] array = instance.term_upgrade_preview.Split('\n');
		string text = ((array.Length > 1) ? array[1] : instance.term_upgrade_preview);
		viewUpgradeButtonPreviewSetup.options.Add(new ViewUpgradeButtonPreviewOption
		{
			text = instance.term_default_value + "\n" + text,
			previewItem = new Item(itemSkin.attachItem)
		});
		Item item = new Item(itemSkin.attachItem);
		item.Upgrade();
		viewUpgradeButtonPreviewSetup.options.Add(new ViewUpgradeButtonPreviewOption
		{
			text = instance.term_upgrade_preview,
			previewItem = item
		});
		foreach (ItemSkinDef item2 in applicationState.itemSkinManager.GetAllIndexVisibleSkinsOfItem(itemSkin.attachItem))
		{
			if (!(item2 == itemSkin))
			{
				viewUpgradeButtonPreviewSetup.options.Add(new ViewUpgradeButtonPreviewOption
				{
					text = instance.term_upgrade_cardskin_arg0_preview.FormatWith(item2.attachSkinSet.skinSetName),
					color = item2.attachSkinSet.skinSetMainColor,
					previewItem = item2.GenerateSkinedItem()
				});
			}
		}
		return viewUpgradeButtonPreviewSetup;
	}
}
