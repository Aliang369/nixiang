using System;
using TMPro;
using UnityEngine;

public class PurchaseConfirmPanel : UIBase
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private MatterCell costItemDisplay;

	[SerializeField]
	private GlobalItemDisplay purchaseItemDisplay;

	[SerializeField]
	private GameObject convertMatterArea;

	[SerializeField]
	private MatterCell convertMatterSourceCell;

	[SerializeField]
	private RangeSliderWithAdjustButton convertMatterSlider;

	[SerializeField]
	private TextMeshProUGUI remainCountText;

	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	[SerializeField]
	private GlobalItemBoxInfoDisplay globalItemBoxInfoDisplay;

	[NonSerialized]
	private PurchaseInfo purchase;

	public bool ToShowConvertSlider
	{
		get
		{
			if (purchase == null || !purchase.SupportBatchPurchase)
			{
				return false;
			}
			convertMatterSlider.MinValue = 1;
			return GetCurrentMaxPurchaseTimes(purchase) > 1;
		}
	}

	protected override void Init()
	{
		InitUI(purchaseItemDisplay);
		InitUI(convertMatterSourceCell);
		InitUI(costItemDisplay);
		InitUI(skillInfoDisplay);
		InitUI(globalItemBoxInfoDisplay);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		convertMatterSlider.onValueChanged.AddListener(_OnConvertMatterSliderValueChanged);
	}

	public void Setup(PurchaseInfo purchase)
	{
		this.purchase = purchase;
		skillInfoDisplay.Unset();
		globalItemBoxInfoDisplay.Unset();
		if (purchase.GlobalItemRefSkill != null)
		{
			skillInfoDisplay.Setup(new GameSkill(purchase.GlobalItemRefSkill));
		}
		else
		{
			globalItemBoxInfoDisplay.Setup(purchase);
		}
		if (!purchase.hideRangeSliderArea && ToShowConvertSlider)
		{
			convertMatterSlider.MinValue = 1;
			convertMatterSlider.MaxValue = Mathf.Max(GetCurrentMaxPurchaseTimes(purchase), 1);
			convertMatterSlider.CurrentValue = 1;
		}
		purchaseItemDisplay.Setup(purchase);
		SetActiveAndDirty(active: true);
	}

	protected override void UpdateDisplay()
	{
		MatterInfo matterInfo = purchase.CurrentMatterCost(UIBase.applicationState);
		remainCountText.transform.parent.gameObject.SetActive(value: false);
		if (ToShowConvertSlider)
		{
			convertMatterArea.gameObject.SetActive(value: true);
			int count = convertMatterSlider.CurrentValue * matterInfo.count;
			convertMatterSourceCell.Setup(new MatterInfo(matterInfo.matterDef, count));
			costItemDisplay.Setup(new MatterInfo(matterInfo.matterDef, matterInfo.count));
			if (purchase is ConvertMatterToMatterPurchase convertMatterToMatterPurchase)
			{
				purchaseItemDisplay.Setup(new GlobalRewardInfo
				{
					matter = new MatterInfo(convertMatterToMatterPurchase.gain.matterDef, convertMatterToMatterPurchase.gain.count)
				});
			}
			else
			{
				purchaseItemDisplay.Setup(purchase);
			}
		}
		else
		{
			costItemDisplay.Setup(matterInfo);
			convertMatterSourceCell.Setup(matterInfo);
			convertMatterArea.gameObject.SetActive(value: false);
			remainCountText.transform.parent.gameObject.SetActive(value: true);
			remainCountText.text = $"{GetCurrentMaxPurchaseTimes(purchase)}";
		}
		int count2 = ((!ToShowConvertSlider) ? 1 : convertMatterSlider.CurrentValue);
		confirmButton.interactable = purchase.IsEnoughCost(UIBase.applicationState, count2) && purchase.IsAllowPurchase(UIBase.applicationState);
		base.rectTransform.RefreshLayout();
	}

	private void _OnConfirmButtonClicked()
	{
		int times = ((!ToShowConvertSlider) ? 1 : convertMatterSlider.CurrentValue);
		UIBase.applicationState.purchaseManager.DoPurchase(purchase, times, delegate(GlobalRewardPackage reward)
		{
			base.gameObject.SetActive(value: false);
			UIBase.fortressState.uiManager.purchaseUIPanel.SetDirty();
			UIBase.fortressState.uiManager.purchaseUIPanel.fortressGlobalRewardPackageDisplay.Setup(reward);
			UIBase.fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
		});
	}

	private void _OnCancelButtonClicked()
	{
		base.gameObject.SetActive(value: false);
		SetDirty();
	}

	private void _OnConvertMatterSliderValueChanged(int value)
	{
		SetDirty();
	}

	private int GetCurrentMaxPurchaseTimes(PurchaseInfo targetPurchase)
	{
		MatterInfo matterInfo = targetPurchase.CurrentMatterCost(UIBase.applicationState);
		if (matterInfo == null || matterInfo.matterDef == null || matterInfo.count <= 0)
		{
			return 0;
		}
		int a = (targetPurchase.CurrentStageHasUnlimitedPurchaseCount(UIBase.applicationState) ? 99 : Mathf.Max(targetPurchase.CurrentStageRemainPurchaseCount(UIBase.applicationState), 0));
		int b = UIBase.applicationState.matterManager.GetMatterCount(matterInfo.matterDef) / matterInfo.count;
		return Mathf.Min(a, b);
	}
}
