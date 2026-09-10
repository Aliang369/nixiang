using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class CompanionItemArea : UIBase
{
	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private Transform itemSlotDisplayParent;

	[NonSerialized]
	public CustomLayoutGroup customLayoutGroup;

	[NonSerialized]
	private ItemContainer itemContainer;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemSlotDisplayPool;

	[NonSerialized]
	public List<ItemDisplayBase> slotDisplays;

	public ItemDisplayBase this[Item item] => slotDisplays.Find((ItemDisplayBase display) => display.item == item);

	protected override void Init()
	{
		base.Init();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent += _OnActionFrameFinished;
		itemSlotDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemSlotDisplayParent);
		slotDisplays = new List<ItemDisplayBase>();
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
		UIBase.gameState.actionManager.actionFrameFinishEvent -= _OnActionFrameFinished;
		Unset();
	}

	public void Setup(ItemContainer container)
	{
		itemContainer = container;
		ItemContainer obj = itemContainer;
		obj.itemSequenceChangeEvent = (Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>)Delegate.Combine(obj.itemSequenceChangeEvent, new Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>(_OnItemSequenceChange));
		CustomLayoutGroup obj2 = new CustomLayoutGroup
		{
			baseRect = this.RectTransform()
		};
		ItemContainer obj3 = itemContainer;
		obj2.align = ((obj3 == null || obj3.owner?.isPlayer != true) ? CustomLayoutGroup.Align.Left : CustomLayoutGroup.Align.Right);
		customLayoutGroup = obj2;
		SetDirty();
	}

	public void Unset()
	{
		if (itemContainer != null)
		{
			ItemContainer obj = itemContainer;
			obj.itemSequenceChangeEvent = (Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>)Delegate.Remove(obj.itemSequenceChangeEvent, new Action<ItemContainer.ItemSequenceState, ItemContainer.ItemSequenceState>(_OnItemSequenceChange));
			itemContainer = null;
			customLayoutGroup = null;
		}
	}

	protected override void UpdateDisplay()
	{
		foreach (Item allItem in itemContainer.GetAllItems())
		{
			if (!(this[allItem] != null))
			{
				ItemDisplayBase itemDisplayBase = itemSlotDisplayPool.Spawn(allItem);
				slotDisplays.Add(itemDisplayBase);
				itemDisplayBase.gameObject.SetActive(value: true);
			}
		}
		foreach (ItemDisplayBase item in new List<ItemDisplayBase>(slotDisplays))
		{
			if (!itemContainer.Contains(item.item))
			{
				slotDisplays.Remove(item);
				item.gameObject.SetActive(value: true);
				itemSlotDisplayPool.Despawn(item);
			}
		}
		foreach (ItemDisplayBase slotDisplay in slotDisplays)
		{
			slotDisplay.GetModule<CompanionItemInfoModule>().OnShift();
		}
	}

	public void OnAddItem(Item item, Vector3? fromPosition)
	{
		if (item != null && !(this[item] != null))
		{
			UIBase.gameState.StartCoroutine(() => AddItem(item, fromPosition));
		}
		IEnumerator AddItem(Item data, Vector3? fromPosition2)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.AddItem);
			ItemDisplayBase itemDisplayBase = itemSlotDisplayPool.Spawn(data);
			slotDisplays.Add(itemDisplayBase);
			manager.HideUI(itemDisplayBase);
			yield return itemDisplayBase.GetModule<CompanionItemInfoModule>().PlayTween_Add(fromPosition2);
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.AddItem);
			SetDirty();
		}
	}

	public void OnItemSpell(Item item)
	{
		if (item == null)
		{
			return;
		}
		ItemDisplayBase display = this[item];
		if (!(display == null))
		{
			UIBase.gameState.StartCoroutine(() => _OnItemSpell(display));
		}
		IEnumerator _OnItemSpell(ItemDisplayBase itemDisplayBase)
		{
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			yield return itemDisplayBase.GetModule<CompanionItemInfoModule>().PlayTween_Spell();
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
			SetDirty();
		}
	}

	public void OnConvertItem(Item item)
	{
		if (item != null && !(this[item] == null))
		{
			UIBase.gameState.StartCoroutine(() => ConvertItem(item));
		}
		IEnumerator ConvertItem(Item item2)
		{
			ItemDisplayBase itemDisplayBase = this[item2];
			itemDisplayBase.Unset();
			itemDisplayBase.Setup(item2);
			UIBase.gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			yield return itemDisplayBase.GetModule<CompanionItemInfoModule>().OnConvert();
			UIBase.gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
	}

	public void UnselectDisplay()
	{
		foreach (ItemDisplayBase slotDisplay in slotDisplays)
		{
			slotDisplay.GetModule<CompanionItemInfoModule>().OnUnselect();
		}
	}

	private void _OnEmptyAreaClicked()
	{
		UnselectDisplay();
	}

	private void _OnItemSequenceChange(ItemContainer.ItemSequenceState before, ItemContainer.ItemSequenceState after)
	{
		SetDirty();
	}

	private void _OnActionFrameFinished()
	{
		SetDirty();
	}
}
