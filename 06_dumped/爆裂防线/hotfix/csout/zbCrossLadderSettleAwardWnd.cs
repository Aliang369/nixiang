using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCrossLadderSettleAwardWnd : zbBaseWnd
{
	private Label m_lbTime;

	private ScrollView m_scrollView;

	public Button m_btnClose;

	private zbLadderCsvRecord[] m_records;

	private List<zbCrossLadderSettleAwardItem> m_items = new List<zbCrossLadderSettleAwardItem>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitColumns();
	}

	private void InitColumns()
	{
		m_records = zbDataConfig.Instance.ladderCsvTable.GetRecords().records.Values.OrderByDescending((zbLadderCsvRecord r) => r.id).ToArray();
		((VisualElement)m_scrollView).Clear();
		zbLadderCsvRecord[] records = m_records;
		foreach (zbLadderCsvRecord record in records)
		{
			zbCrossLadderSettleAwardItem zbCrossLadderSettleAwardItem2 = new zbCrossLadderSettleAwardItem();
			zbCrossLadderSettleAwardItem2.OnEnable(this);
			zbCrossLadderSettleAwardItem2.Init(record);
			m_items.Add(zbCrossLadderSettleAwardItem2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbCrossLadderSettleAwardItem2);
		}
	}

	public override void Refresh()
	{
		RefreshTime();
		foreach (zbCrossLadderSettleAwardItem item in m_items)
		{
			item.Refresh();
		}
		int targetLadderId = Math.Max(1, zbServerData.sGameData.CrossLadderInfo.AchieveInfo.BestLadderId - 2);
		if (targetLadderId == 1)
		{
			((VisualElement)m_scrollView).schedule.Execute((Action)delegate
			{
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				float height = m_scrollView.contentViewport.resolvedStyle.height;
				float height2 = ((VisualElement)m_scrollView).contentContainer.resolvedStyle.height;
				float num = Mathf.Max(0f, height2 - height);
				m_scrollView.scrollOffset = new Vector2(0f, num);
			}).StartingIn(1L);
		}
		else
		{
			int index = Array.FindIndex(m_records, (zbLadderCsvRecord d) => d.id == targetLadderId);
			zbUtil.ScrollVerticalToIndex(m_scrollView, 6, m_records.Length, 190f, index);
		}
	}

	private void RefreshTime()
	{
		if (zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeSmart(zbServerData.sGameData.CrossLadderInfo.OpenInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
		else
		{
			((TextElement)m_lbTime).text = "";
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnClose()
	{
		Hide();
	}
}
