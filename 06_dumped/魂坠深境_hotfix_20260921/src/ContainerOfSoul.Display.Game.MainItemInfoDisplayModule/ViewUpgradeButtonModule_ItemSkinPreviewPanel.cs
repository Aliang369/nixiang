using System;
using UnityEngine;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public class ViewUpgradeButtonModule_ItemSkinPreviewPanel : MainItemInfoDisplayBase.DisplayModule
{
	[SerializeField]
	private GameButton viewUpgradeButton;

	[NonSerialized]
	private ItemSkinPreviewPanel itemSkinPreviewPanel;

	public override void Init()
	{
		itemSkinPreviewPanel = parent.GetUIInParents<ItemSkinPreviewPanel>();
		if (itemSkinPreviewPanel == null)
		{
			Debug.LogError("itemSkinPreviewPanel == null");
		}
		viewUpgradeButton.onClick.AddListener(delegate
		{
			itemSkinPreviewPanel.originItemInfo.mainItemInfoDisplay.GetModule<ViewUpgradeButtonModule_ItemSkinPreviewPanel>().ShowUpgrade();
			itemSkinPreviewPanel.newItemInfo.mainItemInfoDisplay.GetModule<ViewUpgradeButtonModule_ItemSkinPreviewPanel>().ShowUpgrade();
		});
	}

	public override void UpdateDisplay()
	{
		viewUpgradeButton.gameObject.SetActive(base.item.Upgradeable);
		if (base.item.Upgradeable)
		{
			viewUpgradeButton.transform.SetAsLastSibling();
		}
	}

	private void ShowUpgrade()
	{
		Item item = base.item.Copy();
		item.Upgrade();
		ItemInfoReferenceModule referenceModule = GetReferenceModule();
		if (referenceModule == null)
		{
			Debug.LogError("[ViewUpgradeButton] 未找到卡牌引用模块", base.mainItemInfoDisplay);
		}
		else
		{
			referenceModule.SetupItemReference(item, base.mainItemInfoDisplay, isUpgrade: true);
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
