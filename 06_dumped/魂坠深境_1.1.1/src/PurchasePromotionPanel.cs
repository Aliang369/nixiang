using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.Purchase;
using TMPro;
using UnityEngine;

public class PurchasePromotionPanel : UIBase, IStackPanel
{
	public PurchaseCell purchaseCell;

	public GameButton closeButton;

	public GlobalItemDisplay globalItemDisplayPrefab;

	[SerializeField]
	private TextMeshProUGUI characterNameText;

	[NonSerialized]
	private CellUIPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup> pool;

	[NonSerialized]
	private PurchasePromotion promotion;

	protected override void Init()
	{
		InitUI(purchaseCell);
		pool = GetPrefabPool<GlobalItemDisplay, IGlobalItemDisplaySetup>(globalItemDisplayPrefab);
		closeButton.onClick.AddListener(_OnBackButtonClicked);
	}

	public void Setup(PurchasePromotion purchasePromotion)
	{
		if (purchasePromotion == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		RewardBundlePurchase purchaseInfo = UIBase.applicationState.purchaseManager.GetPurchaseInfo(purchasePromotion.purchaseId) as RewardBundlePurchase;
		if (purchaseInfo == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		promotion = purchasePromotion;
		UIBase.fortressState?.uiManager.universalCommonInfoPanel.HideAll();
		RefreshCharacterNameText(purchaseInfo);
		purchaseCell.Setup(purchaseInfo);
		purchaseCell.SetCustomPurchaseAction(delegate
		{
			PurchaseFromPopup(purchaseInfo);
		});
		pool.Sync(((IEnumerable<GlobalRewardSetup>)purchaseInfo.rewardBundle.rewards).Select((Func<GlobalRewardSetup, IGlobalItemDisplaySetup>)((GlobalRewardSetup r) => r)).ToList());
	}

	private void RefreshCharacterNameText(RewardBundlePurchase purchaseInfo)
	{
		if (!(characterNameText == null))
		{
			string text = purchaseInfo.rewardBundle?.rewards?.Select((GlobalRewardSetup r) => r?.GlobalItemRefPlayerCharacter?.characterName).FirstOrDefault((string t) => !string.IsNullOrEmpty(t));
			if (!string.IsNullOrEmpty(text))
			{
				characterNameText.text = text;
			}
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
				ThinkingAnalyticsExtends.TrackFinishPromotionPopup(UIBase.applicationState, promotion.PromotionId, "完成弹窗付费");
			});
		}
	}

	private void _OnBackButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		ThinkingAnalyticsExtends.TrackFinishPromotionPopup(UIBase.applicationState, promotion.PromotionId, "跳过弹窗付费");
	}
}
