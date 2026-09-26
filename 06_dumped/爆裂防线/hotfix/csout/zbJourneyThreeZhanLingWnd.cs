using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyThreeZhanLingWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbTime;

	private Label m_lbLv;

	private Label m_lbDesc;

	private Label m_lbCount;

	private VisualElement m_progressVal;

	private ListView m_listView;

	private Button m_btnReceive;

	private Button m_btnBuyMid;

	private Button m_btnBuyHigh;

	private Button m_btnClose;

	private VisualElement m_notify;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11176");
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		EnableListView();
		m_btnReceive = UQueryExtensions.Q<Button>(m_RootElement, "btn_receive", (string)null);
		m_btnBuyHigh = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyHigh", (string)null);
		m_btnBuyMid = UQueryExtensions.Q<Button>(m_RootElement, "btn_buyMid", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnReceive);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceive, (Action)OnReceive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyMid, (Action)OnBuyMid, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuyHigh, (Action)OnBuyHigh, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_JourneyInfo, OnEventUpdate);
	}

	public override void Refresh()
	{
		InitBtns();
		InitBanner();
		UpdateNotify();
		UpdateListView();
		RefreshLeftTime();
	}

	private void EnableListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbJourneyThreeZhanLingItem obj = new zbJourneyThreeZhanLingItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbChapterThreeZhanLingCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbChapterThreeZhanLingCsvRecord>;
			(element as zbJourneyThreeZhanLingItem).Init(list[index]);
		};
	}

	private void UpdateListView()
	{
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterThree.ZhanLing;
		List<zbChapterThreeZhanLingCsvRecord> list = zbDataConfig.Instance.chapterThreeZhanLingCsvTable.GetRecords().records.Values.ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		((BaseVerticalCollectionView)m_listView).ScrollToItemById(Mathf.Min((zhanLing.Level > 2) ? (zhanLing.Level + 2) : zhanLing.Level, list.Count - 4));
	}

	private void InitBanner()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterThree.ZhanLing;
		Dictionary<int, zbChapterThreeZhanLingCsvRecord> records = zbDataConfig.Instance.chapterThreeZhanLingCsvTable.GetRecords().records;
		zbChapterThreeZhanLingCsvRecord recordByKey = zbDataConfig.Instance.chapterThreeZhanLingCsvTable.GetRecordByKey(zbServerData.sGameData.JourneyInfo.ChapterThree.ZhanLing.Level + 1);
		bool flag = zhanLing.Level == records.Count || recordByKey == null;
		((TextElement)m_lbLv).text = zhanLing.Level.ToString();
		((TextElement)m_lbCount).text = (flag ? "Max" : (zhanLing.Reach + "/" + recordByKey.openCount));
		float num = (flag ? 1f : Mathf.Clamp01((float)zhanLing.Reach / (float)recordByKey.openCount));
		m_progressVal.style.width = new StyleLength(Length.Percent(num * 100f));
	}

	private void InitBtns()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		int journeyThreeMidGoodsId = zbSelf.Instance.journeyThreeHandler.journeyThreeMidGoodsId;
		int journeyThreeHighGoodsId = zbSelf.Instance.journeyThreeHandler.journeyThreeHighGoodsId;
		bool flag = zbSelf.Instance.journeyThreeHandler.IsZhanlingActive(journeyThreeMidGoodsId);
		bool flag2 = zbSelf.Instance.journeyThreeHandler.IsZhanlingActive(journeyThreeHighGoodsId);
		((VisualElement)m_btnBuyMid).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_btnBuyHigh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		((VisualElement)m_btnReceive).style.left = StyleLength.op_Implicit((flag & flag2) ? 247f : 13f);
		((TextElement)m_btnBuyMid).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(journeyThreeMidGoodsId);
		((TextElement)m_btnBuyHigh).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(journeyThreeHighGoodsId);
	}

	private void OnBuyMid()
	{
		int journeyThreeMidGoodsId = zbSelf.Instance.journeyThreeHandler.journeyThreeMidGoodsId;
		if (!zbSelf.Instance.journeyThreeHandler.IsZhanlingActive(journeyThreeMidGoodsId))
		{
			zbChannel_Pay.OnPay(journeyThreeMidGoodsId);
		}
	}

	private void OnBuyHigh()
	{
		int journeyThreeHighGoodsId = zbSelf.Instance.journeyThreeHandler.journeyThreeHighGoodsId;
		if (!zbSelf.Instance.journeyThreeHandler.IsZhanlingActive(journeyThreeHighGoodsId))
		{
			zbChannel_Pay.OnPay(journeyThreeHighGoodsId);
		}
	}

	private void OnReceive()
	{
		if (zbSelf.Instance.journeyThreeHandler.CanReceiveZhanling())
		{
			zbWsMsgHandler.Instance.Journey_ChapterThreeZhanLingAward_Req(zbServerData.sGameData.JourneyInfo.ChapterThree.ZhanLing.Level);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10391"));
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		UpdateNotify();
		RefreshLeftTime();
	}

	private void UpdateNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyThreeHandler.CanReceiveZhanling()));
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyThreeHandler.GetEndLeftTime();
		if (endLeftTime > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(endLeftTime);
		}
		else
		{
			OnClose();
		}
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	private void OnClose()
	{
		Hide();
	}
}
