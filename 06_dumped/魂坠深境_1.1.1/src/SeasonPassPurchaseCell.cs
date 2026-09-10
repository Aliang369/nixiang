using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SeasonPassPurchaseCell : CellUI<PurchaseInfo>
{
	public GlobalItemDisplay getNowItemDisplayPrefab;

	public GlobalItemDisplay getFromLevelLineDisplayPrefab;

	public RectTransform getNowItemParent;

	public RectTransform getFromLevelLineParent;

	public TextMeshProUGUI seasonPassDescriptionText;

	public TextMeshProUGUI purchaseButtonPriceText;

	public GameButton purchaseButton;

	public Image purchaseBundleIcon;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> getNowItemDisplayPool;

	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> getFromLevelLineDisplayPool;

	protected override void Init()
	{
		purchaseButton.onClick.AddListener(_OnPurchaseButtonClicked);
		getNowItemDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(getNowItemDisplayPrefab, getNowItemParent);
		getFromLevelLineDisplayPool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(getFromLevelLineDisplayPrefab, getFromLevelLineParent);
	}

	protected override void UpdateDisplay()
	{
		if (!(base.setupData is RewardBundlePurchase { rewardBundle: var rewardBundle } rewardBundlePurchase))
		{
			return;
		}
		bool flag = rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockSeasonPassSetup) && !rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockHighSeasonPassSetup);
		bool flag2 = rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockSeasonPassSetup) && rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockHighSeasonPassSetup);
		bool flag3 = !rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockSeasonPassSetup) && rewardBundle.rewards.Exists((GlobalRewardSetup r) => r is UnlockHighSeasonPassSetup);
		purchaseBundleIcon.sprite = rewardBundle.icon;
		if (flag)
		{
			seasonPassDescriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_season_pass_get_extra_reward;
		}
		else
		{
			LevelLine seasonLeveLine = CellUI<PurchaseInfo>.applicationState.seasonManager.currentSeason.seasonLeveLine;
			int num = Mathf.RoundToInt((float)seasonLeveLine.SumGotExp * ((float)ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassInstanceAddExpPercentToGotSumExp / 100f));
			int num2 = ((seasonLeveLine.CurrentLevel != null) ? (num / seasonLeveLine.CurrentLevel.expCost) : 0);
			seasonPassDescriptionText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_high_season_pass_unlock_get_arg0_exp_bonus_arg1_exp_limit_arg2_instant_exp_arg3_level_up.FormatWith(ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassQuestExpBonusPercent, ScriptableObjectSingleton<FortressDataManager>.instance.highSeasonPassWeeklyExpLimitAddPercent, num, num2);
		}
		List<IGlobalItemDisplaySetup> list = rewardBundle.rewards.Where((GlobalRewardSetup r) => !r.isHidden).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList();
		getNowItemDisplayPool.Sync(list);
		List<IGlobalItemDisplaySetup> list2 = (from r in ((IEnumerable<GlobalRewardSetup>)GlobalRewardSetup.CompressRewardsForDisplay(CellUI<PurchaseInfo>.applicationState.seasonManager.currentSeason.seasonLeveLine.GetAllAppendRewards())).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r))
			orderby r.GlobalItemQuality descending
			select r).ToList();
		getFromLevelLineDisplayPool.Sync(list2);
		bool seasonPassUnlocked = CellUI<PurchaseInfo>.applicationState.seasonManager.currentSeason.seasonPassUnlocked;
		bool highseasonPassUnlocked = CellUI<PurchaseInfo>.applicationState.seasonManager.currentSeason.highseasonPassUnlocked;
		if (rewardBundlePurchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState) && ((flag && !seasonPassUnlocked) || (flag2 && !highseasonPassUnlocked && !seasonPassUnlocked) || ((flag3 & seasonPassUnlocked) && !highseasonPassUnlocked)))
		{
			purchaseButton.interactable = true;
			if (GlobalConfigManager.instance.enableNetwork)
			{
				purchaseButtonPriceText.text = CellUI<PurchaseInfo>.applicationState.purchaseManager.LocPrice(rewardBundlePurchase.purchaseId);
			}
		}
		else
		{
			purchaseButton.interactable = false;
			purchaseButtonPriceText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_sold_out;
		}
	}

	private void _OnPurchaseButtonClicked()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_purchase_click);
		CellUI<PurchaseInfo>.applicationState.purchaseManager.DoPurchase(base.setupData, 1, delegate(GlobalRewardPackage reward)
		{
			CellUI<PurchaseInfo>.fortressState.uiManager.purchaseUIPanel.SetDirty();
			CellUI<PurchaseInfo>.fortressState.uiManager.purchaseUIPanel.fortressGlobalRewardPackageDisplay.Setup(reward);
		});
	}
}
