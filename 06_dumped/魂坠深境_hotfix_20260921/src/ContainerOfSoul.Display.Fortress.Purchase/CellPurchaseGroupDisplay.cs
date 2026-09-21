using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.Purchase;

public class CellPurchaseGroupDisplay : PurchaseGroupDefDisplay, IFrameUpdate
{
	[SerializeField]
	private ScrollRect scrollRect;

	[SerializeField]
	private TextMeshProUGUI cleanedText;

	[SerializeField]
	private GameObject expireDateHintArea;

	[SerializeField]
	private TextMeshProUGUI expireDateText;

	[SerializeField]
	private GameObject weeklyTokenLimitHintArea;

	[SerializeField]
	private TextMeshProUGUI weeklyTokenLimitHintText;

	[SerializeField]
	private MatterCell weeklyTokenLimitHintMatterCell;

	[SerializeField]
	private CellUIPrefabPoolConfig<PurchaseCell, PurchaseInfo> purchaseCellPool;

	[SerializeField]
	private CellUIPrefabPoolConfig<PurchaseCell, PurchaseInfo> bundlePurchaseCellPool;

	[SerializeField]
	private Animator animator;

	private PurchaseGroupDef purchaseGroup => base.setupData;

	protected override void Init()
	{
		if (weeklyTokenLimitHintMatterCell != null)
		{
			InitUI(weeklyTokenLimitHintMatterCell);
		}
		Init(purchaseCellPool);
		Init(bundlePurchaseCellPool);
	}

	protected override void OnSetup(PurchaseGroupDef _)
	{
		CellUI<PurchaseGroupDef>.fortressState.StartCoroutine(ShowCor);
	}

	protected override void OnUnset()
	{
		purchaseCellPool.DespawnAll();
		bundlePurchaseCellPool?.DespawnAll();
	}

	public void FrameUpdate()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			return;
		}
		expireDateHintArea.SetActive(value: false);
		if (purchaseGroup.manualEndTime.HasValue)
		{
			expireDateHintArea.SetActive(value: true);
			expireDateText.text = TextDisplayHelper.GetTimeSpanText(purchaseGroup.manualEndTime.Value.UtcDateTime - DateTime.UtcNow);
			return;
		}
		switch (purchaseGroup.resetTimeType)
		{
		case ResetTimeType.Daily:
			expireDateHintArea.SetActive(value: true);
			expireDateText.text = TextDisplayHelper.GetTimeSpanText(ScriptableObjectSingleton<CalendarDataManager>.instance.GetDayPeriodEndTime().UtcDateTime - DateTime.UtcNow);
			break;
		case ResetTimeType.Weekly:
			expireDateHintArea.SetActive(value: true);
			expireDateText.text = TextDisplayHelper.GetTimeSpanText(ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime().UtcDateTime - DateTime.UtcNow);
			break;
		case ResetTimeType.Monthly:
			expireDateHintArea.SetActive(value: true);
			expireDateText.text = TextDisplayHelper.GetTimeSpanText(ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime().UtcDateTime - DateTime.UtcNow);
			break;
		case ResetTimeType.None:
		case ResetTimeType.MonthlyCardExpired:
			break;
		}
	}

	protected override void UpdateDisplay()
	{
		if (purchaseGroup == null)
		{
			return;
		}
		if (weeklyTokenLimitHintArea != null)
		{
			bool flag = purchaseGroup.groupType == PurchaseGroupType.WeekShuffle;
			weeklyTokenLimitHintArea.SetActive(flag);
			if (flag)
			{
				weeklyTokenLimitHintText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_weekly_token_retention_hint;
				weeklyTokenLimitHintMatterCell?.Setup(new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatter, ScriptableObjectSingleton<FortressDataManager>.instance.weeklyTokenMatterUpperLimit));
			}
		}
		List<PurchaseInfo> list = ((base.showChecker != null) ? purchaseGroup.purchases.Where(base.showChecker).ToList() : purchaseGroup.purchases.ToList());
		if (bundlePurchaseCellPool != null)
		{
			List<PurchaseInfo> list2 = list.Where(ShouldUseBundlePreviewCell).ToList();
			List<PurchaseInfo> list3 = list.Where((PurchaseInfo p) => !ShouldUseBundlePreviewCell(p)).ToList();
			list2.RemoveAll((PurchaseInfo p) => p is PurchaseCell.ILimitedDisplayTime limitedDisplayTime && !limitedDisplayTime.isValid);
			purchaseCellPool.Sync(list3);
			bundlePurchaseCellPool.Sync(list2);
			OrderActiveCells();
		}
		else
		{
			purchaseCellPool.Sync(list);
			purchaseCellPool.pool.Order(SortCells);
		}
		cleanedText.gameObject.SetActive(list.Count == 0);
	}

	private bool ShouldUseBundlePreviewCell(PurchaseInfo purchase)
	{
		RewardBundlePurchase rewardBundlePurchase = purchase as RewardBundlePurchase;
		if (purchaseGroup.groupType != PurchaseGroupType.RealMoneyBuy || purchaseGroup.layoutType != PurchaseGroupLayoutType.HorizantalGrid)
		{
			return false;
		}
		if (PurchaseDisplayRules.IsDailyAdGiftPurchase(purchase))
		{
			return true;
		}
		if (rewardBundlePurchase == null)
		{
			return false;
		}
		int valueOrDefault = (rewardBundlePurchase.rewardBundle?.rewards?.Count).GetValueOrDefault();
		if (valueOrDefault >= 2)
		{
			return valueOrDefault <= 6;
		}
		return false;
	}

	private void OrderActiveCells()
	{
		List<PurchaseCell> list = new List<PurchaseCell>();
		list.AddRange(purchaseCellPool.pool.GetActiveElements());
		list.AddRange(bundlePurchaseCellPool.pool.GetActiveElements());
		foreach (PurchaseCell item in SortCells(list))
		{
			item.transform.SetAsLastSibling();
		}
	}

	private List<PurchaseCell> SortCells(List<PurchaseCell> cells)
	{
		return (from c in cells
			orderby c.Purchasable descending, purchaseGroup.purchases.IndexOf(c.setupData)
			select c).ToList();
	}

	private IEnumerator ShowCor()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		ResetLayout();
		yield return null;
		ResetLayout();
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
	}

	private void ResetLayout()
	{
		base.rectTransform.RefreshLayout();
		scrollRect.horizontalNormalizedPosition = 0f;
	}
}
