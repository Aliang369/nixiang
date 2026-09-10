using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public abstract class ItemArea : UIBase
{
	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private Transform itemSlotDisplayParent;

	[SerializeField]
	private CommonInfoPanelBase infoPanel;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	protected override void Init()
	{
		itemDisplayPrefab.gameObject.SetActive(value: false);
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemSlotDisplayParent);
		if (infoPanel != null)
		{
			InitUI(infoPanel);
			itemDisplayPool.ConfigInfoPanel(infoPanel);
		}
	}

	protected override void UpdateDisplay()
	{
		UpdateItemSequence();
	}

	public ItemDisplayBase GetItemDisplay(Item item)
	{
		return itemDisplayPool.GetActiveElements().Find((ItemDisplayBase d) => d.item == item);
	}

	public IReadOnlyList<ItemDisplayBase> GetAllItemDisplays()
	{
		return itemDisplayPool.GetActiveElements();
	}

	protected abstract List<Item> GetItems();

	private void UpdateItemSequence()
	{
		List<Item> source = new List<Item>(GetItems());
		foreach (ItemDisplayBase item in (from display in itemDisplayPool.GetActiveElements()
			where !GetItems().Contains(display.item)
			select display).ToList())
		{
			RemoveItemDisplay(item);
		}
		foreach (Item item2 in source.Where((Item i) => GetItemDisplay(i) == null))
		{
			AddItemDisplay(item2);
		}
		foreach (ItemDisplayBase activeElement in itemDisplayPool.GetActiveElements())
		{
			activeElement.transform.SetSiblingIndex(activeElement.item.owner.GetRawIndex(activeElement.item));
		}
	}

	private void AddItemDisplay(Item item)
	{
		itemDisplayPool.Spawn(item).SetDirty();
		SetDirty();
	}

	private void RemoveItemDisplay(ItemDisplayBase display)
	{
		if (itemDisplayPool.GetActiveElements().Contains(display))
		{
			itemDisplayPool.Despawn(display);
			SetDirty();
		}
	}
}
