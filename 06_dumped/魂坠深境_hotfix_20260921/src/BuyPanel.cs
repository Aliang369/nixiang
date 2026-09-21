using System;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Game.BattleDisplay;
using TMPro;
using UnityEngine;

public class BuyPanel : UIBase
{
	[SerializeField]
	private Transform itemPanelChoiceArea;

	[SerializeField]
	private ItemDisplayBase itemPrefab;

	[SerializeField]
	private TextMeshProUGUI refreshCostText;

	[SerializeField]
	private TextMeshProUGUI costText;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton refreshItemButton;

	[SerializeField]
	private RectTransform ItemInfoArea;

	[SerializeField]
	private ItemInfoPanel itemInfoPanel;

	[NonSerialized]
	private CellUIPrefabPool<ItemDisplayBase, Item> itemPanelPool;

	[NonSerialized]
	private GoldStoreEventPanel parentPanel;

	[NonSerialized]
	public ItemDisplayBase selected;

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	protected override void Init()
	{
		parentPanel = GetUIInParents<GoldStoreEventPanel>();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		confirmButton.onClick.AddListener(_OnBuyButtonClicked);
		refreshItemButton.onClick.AddListener(_OnRefreshButtonClicked);
		itemPrefab.gameObject.SetActive(value: false);
		itemPanelPool = GetPrefabPool<ItemDisplayBase, Item>(itemPrefab, itemPanelChoiceArea);
		InitUI(itemInfoPanel);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		UpdateItemDisplays();
		if (parentPanel.storeEvent.EnableRefreshBuyItems())
		{
			refreshItemButton.gameObject.SetActive(value: true);
			refreshItemButton.interactable = parentPanel.storeEvent.CanRefreshBuyItems();
			refreshCostText.text = $"{ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText()}  {parentPanel.storeEvent.GetRefreshPrice()}";
		}
		else
		{
			refreshItemButton.gameObject.SetActive(value: false);
		}
		if (selected != null)
		{
			ItemInfoArea.gameObject.SetActive(value: true);
			StoreEvent.PricedItem pricedItem = parentPanel.TryGetPricedItem(selected.item);
			confirmButton.interactable = UIBase.gameState.playerState.HasEnoughCoin(pricedItem.price);
			itemInfoPanel.ShowCommonInfo(new CommonInfoData(pricedItem.item, CommonInfoData.ControlType.Click, null, true));
			string text = ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText() + " " + pricedItem.price.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.coinColor);
			costText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_this_item_need_price_arg0.FormatWith(text);
		}
		else
		{
			ItemInfoArea.gameObject.SetActive(value: false);
			itemInfoPanel.HideAllCommonInfo();
		}
	}

	private void UpdateItemDisplays()
	{
		foreach (StoreEvent.PricedItem priceItem in parentPanel.storeEvent.itemsForSell)
		{
			ItemDisplayBase itemDisplayBase = itemPanelPool.GetActiveElements().Find((ItemDisplayBase cd) => cd.GetModule<StoreBuyStateModule>().priceItem == priceItem);
			if (itemDisplayBase == null)
			{
				itemDisplayBase = itemPanelPool.Spawn(priceItem.item);
				itemDisplayBase.GetComponent<StoreBuyStateModule>().priceItem = priceItem;
			}
			RewardUncollectedTag.AttachTo(itemDisplayBase.GetComponent<StoreBuyStateModule>()?.UncollectedTagPivot ?? itemDisplayBase.transform, RewardUncollectedTagLayout.StoreCard)?.Setup(priceItem.item?.GetDef(), UIBase.applicationState);
		}
		foreach (ItemDisplayBase item in (from s in itemPanelPool.GetActiveElements()
			where s.GetModule<StoreBuyStateModule>().priceItem == null || !parentPanel.storeEvent.itemsForSell.Contains(s.GetModule<StoreBuyStateModule>().priceItem)
			select s).ToList())
		{
			itemPanelPool.Despawn(item);
		}
	}

	public void SelectItem(ItemDisplayBase itemDisplay)
	{
		if (!(selected == itemDisplay))
		{
			selected = itemDisplay;
			SetDirty();
		}
	}

	public void UnselectItem()
	{
		if (!(selected == null))
		{
			selected = null;
			SetDirty();
		}
	}

	private void _OnBuyButtonClicked()
	{
		parentPanel.storeEvent.BuyItem(selected.GetModule<StoreBuyStateModule>().priceItem);
		UIBase.gameState.actionManager.EnqueueAction(delegate
		{
			parentPanel.PlayNPCSpineShotAnimation("#购买#");
		});
		UIBase.gameState.actionManager.EnqueueAction(new WaitAction(parentPanel.storeEvent.Config.buyItemAniDialogWaitTime));
		DialogueSequence buyItemDialogue = ScriptableObjectSingleton<GameDataManager>.instance.storeConfig.buyItemDialogue;
		UIBase.gameState.actionManager.EnqueueAction(new TriggerDialogueAction(buyItemDialogue, parentPanel.buyPanelDialoguePlayer));
		UnselectItem();
		parentPanel.SetDirty();
	}

	private void _OnEmptyClicked()
	{
		UnselectItem();
		SetDirty();
	}

	private void _OnRefreshButtonClicked()
	{
		UnselectItem();
		SecondConfirmInfo secondConfirmInfo = new SecondConfirmInfo();
		string arg = $"{ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText()} {parentPanel.storeEvent.GetRefreshPrice()}";
		secondConfirmInfo.description = string.Format(ScriptableObjectSingleton<GameDataManager>.instance.storeConfig.refreshStorChoiceText.choiceDescription, arg);
		secondConfirmInfo.cancelChoiceText = ScriptableObjectSingleton<TextDataPreset>.instance.term_refresh_cancel;
		secondConfirmInfo.confirmChoiceText = ScriptableObjectSingleton<TextDataPreset>.instance.term_refresh_confirm;
		secondConfirmInfo.confirmCallback = delegate
		{
			parentPanel.RefreshItems();
			parentPanel.SetDirty();
		};
		secondConfirmInfo.cancelCallback = delegate
		{
			parentPanel.SetDirty();
		};
		secondConfirmInfo.confirmInteractable = parentPanel.storeEvent.CanRefreshBuyItems();
		UIBase.gameState.UIManager.commonSecondConfirmPanel.Setup(secondConfirmInfo);
		SetDirty();
	}
}
