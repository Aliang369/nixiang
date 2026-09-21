using System;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress.DotManagers;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class PurchaseCell : CellUI<PurchaseInfo>, IDisplayModulesController, IFrameUpdate
{
	public interface ILimitedDisplayTime
	{
		bool isValid { get; }
	}

	[SerializeField]
	public GameButton purchaseButton;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI purchaseMoneyText;

	[SerializeField]
	private GameObject watchAdHint;

	[SerializeField]
	private MatterCell pruchaseMattercell;

	[SerializeField]
	private GameObject notPurchasableMask;

	[SerializeField]
	private TextMeshProUGUI notPurchaseableText;

	[SerializeField]
	private GlobalItemDisplay globalItemDisplay;

	[SerializeField]
	private TextMeshProUGUI purchaseCountText;

	[SerializeField]
	private GameObject bonusCountHint;

	[SerializeField]
	private TextMeshProUGUI bonusCountHintText;

	[SerializeField]
	private MatterCell bonusMatterCell;

	[SerializeField]
	private GameObject discountHint;

	[SerializeField]
	private TextMeshProUGUI discountHintText;

	[SerializeField]
	private TextMeshProUGUI extraHintText;

	[SerializeField]
	private TextMeshProUGUI originText;

	[SerializeField]
	private GameObject videoHint;

	[SerializeField]
	private TextMeshProUGUI videoHintText;

	[SerializeField]
	private HintRedDot redDot;

	[SerializeField]
	private GameButton fullCellButton;

	[SerializeField]
	private TextMeshProUGUI countdownText;

	private PurchaseBundleRewardPreview bundleRewardPreview;

	private Action customPurchaseAction;

	private string purchaseButtonTextOverride;

	private bool limitedCountdownWasActive;

	public GameObject purchaseCountTextParent => purchaseCountText.transform.parent.gameObject;

	private PurchaseGroupDef purchaseGroupDef => (parent as PurchaseGroupDefDisplay)?.setupData ?? CellUI<PurchaseInfo>.applicationState.purchaseManager.GetPurchaseGrouOfPurchase(purchase.purchaseId);

	private PurchaseInfo purchase => base.setupData;

	public bool Purchasable => purchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState);

	protected override void Init()
	{
		InitUI(pruchaseMattercell);
		InitUI(bonusMatterCell);
		InitUI(redDot);
		bundleRewardPreview = GetComponent<PurchaseBundleRewardPreview>();
		purchaseButton.onClick.AddListener(_PurchaseButtonClick);
		purchaseButton.onNotInteractableClick.AddListener(_OnNoPurchasableButtonClicked);
		redDot.Setup(new CustomModule
		{
			shouldShow = delegate
			{
				PurchaseInfo purchaseInfo = purchase;
				return purchaseInfo != null && purchaseInfo.IsFreePurchase && purchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState) && CellUI<PurchaseInfo>.applicationState.playerInfoManager.showFreePurchase;
			}
		});
		if (fullCellButton != null)
		{
			fullCellButton.onClick.AddListener(_PurchaseButtonClick);
		}
	}

	protected override void OnSetup(PurchaseInfo purchase)
	{
		limitedCountdownWasActive = false;
		SetDirty();
	}

	public void FrameUpdate()
	{
		if (purchase == null || countdownText == null)
		{
			return;
		}
		if (!TryGetLimitedDisplayRemainTime(out var remainTime))
		{
			countdownText.gameObject.SetActive(value: false);
			if (limitedCountdownWasActive)
			{
				limitedCountdownWasActive = false;
				CellUI<PurchaseInfo>.fortressState.uiManager.SetDirty();
			}
		}
		else
		{
			limitedCountdownWasActive = true;
			countdownText.gameObject.SetActive(value: true);
			countdownText.text = PurchaseInfo.FormatLimitedTimeBundleCountdown(remainTime);
		}
	}

	private bool TryGetLimitedDisplayRemainTime(out TimeSpan remainTime)
	{
		remainTime = TimeSpan.Zero;
		if (!(purchase is ILimitedDisplayTime))
		{
			return false;
		}
		LimitedTimeBundleInfo limitedTimeBundleInfo = CellUI<PurchaseInfo>.applicationState.purchaseManager.limitedTimeBundleInfos.FirstOrDefault((LimitedTimeBundleInfo info) => info != null && string.Equals(info.PurchaseId, purchase.purchaseId, StringComparison.Ordinal) && info.EndTimeUtc.HasValue);
		if (limitedTimeBundleInfo == null || !limitedTimeBundleInfo.EndTimeUtc.HasValue)
		{
			return false;
		}
		remainTime = limitedTimeBundleInfo.EndTimeUtc.Value - DateTime.UtcNow;
		return remainTime > TimeSpan.Zero;
	}

	public void SetCustomPurchaseAction(Action action)
	{
		customPurchaseAction = action;
	}

	public void SetPurchaseButtonTextOverride(string value)
	{
		purchaseButtonTextOverride = value;
		SetDirty();
	}

	public void SetGlobalItemInfoEnabled(bool enabled)
	{
		InfoModule infoModule = globalItemDisplay?.GetModule<InfoModule>();
		if (infoModule != null)
		{
			infoModule.infoPanel = (enabled ? UIHelper.GetDefaultCommonInfoPanel() : null);
		}
	}

	public void SetGlobalItemCountAreaAnchoredPosition(Vector2 anchoredPosition)
	{
		globalItemDisplay?.SetCountAreaAnchoredPosition(anchoredPosition);
	}

	public void ResetGlobalItemCountAreaAnchoredPosition()
	{
		globalItemDisplay?.ResetCountAreaAnchoredPosition();
	}

	public GameObject GetRefreshTimeDisplayTemplate()
	{
		RefreshTimeModule[] components = GetComponents<RefreshTimeModule>();
		foreach (RefreshTimeModule refreshTimeModule in components)
		{
			if (refreshTimeModule.DisplayArea != null)
			{
				return refreshTimeModule.DisplayArea;
			}
		}
		return null;
	}

	public void SetRefreshTimeVisible(bool visible)
	{
		RefreshTimeModule[] components = GetComponents<RefreshTimeModule>();
		for (int i = 0; i < components.Length; i++)
		{
			components[i].SetDisplayEnabled(visible);
		}
	}

	protected override void UpdateDisplay()
	{
		if (purchase == null)
		{
			return;
		}
		PurchaseGroupDef purchaseGroupDef = this.purchaseGroupDef;
		pruchaseMattercell.gameObject.SetActive(value: false);
		MatterInfo matterInfo = purchase.CurrentMatterCost(CellUI<PurchaseInfo>.applicationState);
		int num = purchase.CurrentDiscountPercent(CellUI<PurchaseInfo>.applicationState);
		string purchaseSubText = purchase.PurchaseSubText;
		description.text = purchaseSubText ?? "";
		description.gameObject.SetActive(!purchaseSubText.IsNullOrEmpty());
		purchaseButton.interactable = purchase.IsEnoughCost(CellUI<PurchaseInfo>.applicationState) && purchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState);
		if (matterInfo != null)
		{
			pruchaseMattercell.gameObject.SetActive(value: true);
			pruchaseMattercell.Setup(matterInfo);
		}
		else
		{
			pruchaseMattercell.gameObject.SetActive(value: false);
		}
		if (purchase.realMoneyCost > 0)
		{
			purchaseMoneyText.gameObject.SetActive(value: true);
			if (GlobalConfigManager.instance.enableNetwork)
			{
				string text = CellUI<PurchaseInfo>.applicationState.purchaseManager.LocPrice(purchase.purchaseId);
				purchaseMoneyText.text = (text.IsNullOrEmpty() ? ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_unavailable : text);
			}
		}
		else
		{
			purchaseMoneyText.gameObject.SetActive(value: false);
		}
		watchAdHint.gameObject.SetActive(value: false);
		videoHint.gameObject.SetActive(value: false);
		if (purchase.IsFreePurchase)
		{
			purchaseMoneyText.gameObject.SetActive(value: true);
			pruchaseMattercell.gameObject.SetActive(value: false);
			if (purchase is WatchAdGetMatterPurchase && !CellUI<PurchaseInfo>.applicationState.playerInfoManager.vip)
			{
				purchaseMoneyText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_WatchAd ?? "";
				videoHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_VIPSkip_hint ?? "";
				watchAdHint.gameObject.SetActive(value: true);
				videoHint.gameObject.SetActive(value: true);
			}
			else
			{
				purchaseMoneyText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_GetPurchase;
				videoHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_VIPPurchase ?? "";
			}
		}
		if (!string.IsNullOrEmpty(purchaseButtonTextOverride))
		{
			pruchaseMattercell.gameObject.SetActive(value: false);
			purchaseMoneyText.gameObject.SetActive(value: true);
			purchaseMoneyText.text = purchaseButtonTextOverride;
		}
		UpdatePurchasableMask();
		bool flag = ShouldShowPurchaseCount();
		bool flag2 = ((object)purchaseGroupDef != null && purchaseGroupDef.layoutType == PurchaseGroupLayoutType.HorizantalGrid && purchaseGroupDef.showCellRepeatTime) & flag;
		if (((object)purchaseGroupDef == null || purchaseGroupDef.layoutType != PurchaseGroupLayoutType.HorizantalGrid) & flag)
		{
			purchaseCountTextParent.SetActive(value: true);
			purchaseCountText.text = GetPurchaseCountText(purchaseGroupDef);
		}
		else
		{
			purchaseCountTextParent.SetActive(value: false);
		}
		if (num > 0)
		{
			if (purchase.TryBuildOriginalMatterCost(out var originalMatterCost))
			{
				originText.gameObject.SetActive(value: true);
				discountHint.gameObject.SetActive(value: true);
				originText.text = $"<S>{originalMatterCost.count}<S>";
				discountHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_discount_percent_arg0.FormatWith(num);
			}
			else
			{
				originText.gameObject.SetActive(value: false);
				discountHint.gameObject.SetActive(value: false);
			}
		}
		else
		{
			originText.gameObject.SetActive(value: false);
			discountHint.gameObject.SetActive(value: false);
		}
		UpdateBonusCountHint();
		if (extraHintText != null)
		{
			string text2 = purchase.GetExtraHintText(CellUI<PurchaseInfo>.applicationState);
			if (text2.IsNullOrEmpty() & flag2)
			{
				text2 = GetPurchaseCountText(purchaseGroupDef);
			}
			if (text2.IsNullOrEmpty())
			{
				extraHintText.gameObject.SetActive(value: false);
			}
			else
			{
				extraHintText.gameObject.SetActive(value: true);
				extraHintText.text = text2;
				extraHintText.transform.SetAsLastSibling();
			}
		}
		if (fullCellButton != null)
		{
			fullCellButton.gameObject.SetActive(value: false);
			if (purchase is RealMoneyToMatterPurchase && ((object)purchaseGroupDef == null || purchaseGroupDef.layoutType != PurchaseGroupLayoutType.ItemSkin))
			{
				fullCellButton.gameObject.SetActive(value: true);
			}
		}
		bool flag3 = PurchaseDisplayRules.IsDailyAdGiftPurchase(purchase);
		if (flag3)
		{
			globalItemDisplay.SetupWithoutCount(purchase);
		}
		else
		{
			globalItemDisplay.Setup(purchase);
		}
		if (bundleRewardPreview != null)
		{
			if (purchase is RewardBundlePurchase rewardBundlePurchase)
			{
				bundleRewardPreview.Setup(rewardBundlePurchase, this);
			}
			else if (flag3 && purchase is WatchAdGetMatterPurchase watchAdGetMatterPurchase)
			{
				bundleRewardPreview.Setup(watchAdGetMatterPurchase, this);
			}
			else
			{
				bundleRewardPreview.Hide();
			}
		}
		void UpdateBonusCountHint()
		{
			if (!(bonusCountHint == null) && !(bonusCountHintText == null) && !(bonusMatterCell == null))
			{
				bonusCountHint.gameObject.SetActive(value: false);
				if (purchase is RealMoneyToMatterPurchase realMoneyToMatterPurchase)
				{
					if (realMoneyToMatterPurchase.firstPurchaseDouble && realMoneyToMatterPurchase.PurchasedCount(CellUI<PurchaseInfo>.applicationState) == 0)
					{
						bonusCountHint.gameObject.SetActive(value: true);
						bonusMatterCell.Setup(new MatterInfo(realMoneyToMatterPurchase.gain.matterDef, realMoneyToMatterPurchase.gain.count));
						if (bonusCountHintText != null)
						{
							bonusCountHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_first_double;
						}
					}
					else if (realMoneyToMatterPurchase.bonusCount != 0)
					{
						bonusCountHint.gameObject.SetActive(value: true);
						bonusMatterCell.Setup(new MatterInfo(realMoneyToMatterPurchase.gain.matterDef, realMoneyToMatterPurchase.bonusCount));
						if (bonusCountHintText != null)
						{
							bonusCountHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_bonus;
						}
					}
				}
			}
		}
		void UpdatePurchasableMask()
		{
			if (!(notPurchasableMask == null) && !(notPurchaseableText == null))
			{
				if (!Purchasable)
				{
					notPurchasableMask.gameObject.SetActive(value: true);
					notPurchaseableText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_sold_out_short;
					if (!purchase.PurchaseCountEnough(CellUI<PurchaseInfo>.applicationState))
					{
						notPurchaseableText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_sold_out_reset_in_time_arg0.FormatWith(ScriptableObjectSingleton<TextDataPreset>.instance.GetNextTimePeriodTerm(purchase.resetTimesType));
					}
				}
				else
				{
					notPurchasableMask.gameObject.SetActive(value: false);
				}
			}
		}
	}

	private bool ShouldShowPurchaseCount()
	{
		if (purchase.CurrentStageHasUnlimitedPurchaseCount(CellUI<PurchaseInfo>.applicationState))
		{
			return false;
		}
		return !purchase.buyTimesLimit || purchase.resetTimesType != ResetTimeType.None || purchase.repeatBuyTimes != 1;
	}

	private string GetPurchaseCountText(PurchaseGroupDef groupDef)
	{
		int num = purchase.CurrentStageRemainPurchaseCount(CellUI<PurchaseInfo>.applicationState);
		if (purchase.IsDiscountStageActive(CellUI<PurchaseInfo>.applicationState))
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_discount_remain_times_arg0.FormatWith(num);
		}
		if ((object)groupDef == null || !groupDef.showCellRepeatTime)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_remain_times_arg0.FormatWith(num);
		}
		switch (purchase.resetTimesType)
		{
		case ResetTimeType.Daily:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_remain_times_day.FormatWith(num);
		case ResetTimeType.Weekly:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_remain_times_week.FormatWith(num);
		case ResetTimeType.Monthly:
		case ResetTimeType.MonthlyCardExpired:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_remain_times_month.FormatWith(num);
		default:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase_remain_times_arg0.FormatWith(num);
		}
	}

	private void _PurchaseButtonClick()
	{
		if (customPurchaseAction != null)
		{
			customPurchaseAction();
		}
		else
		{
			DoPurchase(purchase);
		}
	}

	private void DoPurchase(PurchaseInfo targetPurchase)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (targetPurchase == null || !targetPurchase.IsEnoughCost(CellUI<PurchaseInfo>.applicationState) || !targetPurchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState))
		{
			return;
		}
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_purchase_click);
		if (targetPurchase.mattercost != null)
		{
			if (targetPurchase is RewardBundlePurchase rewardBundlePurchase && CellUI<PurchaseInfo>.fortressState != null)
			{
				CellUI<PurchaseInfo>.fortressState.uiManager.purchaseBundleCheckPanel.ShowUI(rewardBundlePurchase);
			}
			else
			{
				CellUI<PurchaseInfo>.fortressState.uiManager.purchaseConfirmPanel.Setup(targetPurchase);
			}
		}
		else
		{
			CellUI<PurchaseInfo>.applicationState.purchaseManager.DoPurchase(targetPurchase, 1, AfterPurchase);
		}
		void AfterPurchase(GlobalRewardPackage reward)
		{
			TraceLog.Debug("[PurchaseCell] AfterPurchase()");
			if (targetPurchase is RealMoneyToMatterPurchase { useMatterLootAfterPurchaseInsteadShow: not false } realMoneyToMatterPurchase)
			{
				if (realMoneyToMatterPurchase.gain.matterDef.matterType == MatterDef.MatterType.Loot && realMoneyToMatterPurchase.gain.matterDef.lootSetup is PickLootSetup pickLootSetup && pickLootSetup.globalRewards.TrueForAll((GlobalRewardSetup s) => s is RandomItemSkinInSkinSetReward))
				{
					CellUI<PurchaseInfo>.fortressState.uiManager.itemSkinSetSelectPanel.ShowUI(realMoneyToMatterPurchase.gain);
				}
				else
				{
					CellUI<PurchaseInfo>.fortressState.uiManager.globalRewardBundleExchangePanel.ShowUI(realMoneyToMatterPurchase.gain);
				}
			}
			else
			{
				CellUI<PurchaseInfo>.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
			}
			CellUI<PurchaseInfo>.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
			CellUI<PurchaseInfo>.fortressState.uiManager.purchaseUIPanel.SetDirty();
		}
	}

	private void _OnNoPurchasableButtonClicked()
	{
		OnNoPurchasableButtonClicked(purchase);
	}

	private void OnNoPurchasableButtonClicked(PurchaseInfo targetPurchase)
	{
		if (targetPurchase != null)
		{
			if (!targetPurchase.IsAllowPurchase(CellUI<PurchaseInfo>.applicationState))
			{
				CellUI<PurchaseInfo>.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_sold_out);
			}
			else if (!targetPurchase.IsEnoughCost(CellUI<PurchaseInfo>.applicationState))
			{
				CellUI<PurchaseInfo>.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_not_purchasable_not_enough_matter_arg0_hint.FormatWith(targetPurchase.CurrentMatterCost(CellUI<PurchaseInfo>.applicationState)?.matterDef?.matterName ?? ""));
			}
		}
	}
}
