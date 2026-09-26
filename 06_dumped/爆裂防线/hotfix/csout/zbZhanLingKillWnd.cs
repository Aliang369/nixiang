using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbZhanLingKillWnd : zbBaseWnd
{
	private Label m_lbTime;

	private Label m_lbLv;

	private Label m_lbDesc;

	private Label m_lbCount;

	private VisualElement m_progressVal;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private Button m_btnGet;

	private Button m_btnActive;

	private Label m_lbActive;

	public Button m_btnClose;

	private VisualElement m_notifyGet;

	private zbZhanLingKillItem m_grandPrize;

	private int m_goodsId;

	public zbZhanLingKillTargetCsvRecord[] m_records;

	private int m_maxLv;

	private List<int> m_grandIndexes;

	private int m_curGrandIndex = -1;

	public ZhanLingKillDetail m_data;

	protected override void OnEnable()
	{
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_grandPrize = new zbZhanLingKillItem();
		m_grandPrize = UQueryExtensions.Q<zbZhanLingKillItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_lbActive = UQueryExtensions.Q<Label>(m_RootElement, "lb_active", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_notifyGet = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(53f, -7f), 1f, true);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		InitRecords();
		InitListView();
	}

	private void InitRecords()
	{
		m_goodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.zhanLingKillBaseJsonTable.GetRecord()["goodsId"]);
		((TextElement)m_lbActive).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(m_goodsId).ToString();
		m_records = zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecords().records.Values.ToArray();
		m_maxLv = m_records.Max((zbZhanLingKillTargetCsvRecord r) => r.lv);
	}

	private void InitListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbZhanLingKillItem obj = new zbZhanLingKillItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbZhanLingKillItem).Refresh(m_records[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_records;
	}

	public void RefreshCustom(ZhanLingKillDetail data, bool isFirstIn = true)
	{
		m_data = data;
		if (m_data != null)
		{
			RefreshProgress();
			RefreshGrandPrize();
			RefreshNotify();
			RefreshLeftTime();
			((BaseVerticalCollectionView)m_listView).RefreshItems();
			((BaseVerticalCollectionView)m_listView).ScrollToItemById(Mathf.Min((m_data.ZhanLingInfo.Level > 2) ? (m_data.ZhanLingInfo.Level + 2) : m_data.ZhanLingInfo.Level, m_records.Length - 4));
		}
	}

	private void RefreshProgress()
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLv).text = m_data.ZhanLingInfo.Level.ToString();
		if (m_data.ZhanLingInfo.Level >= m_maxLv)
		{
			((TextElement)m_lbCount).text = "Max";
			m_progressVal.style.width = new StyleLength(Length.Percent(100f));
			return;
		}
		zbZhanLingKillTargetCsvRecord recordByKey = zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecordByKey(m_data.ZhanLingInfo.Level + 1);
		((TextElement)m_lbCount).text = m_data.ZhanLingInfo.Reach + "/" + recordByKey.kill;
		float num = Mathf.Clamp01((float)m_data.ZhanLingInfo.Reach / (float)recordByKey.kill);
		m_progressVal.style.width = new StyleLength(Length.Percent(num * 100f));
	}

	private void RefreshGrandPrize()
	{
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Length; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
		RefreshPreGrandPrizeByScroll(needRefresh: true);
	}

	private void RefreshNotify()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		m_notifyGet.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.operateHandler.HasZhanLingKillNotify(m_data.BaseInfo.UUID)));
		((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_data.ZhanLingInfo.Active != EBoolState.False) ? 1 : 0));
	}

	private void RefreshLeftTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	private void OnGet()
	{
		if (zbSelf.Instance.operateHandler.HasZhanLingKillNotify(m_data?.BaseInfo.UUID))
		{
			zbWsMsgHandler.Instance.Operate_ZhanLingKillAward_Req(m_data.BaseInfo.UUID, m_data.ZhanLingInfo.Level);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActive()
	{
		ZhanLingKillDetail data = m_data;
		if (data != null && data.ZhanLingInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(m_goodsId);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		((BaseVerticalCollectionView)m_listView).ScrollToItem(0);
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshPreGrandPrizeByScroll();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void RefreshPreGrandPrizeByScroll(bool needRefresh = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.y / ((BaseVerticalCollectionView)m_listView).fixedItemHeight);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] >= num + 6)
			{
				num2 = i;
				break;
			}
		}
		if (num2 != -1 && ((num2 != m_curGrandIndex) | needRefresh))
		{
			m_curGrandIndex = num2;
			int num3 = m_grandIndexes[m_curGrandIndex];
			zbZhanLingKillTargetCsvRecord record = m_records[num3];
			m_grandPrize.Refresh(record);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}

	private void OnEventUpdate(object data)
	{
		if (m_data != null)
		{
			ZhanLingKillDetail zhanLingKillDetailByUUID = zbSelf.Instance.operateHandler.GetZhanLingKillDetailByUUID(m_data?.BaseInfo.UUID);
			RefreshCustom(zhanLingKillDetailByUUID, isFirstIn: false);
		}
	}
}
