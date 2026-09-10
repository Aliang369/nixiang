using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class SellPanel : UIBase, ISelectablePanel
{
	[SerializeField]
	private ItemDisplayBase itemDisplayPrefab;

	[SerializeField]
	private Transform itemSlotDisplayParent;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private RectTransform itemInfoDisplayArea;

	[SerializeField]
	private RectTransform exileVFXPivot;

	[SerializeField]
	private ItemInfoPanel itemInfoPanel;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemDisplayPool;

	private GoldStoreEventPanel parentPanel;

	[NonSerialized]
	protected StoreEvent.PricedItem selected;

	[NonSerialized]
	private Item selectedItem;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	private IReadOnlyList<ItemDisplayBase> itemDisplays => itemDisplayPool.GetActiveElements();

	private int recycleItemCount => parentPanel.storeEvent.soldItems.FindAll((StoreEvent.PricedItem s) => !s.IsTreasure && !s.free).Count;

	private ItemDisplayBase selectedItemDisplay => itemDisplayPool.GetActiveElement(selectedItem);

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectedItem == item;
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selectedItem == item)
		{
			Unselect();
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selectedItem != item)
		{
			selectedItem = item;
			selected = parentPanel.storeEvent.GetPriceItem(item);
			SetDirty();
		}
	}

	protected override void Init()
	{
		InitUI(itemInfoPanel);
		parentPanel = GetUIInParents<GoldStoreEventPanel>();
		base.gameObject.SetActive(value: false);
		itemDisplayPool = GetPrefabPool<ItemDisplayBase, Item>(itemDisplayPrefab, itemSlotDisplayParent);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		confirmButton.onClick.AddListener(_OnSellButtonClicked);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	public void Setup()
	{
		Unselect();
		SetActiveAndDirty(active: true);
	}

	public void Unset()
	{
		Unselect();
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		UpdateItemSequence();
		if (selected != null)
		{
			itemInfoDisplayArea.gameObject.SetActive(value: true);
			itemInfoPanel.ShowCommonInfo(new CommonInfoData(selected.item, CommonInfoData.ControlType.Click, null, false));
			int price = selected.price;
			bool interactable = UIBase.gameState.playerState.HasEnoughCoin(price);
			confirmButton.interactable = interactable;
			if (price >= 0)
			{
				string text = ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText() + " " + Mathf.Abs(price).ToString().ColorText(Color.yellow);
				costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remove_item_with_arg0_coin_cost.FormatWith(text);
				confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_destroy_item;
			}
			else
			{
				string text2 = ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText() + " " + Mathf.Abs(price).ToString().ColorText(Color.yellow);
				costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_remove_item_with_arg0_coin_gain.FormatWith(text2);
				confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_sell_item;
			}
		}
		else
		{
			itemInfoDisplayArea.gameObject.SetActive(value: false);
			itemInfoPanel.HideAllCommonInfo();
		}
	}

	public virtual void SellItem()
	{
		UIBase.gameState.StartCoroutine(SellItemCor);
	}

	private IEnumerator SellItemCor()
	{
		StoreEvent.PricedItem selectedItem = selected;
		ItemDisplayBase selectedItemDisplay = this.selectedItemDisplay;
		if (selectedItem != null && !(selectedItemDisplay == null))
		{
			UIHelper.DisableAllButton();
			parentPanel.PlayNPCSpineShotAnimation("#删卡#");
			yield return new WaitForSeconds(parentPanel.storeEvent.Config.sellItemAniWaitTime);
			StoreEvent.StoreConfig storeConfig = ScriptableObjectSingleton<GameDataManager>.instance.storeConfig;
			if (selectedItem.IsTreasure || selectedItem.free)
			{
				UIBase.gameState.actionManager.EnqueueActionToEnd(new TriggerDialogueAction(storeConfig.sellItemTreasureDialogue, parentPanel.buyPanelDialoguePlayer));
			}
			else
			{
				UIBase.gameState.actionManager.EnqueueActionToEnd(new TriggerDialogueAction(storeConfig.sellItemNoTreasureDialogue, parentPanel.buyPanelDialoguePlayer));
			}
			UIBase.gameState.actionManager.EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_exile, selectedItemDisplay.transform.GetWorldPosition(), selectedItemDisplay.transform.GetVFXRoot()));
			parentPanel.storeEvent.SellPlayerDeckItem(selectedItem);
			itemInfoPanel.HideAllCommonInfo();
			parentPanel.SetDirty();
			NFXPlayState nfxState = UIBase.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_bigBlock_Exile, exileVFXPivot, exileVFXPivot.position);
			yield return new WaitUntil(() => nfxState.IsCompleted());
			Unselect();
			UIHelper.EnableAllButton();
		}
	}

	public void Unselect()
	{
		if (selectedItem != null)
		{
			selectedItem = null;
			selected = null;
			SetDirty();
		}
	}

	private void UpdateItemSequence()
	{
		List<Item> items = GetPool();
		foreach (ItemDisplayBase item in itemDisplays.Where((ItemDisplayBase display) => !items.Contains(display.item)).ToList())
		{
			RemoveItemDisplay(item);
		}
		foreach (Item item2 in items.Where((Item i) => GetItemDisplay(i) == null))
		{
			AddItemDisplay(item2);
		}
		foreach (ItemDisplayBase itemDisplay in itemDisplays)
		{
			itemDisplay.transform.SetSiblingIndex(itemDisplay.item.owner.GetRawIndex(itemDisplay.item));
		}
	}

	private void AddItemDisplay(Item item)
	{
		itemDisplayPool.Spawn(item).SetDirty();
		SetDirty();
	}

	private void RemoveItemDisplay(ItemDisplayBase display)
	{
		if (itemDisplays.Contains(display))
		{
			itemDisplayPool.Despawn(display);
			SetDirty();
		}
	}

	private ItemDisplayBase GetItemDisplay(Item item)
	{
		return itemDisplays.Find((ItemDisplayBase d) => d.item == item);
	}

	private List<Item> GetPool()
	{
		return UIBase.gameState.playerState.DeckContainer.GetAllItems().FindAll(parentPanel.storeEvent.CanSellItem);
	}

	private void _OnEmptyClicked()
	{
		Unselect();
	}

	private void _OnSellButtonClicked()
	{
		SellItem();
	}
}
