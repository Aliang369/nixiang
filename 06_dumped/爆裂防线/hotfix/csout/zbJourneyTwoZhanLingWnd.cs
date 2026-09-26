using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbJourneyTwoZhanLingWnd : zbBaseWnd
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

	private Label[] m_lbTrucks = (Label[])(object)new Label[5];

	private Button m_btnClose;

	private VisualElement m_notify;

	private bool m_isFristIn = true;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		((TextElement)m_lbTitle).text = zbUtil.Language("lang.11102");
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
		for (int i = 0; i < 5; i++)
		{
			m_lbTrucks[i] = UQueryExtensions.Q<Label>(m_RootElement, $"truck{i}", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_lbTrucks[i], (Action<ClickEvent>)delegate
			{
				OnTrucks(tempIndex);
			}, "", (TrickleDown)0);
		}
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
		InitRecord();
	}

	private void InitRecord()
	{
		for (int i = 0; i < 5; i++)
		{
			zbTruckQualityCsvRecord recordByKey = zbDataConfig.Instance.truckQualityCsvTable.GetRecordByKey(i + 1);
			if (recordByKey != null)
			{
				((TextElement)m_lbTrucks[i]).text = "+" + recordByKey.points;
			}
		}
	}

	public override void Refresh()
	{
		if (m_isFristIn)
		{
			m_isFristIn = false;
			zbWsMsgHandler.Instance.Journey_Data_Req();
		}
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
			zbJourneyTwoZhanLingItem obj = new zbJourneyTwoZhanLingItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbChapterTwoZhanLingCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbChapterTwoZhanLingCsvRecord>;
			(element as zbJourneyTwoZhanLingItem).Init(list[index]);
		};
	}

	private void UpdateListView()
	{
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing;
		List<zbChapterTwoZhanLingCsvRecord> list = zbDataConfig.Instance.chapterTwoZhanLingCsvTable.GetRecords().records.Values.ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		((BaseVerticalCollectionView)m_listView).ScrollToItemById(Mathf.Min((zhanLing.Level > 2) ? (zhanLing.Level + 2) : zhanLing.Level, list.Count - 4));
	}

	private void InitBanner()
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing;
		Dictionary<int, zbChapterTwoZhanLingCsvRecord> records = zbDataConfig.Instance.chapterTwoZhanLingCsvTable.GetRecords().records;
		zbChapterTwoZhanLingCsvRecord recordByKey = zbDataConfig.Instance.chapterTwoZhanLingCsvTable.GetRecordByKey(zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing.Level + 1);
		bool flag = zhanLing.Level == records.Count || recordByKey == null;
		((TextElement)m_lbLv).text = zhanLing.Level.ToString();
		((TextElement)m_lbCount).text = (flag ? "Max" : (zhanLing.Reach + "/" + recordByKey.count));
		float num = (flag ? 1f : Mathf.Clamp01((float)zhanLing.Reach / (float)recordByKey.count));
		m_progressVal.style.width = new StyleLength(Length.Percent(num * 100f));
	}

	private void InitBtns()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		int journeyTwoMidGoodsId = zbSelf.Instance.journeyTwoHandler.journeyTwoMidGoodsId;
		int journeyTwoHighGoodsId = zbSelf.Instance.journeyTwoHandler.journeyTwoHighGoodsId;
		bool flag = zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(journeyTwoMidGoodsId);
		bool flag2 = zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(journeyTwoHighGoodsId);
		((VisualElement)m_btnBuyMid).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((VisualElement)m_btnBuyHigh).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag2 ? 1 : 0));
		((VisualElement)m_btnReceive).style.left = StyleLength.op_Implicit((flag & flag2) ? 247f : 13f);
		((TextElement)m_btnBuyMid).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(journeyTwoMidGoodsId);
		((TextElement)m_btnBuyHigh).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(journeyTwoHighGoodsId);
	}

	private void OnBuyMid()
	{
		int journeyTwoMidGoodsId = zbSelf.Instance.journeyTwoHandler.journeyTwoMidGoodsId;
		if (!zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(journeyTwoMidGoodsId))
		{
			zbChannel_Pay.OnPay(journeyTwoMidGoodsId);
		}
	}

	private void OnBuyHigh()
	{
		int journeyTwoHighGoodsId = zbSelf.Instance.journeyTwoHandler.journeyTwoHighGoodsId;
		if (!zbSelf.Instance.journeyTwoHandler.IsZhanlingActive(journeyTwoHighGoodsId))
		{
			zbChannel_Pay.OnPay(journeyTwoHighGoodsId);
		}
	}

	private void OnReceive()
	{
		if (zbSelf.Instance.journeyTwoHandler.CanReceiveZhanling())
		{
			zbWsMsgHandler.Instance.Journey_ChapterTwoZhanLingAward_Req(zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing.Level);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10391"));
		}
	}

	private void OnTrucks(int index)
	{
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(index + 1);
		zbTruckQualityCsvRecord recordByKey2 = zbDataConfig.Instance.truckQualityCsvTable.GetRecordByKey(index + 1);
		if (recordByKey != null && recordByKey2 != null)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11137"), recordByKey.name, recordByKey2.points));
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
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.journeyTwoHandler.CanReceiveZhanling()));
	}

	private void RefreshLeftTime()
	{
		long endLeftTime = zbSelf.Instance.journeyTwoHandler.GetEndLeftTime();
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
		m_isFristIn = true;
		Hide();
	}
}
