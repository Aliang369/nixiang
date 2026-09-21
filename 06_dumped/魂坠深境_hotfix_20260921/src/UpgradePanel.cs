using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UpgradePanel : UIBase, ISelectablePanel
{
	public enum UpgradeActionType
	{
		BaseUpgrade,
		SetSkinToUpgraded,
		SetSkinAndUpgrade
	}

	[SerializeField]
	protected ItemDisplayArea itemArea;

	[SerializeField]
	public ItemInfoDisplay_Upgrade itemInfoDisplay;

	[SerializeField]
	protected GameButton backButton;

	[SerializeField]
	protected RectTransform vfxPivot;

	[NonSerialized]
	public Item previewItem;

	[NonSerialized]
	public Item selectedItem;

	[NonSerialized]
	public Item overItem;

	[NonSerialized]
	protected NFXPlayState currentUpgradeNFX;

	[NonSerialized]
	public List<Item> upgradedItems;

	[NonSerialized]
	public ItemSkinDef upgradeTargetSkin;

	[NonSerialized]
	public bool isSkinUpgradeMode;

	public Item toShowItem => selectedItem ?? overItem;

	public UpgradeActionType CurrentUpgradeActionType
	{
		get
		{
			if (isSkinUpgradeMode && toShowItem.upgraded)
			{
				return UpgradeActionType.SetSkinToUpgraded;
			}
			if (isSkinUpgradeMode && !toShowItem.upgraded)
			{
				return UpgradeActionType.SetSkinAndUpgrade;
			}
			return UpgradeActionType.BaseUpgrade;
		}
	}

	public void SetMode(bool isSkinUpgrade)
	{
		if (isSkinUpgrade != isSkinUpgradeMode)
		{
			selectedItem = null;
			upgradeTargetSkin = null;
		}
		isSkinUpgradeMode = isSkinUpgrade;
	}

	public virtual bool IsSelected(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		return selectedItem == item;
	}

	public virtual void Unselect(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selectedItem == item)
		{
			Unselect();
		}
	}

	public virtual void Select(SelectableModuleBase module)
	{
		Item item = (module.parent as ItemDisplayBase).item;
		if (selectedItem != item)
		{
			selectedItem = item;
			upgradeTargetSkin = null;
			SetDirty();
		}
	}

	public virtual bool IsSelectDark(SelectableModuleBase module)
	{
		if (isSkinUpgradeMode)
		{
			Item item = (module.parent as ItemDisplayBase).item;
			if (!UIBase.applicationState.itemSkinManager.GetAllValidSkinsOfItem(item.def).Any((ItemSkinDef sk) => UIBase.applicationState.itemSkinManager.IsItemSkinUnlocked(sk)))
			{
				return true;
			}
		}
		return false;
	}

	protected override void Init()
	{
		base.gameObject.SetActive(value: false);
		InitUI(itemArea);
		InitUI(itemInfoDisplay);
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
	}

	protected override void Dispose()
	{
		UIBase.gameState.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		UpdatePreviewItem();
	}

	public virtual void UpgradeItem()
	{
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		upgradedItems.Add(toShowItem);
		int variableValue = UIBase.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.ItemUpgradeFreeTimes);
		if (variableValue > 0)
		{
			UIBase.gameState.globalVariableStateInfo.SetVariableValue(InGameGlobalVariableStateInfo.ItemUpgradeFreeTimes, variableValue - 1);
		}
		else
		{
			int upgradeCrystalPrice = GetUpgradeCrystalPrice();
			int upgradeHealthPrice = GetUpgradeHealthPrice();
			int setSkinCoinPrice = GetSetSkinCoinPrice();
			if (upgradeCrystalPrice > 0)
			{
				UIBase.gameState.actionManager.EnqueueAction(new ChangeCrystalAction(-upgradeCrystalPrice, ChangeCrystalAction.ChangeReason.Forge));
			}
			if (upgradeHealthPrice > 0)
			{
				UIBase.gameState.actionManager.EnqueueAction(new ChangeBftHealthAction(UIBase.gameState.playerState.bft, -upgradeHealthPrice));
			}
			if (setSkinCoinPrice > 0)
			{
				UIBase.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-setSkinCoinPrice));
			}
		}
		switch (CurrentUpgradeActionType)
		{
		case UpgradeActionType.BaseUpgrade:
			UIBase.gameState.actionManager.EnqueueAction(new UpgradeItemAction(toShowItem, null));
			break;
		case UpgradeActionType.SetSkinToUpgraded:
		case UpgradeActionType.SetSkinAndUpgrade:
			UIBase.gameState.actionManager.EnqueueAction(new SetItemSkinAction(toShowItem, upgradeTargetSkin, upgrade: true));
			ThinkingAnalyticsExtends.TrackSkinUpgrade(UIBase.applicationState, toShowItem.def, upgradeTargetSkin, CurrentUpgradeActionType.ToString());
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		UIBase.gameState.actionManager.EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_upgrade));
		UIBase.gameState.actionManager.EnqueueAction(new ActionAction(SetDirty));
	}

	public virtual bool CanConfirmUpgrade()
	{
		if (toShowItem == null)
		{
			return false;
		}
		if (isSkinUpgradeMode && (upgradeTargetSkin == null || upgradeTargetSkin == toShowItem.skin))
		{
			return false;
		}
		if (!isSkinUpgradeMode && !toShowItem.Upgradeable)
		{
			return false;
		}
		return WithEnoughUpgradeCost();
	}

	public virtual int GetUpgradeCrystalPrice()
	{
		return 0;
	}

	public virtual int GetUpgradeHealthPrice()
	{
		return 0;
	}

	public virtual int GetSetSkinCoinPrice()
	{
		return upgradeTargetSkin?.GetSetSkinCost(UIBase.applicationState) ?? 0;
	}

	public virtual bool WithEnoughUpgradeCost()
	{
		if ((int)UIBase.gameState.playerState.crystalCount >= GetUpgradeCrystalPrice() && (int)UIBase.gameState.playerState.coinCount >= GetSetSkinCoinPrice())
		{
			return (int)UIBase.gameState.playerState.bft.health >= GetUpgradeHealthPrice();
		}
		return false;
	}

	public bool HasUpgradableItem()
	{
		foreach (Item allItem in UIBase.gameState.playerState.DeckContainer.GetAllItems())
		{
			if (allItem.Upgradeable)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void ShowPanel()
	{
		base.gameObject.SetActive(value: true);
	}

	public virtual void HidePanel()
	{
		currentUpgradeNFX?.Stop();
		currentUpgradeNFX = null;
		base.gameObject.SetActive(value: false);
	}

	public virtual void AfterUpgrade()
	{
	}

	public void PlayUpgradeVFX()
	{
		currentUpgradeNFX?.Stop();
		currentUpgradeNFX = UIBase.gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_bigBlock_Upgrade, vfxPivot, vfxPivot.position);
	}

	public bool IsVFXCompleted()
	{
		return currentUpgradeNFX?.IsCompleted() ?? true;
	}

	public void Unselect()
	{
		if (selectedItem != null)
		{
			selectedItem = null;
			upgradeTargetSkin = null;
			SetDirty();
		}
	}

	private void UpdatePreviewItem()
	{
		if (toShowItem == itemInfoDisplay.item && previewItem != null && previewItem.skin == upgradeTargetSkin)
		{
			return;
		}
		Item item = toShowItem;
		if (item != null)
		{
			previewItem = null;
			if (!isSkinUpgradeMode && item.Upgradeable)
			{
				previewItem = item.Copy();
				previewItem.owner = item.owner;
				previewItem.Upgrade(UIBase.gameState);
			}
			else if (isSkinUpgradeMode && upgradeTargetSkin != null)
			{
				previewItem = item.Copy();
				if (previewItem.upgraded)
				{
					previewItem.Downgrade();
				}
				previewItem.owner = item.owner;
				previewItem.SetSkin(upgradeTargetSkin);
			}
			itemInfoDisplay.SetupItems(item, previewItem);
		}
		else
		{
			itemInfoDisplay.Unset();
		}
	}

	private void _OnEmptyAreaClicked()
	{
		Unselect();
	}

	private void _OnBackButtonClicked()
	{
		HidePanel();
	}
}
