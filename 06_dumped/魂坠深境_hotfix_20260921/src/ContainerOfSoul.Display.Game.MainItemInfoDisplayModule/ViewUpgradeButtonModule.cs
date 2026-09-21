using System;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class ViewUpgradeButtonModule : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private GameButton viewUpgradeButtonPrefab;

	[SerializeField]
	private RectTransform viewUpgradeButtonParent;

	[SerializeField]
	private Color baseUpgradeTextColor;

	[NonSerialized]
	private PrefabPool<GameButton> viewUpgradeButtonPool;

	public override void Init()
	{
		viewUpgradeButtonPool = new PrefabPool<GameButton>(viewUpgradeButtonPrefab, viewUpgradeButtonParent);
	}

	public override void UpdateDisplay()
	{
		if (base.mainItemInfoDisplay.viewUpgradeButtonPreviewSetup?.options != null)
		{
			SetupPreviewButtons(base.mainItemInfoDisplay.viewUpgradeButtonPreviewSetup.options);
			return;
		}
		if (base.item.Upgradeable)
		{
			viewUpgradeButtonParent.gameObject.SetActive(value: true);
			viewUpgradeButtonParent.SetAsLastSibling();
			List<ItemSkinDef> list = new List<ItemSkinDef>();
			list.Add(null);
			foreach (ItemSkinDef item in UIDisplayModule.applicationState.itemSkinManager.GetAllIndexVisibleUnlockedSkinsOfItem(base.item.def))
			{
				list.Add(item);
			}
			viewUpgradeButtonPool.DespawnAll();
			{
				foreach (ItemSkinDef skin in list)
				{
					GameButton gameButton = viewUpgradeButtonPool.Spawn();
					gameButton.onClick.RemoveAllListeners();
					gameButton.onClick.AddListener(delegate
					{
						ShowUpgradePreview(skin);
					});
					TextMeshProUGUI componentInChildren = gameButton.GetComponentInChildren<TextMeshProUGUI>();
					if (skin != null)
					{
						componentInChildren.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_cardskin_arg0_preview.FormatWith(skin.attachSkinSet.skinSetName);
						componentInChildren.color = skin.attachSkinSet.skinSetMainColor;
					}
					else
					{
						componentInChildren.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_preview;
						componentInChildren.color = baseUpgradeTextColor;
					}
				}
				return;
			}
		}
		viewUpgradeButtonParent.gameObject.SetActive(value: false);
	}

	private void SetupPreviewButtons(List<ViewUpgradeButtonPreviewOption> options)
	{
		if (options == null || options.Count == 0)
		{
			viewUpgradeButtonParent.gameObject.SetActive(value: false);
			viewUpgradeButtonPool.DespawnAll();
			return;
		}
		viewUpgradeButtonParent.gameObject.SetActive(value: true);
		viewUpgradeButtonParent.SetAsLastSibling();
		viewUpgradeButtonPool.DespawnAll();
		foreach (ViewUpgradeButtonPreviewOption option in options)
		{
			GameButton gameButton = viewUpgradeButtonPool.Spawn();
			gameButton.onClick.RemoveAllListeners();
			gameButton.onClick.AddListener(delegate
			{
				ShowPreview(option.previewItem, option.showAsUpgrade);
			});
			TextMeshProUGUI componentInChildren = gameButton.GetComponentInChildren<TextMeshProUGUI>();
			componentInChildren.text = option.text;
			componentInChildren.color = option.color ?? baseUpgradeTextColor;
		}
	}

	private void ShowUpgradePreview(ItemSkinDef skin)
	{
		Item item = base.item.Copy();
		if (skin == null)
		{
			item.Upgrade();
		}
		else
		{
			item.SetSkin(skin);
		}
		ShowPreview(item, isUpgrade: true);
	}

	private void ShowPreview(Item previewItem, bool isUpgrade)
	{
		if (previewItem != null)
		{
			ItemInfoReferenceModule referenceModule = GetReferenceModule();
			if (referenceModule == null)
			{
				Debug.LogError("[ViewUpgradeButton] 未找到卡牌引用模块", base.mainItemInfoDisplay);
			}
			else
			{
				referenceModule.SetupItemReference(previewItem, base.mainItemInfoDisplay, isUpgrade);
			}
		}
	}

	private ItemInfoReferenceModule GetReferenceModule()
	{
		ItemInfoDisplayBase itemInfoDisplayBase = base.mainItemInfoDisplay?.itemInfoDisplay ?? base.mainItemInfoDisplay?.GetComponentInParent<ItemInfoDisplayBase>(includeInactive: true);
		object obj = itemInfoDisplayBase?.GetModule<ItemInfoReferenceModule>();
		if (obj == null)
		{
			obj = itemInfoDisplayBase?.GetComponent<ItemInfoReferenceModule>();
			if (obj == null)
			{
				MainItemInfoDisplayBase mainItemInfoDisplayBase = base.mainItemInfoDisplay;
				if ((object)mainItemInfoDisplayBase == null)
				{
					return null;
				}
				obj = mainItemInfoDisplayBase.GetComponentInParent<ItemInfoReferenceModule>(includeInactive: true);
			}
		}
		return (ItemInfoReferenceModule)obj;
	}
}
