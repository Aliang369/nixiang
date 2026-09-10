using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class ItemSkinSelectionModule : ItemInfoDisplayBase.DisplayModule
{
	public Transform skinSelectionButtonParent;

	public ItemSkinSelectButton skinSelectionButtonPrefab;

	private CellUIPrefabPool<ItemSkinSelectButton, (Item, ItemSkinDef)> skinSelectionButtonPool;

	public override void Init()
	{
		skinSelectionButtonPool = base.itemInfoDisplay.GetPrefabPool<ItemSkinSelectButton, (Item, ItemSkinDef)>(skinSelectionButtonPrefab, skinSelectionButtonParent);
	}

	public override void UpdateDisplay()
	{
		if (base.itemInfoDisplay.item == null)
		{
			return;
		}
		skinSelectionButtonPool.DespawnAll();
		List<ItemSkinDef> allIndexVisibleSkinsOfItem = UIDisplayModule.applicationState.itemSkinManager.GetAllIndexVisibleSkinsOfItem(base.itemInfoDisplay.item.def);
		if (allIndexVisibleSkinsOfItem.Count <= 0)
		{
			return;
		}
		skinSelectionButtonPool.Spawn((base.itemInfoDisplay.item, null));
		foreach (ItemSkinDef item in allIndexVisibleSkinsOfItem)
		{
			skinSelectionButtonPool.Spawn((base.itemInfoDisplay.item, item));
		}
	}
}
