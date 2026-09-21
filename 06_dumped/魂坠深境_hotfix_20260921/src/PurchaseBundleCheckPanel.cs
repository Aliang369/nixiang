using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseBundleCheckPanel : CellUI<RewardBundlePurchase>
{
	[SerializeField]
	private GlobalItemDisplay prefab;

	[SerializeField]
	private GameButton bachButton;

	[SerializeField]
	private new TextMeshProUGUI name;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI subDescription;

	[SerializeField]
	private GameButton buyButton;

	[SerializeField]
	private TextMeshProUGUI buyRealMoneyPriceText;

	[SerializeField]
	private MatterCell buyPriceMatterCell;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	protected override void Init()
	{
		InitUI(buyPriceMatterCell);
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(prefab);
		bachButton.onClick.AddListener(((CellUI<RewardBundlePurchase>)this).HideUI<RewardBundlePurchase>);
		buyButton.onClick.AddListener(_OnBuyButtonClicked);
	}

	protected override void OnSetup(RewardBundlePurchase setupData)
	{
		pool.DespawnAll();
		pool.Sync(((IEnumerable<GlobalRewardSetup>)setupData.rewardBundle.rewards).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
		name.text = setupData.rewardBundle.title;
		description.text = setupData.rewardBundle.description;
		subDescription.text = setupData.rewardBundle.subDescription;
		icon.sprite = setupData.rewardBundle.icon;
		buyButton.interactable = setupData.IsAllowPurchase(CellUI<RewardBundlePurchase>.applicationState);
		MatterInfo matterInfo = setupData.CurrentMatterCost(CellUI<RewardBundlePurchase>.applicationState);
		if (matterInfo != null)
		{
			buyPriceMatterCell.gameObject.SetActive(value: true);
			buyPriceMatterCell.Setup(matterInfo);
		}
		else
		{
			buyPriceMatterCell.gameObject.SetActive(value: false);
		}
		if (setupData.realMoneyCost > 0)
		{
			buyRealMoneyPriceText.gameObject.SetActive(value: true);
			if (GlobalConfigManager.instance.enableNetwork)
			{
				buyRealMoneyPriceText.text = CellUI<RewardBundlePurchase>.applicationState.purchaseManager.LocPrice(setupData.purchaseId);
			}
		}
		else
		{
			buyRealMoneyPriceText.gameObject.SetActive(value: false);
		}
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData.CurrentMatterCost(CellUI<RewardBundlePurchase>.applicationState) != null)
		{
			buyButton.interactable = base.setupData.IsEnoughCost(CellUI<RewardBundlePurchase>.applicationState) && base.setupData.IsAllowPurchase(CellUI<RewardBundlePurchase>.applicationState);
		}
		else
		{
			buyButton.interactable = base.setupData.IsAllowPurchase(CellUI<RewardBundlePurchase>.applicationState);
		}
	}

	private void _OnBuyButtonClicked()
	{
		if (base.setupData.CurrentMatterCost(CellUI<RewardBundlePurchase>.applicationState) != null)
		{
			if (base.setupData.IsEnoughCost(CellUI<RewardBundlePurchase>.applicationState) && base.setupData.IsAllowPurchase(CellUI<RewardBundlePurchase>.applicationState))
			{
				CellUI<RewardBundlePurchase>.applicationState.purchaseManager.DoPurchase(base.setupData, 1, delegate(GlobalRewardPackage reward)
				{
					base.gameObject.SetActive(value: false);
					CellUI<RewardBundlePurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
					CellUI<RewardBundlePurchase>.fortressState.uiManager.purchaseUIPanel.fortressGlobalRewardPackageDisplay.Setup(reward);
					CellUI<RewardBundlePurchase>.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
				});
			}
		}
		else if (base.setupData.IsAllowPurchase(CellUI<RewardBundlePurchase>.applicationState))
		{
			CellUI<RewardBundlePurchase>.applicationState.purchaseManager.DoPurchase(base.setupData, 1, delegate(GlobalRewardPackage reward)
			{
				base.gameObject.SetActive(value: false);
				CellUI<RewardBundlePurchase>.fortressState.uiManager.purchaseUIPanel.SetDirty();
				CellUI<RewardBundlePurchase>.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
				CellUI<RewardBundlePurchase>.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
			});
		}
	}
}
