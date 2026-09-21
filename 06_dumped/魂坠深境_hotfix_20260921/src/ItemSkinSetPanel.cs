using Common.UI;
using TMPro;
using UnityEngine.UI;

public class ItemSkinSetPanel : CellUI<ItemSkinSetDef>, IDisplayModulesController
{
	public TextMeshProUGUI nameText;

	public TextMeshProUGUI descriptionText;

	public TextMeshProUGUI gatherText;

	public SkinSetBatchDisplay batchPrefab;

	public CellUIPrefabPool<SkinSetBatchDisplay, ItemSkinSetDef.SkinSetBatchConfig> batchPool;

	public ItemInfoPanel itemInfoPanel;

	public GameButton helpButton;

	public GameButton backButton;

	public GameButton applyAllButton;

	public TextMeshProUGUI applyAllButtonText;

	public GameButton jumpToStoreButton;

	public ScrollRect scrollRect;

	protected override void Init()
	{
		GetModule<RankModule>().fQuality = () => base.setupData.quality;
		InitUI(itemInfoPanel);
		batchPool = GetPrefabPool<SkinSetBatchDisplay, ItemSkinSetDef.SkinSetBatchConfig>(batchPrefab);
		backButton.onClick.AddListener(delegate
		{
			Unset();
			manager.HideUI(this);
		});
		helpButton.onClick.AddListener(delegate
		{
			CellUI<ItemSkinSetDef>.fortressState.uiManager.fortressGuidePanel.itemSkinGuideHint.Show();
		});
		applyAllButton.onClick.AddListener(SetAll);
		jumpToStoreButton.onClick.AddListener(_OnJumpToStoreClicked);
		static void SetAll()
		{
		}
	}

	protected override void OnSetup(ItemSkinSetDef setDef)
	{
		SetActiveAndDirtyWithFadeAnim(active: true);
		nameText.text = base.setupData.skinSetName;
		descriptionText.text = base.setupData.description;
		gatherText.text = base.setupData.GetGatherString();
		batchPool.Sync(base.setupData.GetIndexBatchConfigs());
		applyAllButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_apply_skin_in_skinset_arg0.FormatWith(setDef.skinSetName);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<ItemSkinSetDef>.applicationState, "档案馆-卡牌形态详情");
		scrollRect.verticalNormalizedPosition = 1f;
		TryShowItemSkinGuideHint();
	}

	protected override void OnUnset()
	{
		itemInfoPanel.HideAllCommonInfo();
	}

	private void TryShowItemSkinGuideHint()
	{
		if (CellUI<ItemSkinSetDef>.applicationState.itemSkinManager.unlockedItemSkinLevels.Count != 0)
		{
			GuideHintPanelSet itemSkinGuideHint = CellUI<ItemSkinSetDef>.fortressState.uiManager.fortressGuidePanel.itemSkinGuideHint;
			if (!CellUI<ItemSkinSetDef>.applicationState.guideManager.autoPopupedHints.Contains(itemSkinGuideHint.gameObject.name))
			{
				itemSkinGuideHint.Show();
				CellUI<ItemSkinSetDef>.applicationState.guideManager.autoPopupedHints.Add(itemSkinGuideHint.gameObject.name);
			}
		}
	}

	private void _OnJumpToStoreClicked()
	{
		if (CellUI<ItemSkinSetDef>.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(PurchaseGroupType.RealMoneyBuy))
		{
			CellUI<ItemSkinSetDef>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
			base.gameObject.SetActive(value: false);
		}
	}
}
