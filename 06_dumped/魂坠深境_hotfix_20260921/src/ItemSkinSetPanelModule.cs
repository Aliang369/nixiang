using System;
using TMPro;
using UnityEngine;

public class ItemSkinSetPanelModule : ItemInfoDisplayBase.DisplayModule
{
	public GameObject skinSetBatchInfoPanel;

	public TextMeshProUGUI skinSetCostText;

	public TextMeshProUGUI skinSetNeedCollectHintText;

	public GameObject skinGetSourcePanel;

	public TextMeshProUGUI getSourceTypeText;

	public GameButton inspectOriginalItemButton;

	[NonSerialized]
	private ItemSkinSetPanel itemSkinSetPanel;

	public override void Init()
	{
		itemSkinSetPanel = GetComponentInParent<ItemSkinSetPanel>(includeInactive: true);
		inspectOriginalItemButton.onClick.AddListener(delegate
		{
			base.itemInfoDisplay.Setup(new Item(base.item.def));
		});
	}

	public override void UpdateDisplay()
	{
		inspectOriginalItemButton.gameObject.SetActive(base.item?.skin != null);
		SetupPreviewLinks();
		if (base.item == null || base.item.skin == null)
		{
			skinSetBatchInfoPanel.gameObject.SetActive(value: false);
			skinGetSourcePanel.gameObject.SetActive(value: false);
			return;
		}
		if (UIDisplayModule.applicationState.itemSkinManager.IsItemSkinUnlocked(base.item.skin))
		{
			skinGetSourcePanel.gameObject.SetActive(value: false);
			ItemSkinSetDef.SkinSetBatchConfig setBatchConfig = base.item.skin.SetBatchConfig;
			int? num = setBatchConfig?.GetCurrentSetSkinCoinCost(UIDisplayModule.applicationState);
			if (num.HasValue && num > 0)
			{
				int? needCollectCountForNextLayer = setBatchConfig.GetNeedCollectCountForNextLayer(UIDisplayModule.applicationState);
				skinSetBatchInfoPanel.gameObject.SetActive(value: true);
				skinSetCostText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_upgrade_skin_wtih_extra_cost_count_arg0_icon_arg1.FormatWith(num, ScriptableObjectSingleton<GameDataManager>.instance.coinTextIcon.ToTMPSpriteText());
				if (!needCollectCountForNextLayer.HasValue)
				{
					skinSetNeedCollectHintText.gameObject.SetActive(value: false);
					return;
				}
				skinSetNeedCollectHintText.gameObject.SetActive(value: true);
				skinSetNeedCollectHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_need_collect_count_arg0_other_batch_skin_to_reduce_cost.FormatWith(needCollectCountForNextLayer.Value);
			}
			else
			{
				skinSetBatchInfoPanel.gameObject.SetActive(value: false);
			}
			return;
		}
		skinSetBatchInfoPanel.gameObject.SetActive(value: false);
		skinGetSourcePanel.gameObject.SetActive(value: false);
		switch (base.item.skin.getSourceType)
		{
		case ItemSkinDef.ItemSkinGetSourceType.None:
			getSourceTypeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_from_source_type_unknown;
			break;
		case ItemSkinDef.ItemSkinGetSourceType.SeasonLevelLine:
			getSourceTypeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_from_source_type_season_levelLine;
			break;
		case ItemSkinDef.ItemSkinGetSourceType.Challenge:
			getSourceTypeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_from_source_type_star_challenge;
			break;
		case ItemSkinDef.ItemSkinGetSourceType.Purchase:
			getSourceTypeText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_get_from_source_type_purchase;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void SetupPreviewLinks()
	{
		if (base.itemInfoDisplay is ItemInfoDisplay_Normal itemInfoDisplay_Normal)
		{
			itemInfoDisplay_Normal.mainItemInfoDisplay.viewUpgradeButtonPreviewSetup = ViewUpgradeButtonPreviewSetup.CreateItemSkinPreviewSetup(base.item?.skin, UIDisplayModule.applicationState);
		}
	}
}
