using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GlobalItemBoxInfoDisplay : CellUI<IGlobalItemDisplaySetup>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		public GlobalItemBoxInfoDisplay display => parent as GlobalItemBoxInfoDisplay;

		public IGlobalItemDisplaySetup setup => display.setupData;
	}

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private GameObject storageCountPanel;

	[SerializeField]
	private TextMeshProUGUI storageCount;

	[SerializeField]
	private TextMeshProUGUI matterRewardText;

	[SerializeField]
	private GameButton viewCharacterButton;

	[SerializeField]
	private GameButton viewItemSkinButton;

	[SerializeField]
	private GameButton viewItemSkinSetButton;

	[SerializeField]
	private GameButton viewBuffs;

	[SerializeField]
	private GameButton viewSillls;

	[SerializeField]
	private GameButton viewMatters;

	[SerializeField]
	private GameButton viewGachaAnnouncement;

	[SerializeField]
	private GameButton jumpToStoreButton;

	[SerializeField]
	private GameButton viewRewardBundleButton;

	[SerializeField]
	private Image icon;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	protected override void Init()
	{
		GetModule<RankModule>().fQuality = () => base.setupData.GlobalItemQuality;
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<IGlobalItemDisplaySetup>.applicationState, description, base.rectTransform);
		viewCharacterButton.onClick.AddListener(delegate
		{
			if (base.setupData.GlobalItemRefPlayerCharacter != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				PlayerCharacterState state = base.setupData.GlobalItemRefPlayerCharacter.GetState(CellUI<IGlobalItemDisplaySetup>.applicationState);
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.characterInfoBasePanel.ShowUI(state);
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseBundleCheckPanel.HideUI();
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseConfirmPanel.gameObject.SetActive(value: false);
			}
		});
		viewItemSkinButton.onClick.AddListener(delegate
		{
			if (base.setupData.GlobalItemRefItemSkin != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.itemSkinPreviewPanel.Setup(base.setupData.GlobalItemRefItemSkin);
			}
			CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
			CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseConfirmPanel.gameObject.SetActive(value: false);
		});
		viewItemSkinSetButton.onClick.AddListener(delegate
		{
			if (base.setupData.GlobalItemSkinSetDef != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				SkinSetDisplay.ShowSkinSetPanel(base.setupData.GlobalItemSkinSetDef, CellUI<IGlobalItemDisplaySetup>.fortressState);
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseConfirmPanel.gameObject.SetActive(value: false);
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.gameObject.SetActive(value: false);
			}
		});
		viewBuffs.onClick.AddListener(delegate
		{
			if (base.setupData.RandomStartGameBuffs != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.Setup(base.setupData.RandomStartGameBuffs);
			}
		});
		viewMatters.onClick.AddListener(delegate
		{
			if (base.setupData.RandomMatterDefs != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.Setup(base.setupData.RandomMatterDefs);
			}
			else if (base.setupData is MatterInfo matterInfo && matterInfo.matterDef.matterType == MatterDef.MatterType.Loot)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.Setup(((IEnumerable<GlobalRewardSetup>)matterInfo.matterDef.GetAllLootRewards()).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
			}
			else if (base.setupData is MatterReward matterReward && matterReward.matterDef.matterType == MatterDef.MatterType.Loot)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.Setup(((IEnumerable<GlobalRewardSetup>)matterReward.matterDef.GetAllLootRewards()).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
			}
		});
		viewSillls.onClick.AddListener(delegate
		{
			if (base.setupData.RandomSkillDefs != null && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.commonGlobalItemsPanel.Setup(base.setupData.RandomSkillDefs);
			}
		});
		viewRewardBundleButton.onClick.AddListener(delegate
		{
			if (base.setupData is RewardBundlePurchase rewardBundlePurchase && CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseBundleCheckPanel.ShowUI(rewardBundlePurchase);
			}
		});
		viewGachaAnnouncement.onClick.AddListener(delegate
		{
			if (CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				UIHelper.HideAllInfo(base.transform);
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.gachaPanel.ShowDefaultEntryPool();
				CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.gachaPanel.gachaRUleInfoPanel.SetActive(value: true);
			}
		});
		jumpToStoreButton.onClick.AddListener(delegate
		{
			if (CellUI<IGlobalItemDisplaySetup>.fortressState != null)
			{
				IGlobalItemDisplaySetup globalItemDisplaySetup = base.setupData;
				if (globalItemDisplaySetup != null)
				{
					PurchaseGroupType jumpToStorePurchaseGroupType = globalItemDisplaySetup.JumpToStorePurchaseGroupType;
					int jumpToStoreSubIndex = globalItemDisplaySetup.JumpToStoreSubIndex;
					if ((jumpToStoreSubIndex >= 0) ? CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(jumpToStorePurchaseGroupType, jumpToStoreSubIndex) : CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(jumpToStorePurchaseGroupType))
					{
						CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.universalCommonInfoPanel.HideAll();
					}
				}
			}
		});
	}

	protected override void OnSetup(IGlobalItemDisplaySetup setupData)
	{
		SetActiveAndDirty(active: true);
		viewCharacterButton.gameObject.SetActive(value: false);
		viewItemSkinButton.gameObject.SetActive(value: false);
		viewItemSkinSetButton.gameObject.SetActive(value: false);
		viewBuffs.gameObject.SetActive(value: false);
		viewSillls.gameObject.SetActive(value: false);
		viewMatters.gameObject.SetActive(value: false);
		viewRewardBundleButton.gameObject.SetActive(value: false);
		viewGachaAnnouncement.gameObject.SetActive(value: false);
		jumpToStoreButton.gameObject.SetActive(value: false);
		storageCountPanel.gameObject.SetActive(value: false);
		icon.sprite = setupData.GlobalItemCellIcon;
		TextMeshProUGUI componentInChildren = jumpToStoreButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		titleText.text = setupData.GlobalItemTitle;
		description.text = setupData.GetGlobalItemDescription(CellUI<IGlobalItemDisplaySetup>.applicationState, out var generator);
		textLinkObjectDisplayer.SetGenerator(generator);
		string globalItemStorageCountText = setupData.GetGlobalItemStorageCountText(CellUI<IGlobalItemDisplaySetup>.applicationState);
		if (globalItemStorageCountText != null)
		{
			storageCountPanel.gameObject.SetActive(value: true);
			storageCount.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_have_arg0.FormatWith(globalItemStorageCountText);
		}
		if (CellUI<IGlobalItemDisplaySetup>.fortressState == null)
		{
			return;
		}
		if (setupData.GlobalItemRefPlayerCharacter != null)
		{
			viewCharacterButton.gameObject.SetActive(value: true);
		}
		if (setupData.GlobalItemRefItemSkin != null)
		{
			viewItemSkinButton.gameObject.SetActive(value: true);
		}
		if (setupData.GlobalItemSkinSetDef != null)
		{
			viewItemSkinSetButton.gameObject.SetActive(value: true);
		}
		if (setupData.RandomStartGameBuffs != null)
		{
			viewBuffs.gameObject.SetActive(value: true);
		}
		if (setupData.RandomSkillDefs != null)
		{
			viewSillls.gameObject.SetActive(value: true);
		}
		matterRewardText.text = CellUI<IGlobalItemDisplaySetup>.textData.term_random_matter_list;
		if (setupData.RandomMatterDefs != null)
		{
			viewMatters.gameObject.SetActive(value: true);
		}
		else if (setupData is MatterInfo matterInfo && matterInfo.matterDef.matterType == MatterDef.MatterType.Loot)
		{
			if (matterInfo.matterDef.lootSetup is PickLootSetup)
			{
				matterRewardText.text = CellUI<IGlobalItemDisplaySetup>.textData.term_pick_matter_list;
			}
			viewMatters.gameObject.SetActive(value: true);
		}
		else if (setupData is MatterReward matterReward && matterReward.matterDef.matterType == MatterDef.MatterType.Loot)
		{
			if (matterReward.matterDef.lootSetup is PickLootSetup)
			{
				matterRewardText.text = CellUI<IGlobalItemDisplaySetup>.textData.term_pick_matter_list;
			}
			viewMatters.gameObject.SetActive(value: true);
		}
		if (setupData.JumpToGachaAnnouncement)
		{
			viewGachaAnnouncement.gameObject.SetActive(value: true);
		}
		if (setupData is RewardBundlePurchase)
		{
			viewRewardBundleButton.gameObject.SetActive(value: true);
		}
		if (setupData.JumpToStoreButton && CellUI<IGlobalItemDisplaySetup>.fortressState.uiManager.purchaseUIPanel.CanOpenPurchasePanel)
		{
			jumpToStoreButton.gameObject.SetActive(value: true);
			if (componentInChildren != null)
			{
				componentInChildren.text = setupData.JumpToStoreButtonText;
			}
		}
	}

	protected override void OnUnset()
	{
		SetActiveAndDirty(active: false);
	}

	private CharacterClassDef TryGetUICharacterClassDefContext()
	{
		IdolPanel componentInParent = GetComponentInParent<IdolPanel>(includeInactive: true);
		if (componentInParent != null)
		{
			return componentInParent.setupData.classDef;
		}
		ForgetShrinePanel componentInParent2 = GetComponentInParent<ForgetShrinePanel>(includeInactive: true);
		if (componentInParent2 != null)
		{
			return componentInParent2.setupData.classDef;
		}
		return null;
	}
}
