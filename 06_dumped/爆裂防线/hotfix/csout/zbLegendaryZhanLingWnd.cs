using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegendaryZhanLingWnd : zbBaseWnd
{
	private ListView m_listView;

	private ScrollView m_scrollView;

	private Label m_lbLevel;

	private Button m_btnGet;

	private Button m_btnActive;

	private Button m_btnWall;

	public Button m_btnClose;

	private VisualElement m_notifyGet;

	private VisualElement m_notifyPre;

	private VisualElement m_notifyNext;

	private VisualElement m_notifyWall;

	private VisualElement m_newWall;

	private zbLegendaryZhanLingItem m_grandPrize;

	private Button m_btnPre;

	private Button m_btnNext;

	public int m_passStage = 1;

	private List<zbZhanLingLegendaryCsvRecord> m_records;

	private List<int> m_grandIndexes;

	private int m_curGrandIndex = -1;

	protected override void OnEnable()
	{
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_btnPre = UQueryExtensions.Q<Button>(m_RootElement, "btn_pre", (string)null);
		m_btnNext = UQueryExtensions.Q<Button>(m_RootElement, "btn_next", (string)null);
		m_grandPrize = new zbLegendaryZhanLingItem();
		m_grandPrize = UQueryExtensions.Q<zbLegendaryZhanLingItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(this);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_btnWall = UQueryExtensions.Q<Button>(m_RootElement, "btn_wall", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPre, (Action)OnPre, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNext, (Action)OnNext, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWall, (Action<ClickEvent>)OnWall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_notifyGet = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnGet, (Vector2?)new Vector2(53f, -7f), 1f, true);
		m_notifyPre = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnPre, (Vector2?)new Vector2(10f, 60f), 1f, true);
		m_notifyNext = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnNext, (Vector2?)new Vector2(10f, 10f), 1f, true);
		m_notifyWall = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnWall);
		m_newWall = zbUtil.CreateNewTag((VisualElement)(object)m_btnWall, (Vector2?)new Vector2(-8f, -5f), 0.9f);
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Tili,
			EMoneyType.Legendary
		});
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegendaryInfo, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WallInfo, delegate
		{
			RefreshWall();
		});
		InitListView();
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
			zbLegendaryZhanLingItem obj = new zbLegendaryZhanLingItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbZhanLingLegendaryCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbZhanLingLegendaryCsvRecord>;
			(element as zbLegendaryZhanLingItem).Refresh(list[index], m_passStage);
		};
	}

	public override void Refresh()
	{
		m_passStage = zbSelf.Instance.legendaryHandler.GetCurrentStage();
		RefreshDetail();
		RefreshWall();
	}

	private void RefreshDetail()
	{
		RefreshGrandPrize();
		RefreshBtns();
		UpdateListView();
	}

	private void RefreshWall()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		m_notifyWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasRedPoint()));
		m_newWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.wallHandler.HasRedPoint() || !zbSelf.Instance.wallHandler.HasNewPoint()) ? 1 : 0));
	}

	private void RefreshGrandPrize()
	{
		m_records = zbSelf.Instance.legendaryHandler.GetZhanlingRecordsByStage(m_passStage);
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
		zbZhanLingLegendaryCsvRecord zbZhanLingLegendaryCsvRecord2 = m_records[m_records.Count - 1];
		((TextElement)m_lbLevel).text = string.Format(zbUtil.Language("lang.10842"), m_records[0].lv) + "-" + zbZhanLingLegendaryCsvRecord2.lv;
		RefreshPreGrandPrizeByScroll(needRefresh: true);
	}

	private void RefreshBtns()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		m_notifyGet.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasRedPointByPassStage(m_passStage)));
		bool flag = m_passStage > 1;
		if (flag)
		{
			m_notifyPre.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasRedPointByPassStage(m_passStage - 1)));
		}
		((VisualElement)m_btnPre).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = m_passStage < zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.ActiveList.Count && zbSelf.Instance.legendaryHandler.IsCurrentZhanlingDone(m_passStage);
		if (flag2)
		{
			m_notifyNext.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legendaryHandler.HasRedPointByPassStage(m_passStage + 1)));
		}
		((VisualElement)m_btnNext).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		bool flag3 = zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.ActiveList[m_passStage - 1].Active == EBoolState.False;
		((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		((TextElement)m_btnActive).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(zbSelf.Instance.legendaryHandler.GetZhanlingActiveDetailByStage(m_passStage).GoodsId);
	}

	private void UpdateListView()
	{
		List<zbZhanLingLegendaryCsvRecord> list = zbSelf.Instance.legendaryHandler.GetZhanlingRecordsByStage(m_passStage) ?? new List<zbZhanLingLegendaryCsvRecord>();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		if (list.Count > 0)
		{
			int currentItemIndex = zbSelf.Instance.legendaryHandler.GetCurrentItemIndex(m_passStage);
			((BaseVerticalCollectionView)m_listView).ScrollToItem(currentItemIndex);
		}
	}

	private void OnPre()
	{
		m_passStage--;
		RefreshDetail();
	}

	private void OnNext()
	{
		m_passStage++;
		RefreshDetail();
	}

	private void OnGet()
	{
		if (zbSelf.Instance.legendaryHandler.HasZhanLingNotify())
		{
			List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = zbSelf.Instance.legendaryHandler.GetZhanlingRecordsByStage(m_passStage);
			int passLevel = Mathf.Min(zhanlingRecordsByStage[zhanlingRecordsByStage.Count - 1].lv, zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.Reach);
			zbWsMsgHandler.Instance.RechargeActive_ZhanLingLegendary_Req(passLevel);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActive()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.ActiveList[m_passStage - 1].Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo.ActiveList[m_passStage - 1].GoodsId);
		}
	}

	public void OnWall(ClickEvent evt)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		zbLinkHelper.OpenWall();
		m_newWall.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		RefreshPreGrandPrizeByScroll();
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
			int index = m_grandIndexes[m_curGrandIndex];
			zbZhanLingLegendaryCsvRecord record = m_records[index];
			m_grandPrize.Refresh(record, m_passStage);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
