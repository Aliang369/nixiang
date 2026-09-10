using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using ContainerOfSoul.Display.Fortress.Purchase;
using TMPro;
using UnityEngine;

public class LimitedTimeBundlePanel : UIBase, IStackPanel, IFrameUpdate
{
	[SerializeField]
	public PurchaseCell purchaseCell;

	[SerializeField]
	public GameButton closeButton;

	[SerializeField]
	public GlobalItemDisplay globalItemDisplayPrefab;

	[SerializeField]
	private TextMeshProUGUI countdownText;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private LimitedTimeBundleInfo limitedTimeBundleInfo;

	protected override void Init()
	{
		InitUI(purchaseCell);
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(globalItemDisplayPrefab);
		closeButton.onClick.AddListener(_OnBackButtonClicked);
	}

	public bool Setup(LimitedTimeBundleInfo bundleInfo)
	{
		if (bundleInfo == null)
		{
			base.gameObject.SetActive(value: false);
			return false;
		}
		RewardBundlePurchase purchaseInfo = UIBase.applicationState.purchaseManager.GetPurchaseInfo(bundleInfo.PurchaseId) as RewardBundlePurchase;
		if (purchaseInfo == null || (purchaseInfo != null && !((PurchaseCell.ILimitedDisplayTime)purchaseInfo).isValid))
		{
			base.gameObject.SetActive(value: false);
			return false;
		}
		limitedTimeBundleInfo = bundleInfo;
		UIBase.fortressState?.uiManager.universalCommonInfoPanel.HideAll();
		purchaseCell.Setup(purchaseInfo);
		purchaseCell.SetCustomPurchaseAction(delegate
		{
			PurchaseFromPopup(purchaseInfo);
		});
		pool.DespawnAll();
		pool.Sync(((IEnumerable<GlobalRewardSetup>)purchaseInfo.rewardBundle.rewards).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
		UpdateCountdownText();
		return true;
	}

	public void FrameUpdate()
	{
		UpdateCountdownText();
	}

	private void UpdateCountdownText()
	{
		if (countdownText == null)
		{
			return;
		}
		LimitedTimeBundleInfo obj = limitedTimeBundleInfo;
		if (obj != null && obj.EndTimeUtc.HasValue)
		{
			TimeSpan timeSpan = limitedTimeBundleInfo.EndTimeUtc.Value - DateTime.UtcNow;
			if (timeSpan < TimeSpan.Zero)
			{
				timeSpan = TimeSpan.Zero;
			}
			countdownText.gameObject.SetActive(value: true);
			countdownText.text = PurchaseInfo.FormatLimitedTimeBundleCountdown(timeSpan);
		}
	}

	private void PurchaseFromPopup(RewardBundlePurchase purchaseInfo)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (purchaseInfo.IsEnoughCost(UIBase.applicationState) && purchaseInfo.IsAllowPurchase(UIBase.applicationState))
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_purchase_click);
			UIBase.applicationState.purchaseManager.DoPurchase(purchaseInfo, 1, delegate(GlobalRewardPackage reward)
			{
				base.gameObject.SetActive(value: false);
				UIBase.fortressState.uiManager.purchaseUIPanel.SetDirty();
				UIBase.fortressState.uiManager.globalRewardPackageDisplay.Setup(reward);
				UIBase.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
				ThinkingAnalyticsExtends.TrackFinishPromotionPopup(UIBase.applicationState, limitedTimeBundleInfo.PurchaseId, "完成弹窗付费");
			});
		}
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		if (limitedTimeBundleInfo != null)
		{
			ThinkingAnalyticsExtends.TrackFinishPromotionPopup(UIBase.applicationState, limitedTimeBundleInfo.PurchaseId, "跳过弹窗付费");
		}
	}
}
