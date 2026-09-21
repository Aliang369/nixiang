using System;
using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class MonthCheckPanel : UIBase, IStackPanel
{
	private const int MaxCalendarRowCountWithoutResize = 5;

	private const float CompactCalendarTargetCellSize = 135f;

	private const int CompactCalendarTopPadding = 6;

	private const int CompactCalendarBottomPadding = 0;

	private const float CompactCalendarRowSpacing = 1f;

	public MonthCheckCell dailyCheckCellPrefab;

	public GameButton closeButton;

	public MonthCheckCell monthCardCheckCell;

	public MonthCheckCell monthCardCheckDoubleCell;

	public TextMeshProUGUI monthCardCheckRemainDaysText;

	public GameButton monthCardPurchaseJumpButton;

	public CellUIPrefabPool<MonthCheckCell, MonthCheckInState> pool;

	private TableLayoutGroup monthCheckLayout;

	private float[] defaultColumnWidths;

	private float defaultMinimumRowHeight;

	private float defaultColumnSpacing;

	private float defaultRowSpacing;

	private int defaultPaddingTop;

	private int defaultPaddingBottom;

	private float defaultGridWidth;

	protected override void Init()
	{
		pool = GetPrefabPool<MonthCheckCell, MonthCheckInState>(dailyCheckCellPrefab, dailyCheckCellPrefab.transform.parent);
		InitMonthCheckLayout();
		InitUI(monthCardCheckCell);
		InitUI(monthCardCheckDoubleCell);
		closeButton.onClick.AddListener(_OnCloseButtonClicked);
		monthCardPurchaseJumpButton.onClick.AddListener(_OnPurchaseButtonClicked);
		UIBase.applicationState.eventManager.ReceiveRewardEvent.AddListener(_OnReceiveReward);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.ReceiveRewardEvent.RemoveListener(_OnReceiveReward);
	}

	protected override void UpdateDisplay()
	{
		pool.Sync(UIBase.applicationState.dailyCheckManager.monthCheckState);
		UpdateMonthCheckLayout(UIBase.applicationState.dailyCheckManager.monthCheckState.Count);
		monthCardCheckCell.Setup(new MonthCheckInState
		{
			index = DateTime.Now.Day,
			isCollected = UIBase.applicationState.playerInfoManager.IsMonthlyCardActive,
			isCardReward = true
		});
		monthCardCheckDoubleCell.Setup(new MonthCheckInState
		{
			index = DateTime.Now.Day,
			isCollected = UIBase.applicationState.playerInfoManager.IsMonthlyCardActive,
			isDoubleReward = true
		});
		if (UIBase.applicationState.playerInfoManager.IsMonthlyCardActive)
		{
			monthCardPurchaseJumpButton.gameObject.SetActive(value: true);
			monthCardCheckRemainDaysText.gameObject.SetActive(value: true);
			monthCardCheckRemainDaysText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_month_card_remain_arg0_days.FormatWith(UIBase.applicationState.playerInfoManager.RemainingMonthlyCardDays);
		}
		else
		{
			monthCardPurchaseJumpButton.gameObject.SetActive(value: true);
			monthCardCheckRemainDaysText.gameObject.SetActive(value: false);
		}
	}

	private void InitMonthCheckLayout()
	{
		monthCheckLayout = dailyCheckCellPrefab.transform.parent.GetComponent<TableLayoutGroup>();
		if (monthCheckLayout == null)
		{
			Debug.LogError("[MonthCheckPanel] Missing TableLayoutGroup on month check cell parent.", this);
			return;
		}
		defaultColumnWidths = (float[])monthCheckLayout.ColumnWidths.Clone();
		defaultMinimumRowHeight = monthCheckLayout.MinimumRowHeight;
		defaultColumnSpacing = monthCheckLayout.ColumnSpacing;
		defaultRowSpacing = monthCheckLayout.RowSpacing;
		defaultPaddingTop = monthCheckLayout.padding.top;
		defaultPaddingBottom = monthCheckLayout.padding.bottom;
		float[] array = defaultColumnWidths;
		foreach (float num in array)
		{
			defaultGridWidth += num;
		}
		defaultGridWidth += defaultColumnSpacing * (float)Mathf.Max(0, defaultColumnWidths.Length - 1);
	}

	private void UpdateMonthCheckLayout(int cellCount)
	{
		if (monthCheckLayout == null || defaultColumnWidths == null || defaultColumnWidths.Length == 0)
		{
			return;
		}
		int num = defaultColumnWidths.Length;
		int num2 = Mathf.CeilToInt((float)cellCount / (float)num);
		if (num2 <= 0)
		{
			return;
		}
		if (num2 <= 5)
		{
			monthCheckLayout.ColumnWidths = (float[])defaultColumnWidths.Clone();
			monthCheckLayout.MinimumRowHeight = defaultMinimumRowHeight;
			monthCheckLayout.ColumnSpacing = defaultColumnSpacing;
			monthCheckLayout.RowSpacing = defaultRowSpacing;
			monthCheckLayout.padding.top = defaultPaddingTop;
			monthCheckLayout.padding.bottom = defaultPaddingBottom;
		}
		else
		{
			float num3 = ((RectTransform)monthCheckLayout.transform).rect.height - 6f - 0f - 1f * (float)(num2 - 1);
			float num4 = Mathf.Min(135f, Mathf.Min(defaultMinimumRowHeight, num3 / (float)num2));
			float[] array = new float[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = num4;
			}
			monthCheckLayout.ColumnWidths = array;
			monthCheckLayout.MinimumRowHeight = num4;
			monthCheckLayout.RowSpacing = 1f;
			monthCheckLayout.ColumnSpacing = ((num > 1) ? ((defaultGridWidth - num4 * (float)num) / (float)(num - 1)) : 0f);
			monthCheckLayout.padding.top = 6;
			monthCheckLayout.padding.bottom = 0;
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)monthCheckLayout.transform);
	}

	public IEnumerator AutoGive(MonthCheckInResult monthCheckInResult, MonthCardCheckInResult monthCardCheckInResult = null)
	{
		base.gameObject.SetActive(value: true);
		UpdateDisplay();
		MonthCheckInState state = UIBase.applicationState.dailyCheckManager.GetCurrentMonthCheckState(DateTime.Now.Day);
		if (state == null)
		{
			yield break;
		}
		MonthCheckCell monthCheckCell = pool.GetActiveElements().Find((MonthCheckCell c) => c._setupData == state);
		if (monthCheckCell == null)
		{
			yield break;
		}
		yield return monthCheckCell.PlayGiveAnim(monthCheckInResult.reward);
		yield return new WaitUntil(() => !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
		if (monthCardCheckInResult != null)
		{
			yield return monthCardCheckCell.PlayGiveAnim(monthCardCheckInResult.reward);
			yield return new WaitUntil(() => !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
			yield return monthCardCheckDoubleCell.PlayGiveAnim(monthCardCheckInResult.checkInDoubleReward);
			yield return new WaitUntil(() => !UIBase.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
		}
		yield return new WaitUntil(() => !UIBase.applicationState.fortressState.uiManager.gachaResultDisplay.gameObject.activeInHierarchy && !UIBase.applicationState.fortressState.uiManager.globalRewardPackageDisplay.gameObject.activeInHierarchy);
		yield return new WaitUntil(() => !base.gameObject.activeInHierarchy);
	}

	private void _OnReceiveReward(GlobalRewardInfo reward)
	{
		SetDirty();
	}

	private void _OnCloseButtonClicked()
	{
		SetActiveAndDirtyWithFadeAnim(active: false);
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
	}

	private void _OnPurchaseButtonClicked()
	{
		if (UIBase.fortressState.uiManager.purchaseUIPanel.TryShowPurchaseGroup(PurchaseGroupType.RealMoneyBuy))
		{
			UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		}
	}
}
