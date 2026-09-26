using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBossSubjectZhanlingWnd
{
	private zbBossSubjectWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private Button m_btnGet;

	private Button m_btnActive;

	private Button m_btnView;

	private VisualElement m_notifyWall;

	private VisualElement m_newWall;

	private Label m_lbLv;

	private Label m_lbCount;

	private Label m_lbTitle;

	private VisualElement m_progressVal;

	private SkeletonAnimation skeletonAnimation;

	private List<zbBossZhanlingCsvRecord> m_records;

	public BossSubjectDetail bossSubjectDetail;

	public zbBossSubjectCsvRecord bossSubjectRecord;

	private zbBossSubjectZhanlingItem m_grandPrize;

	private List<int> m_grandIndexes;

	private int m_curGrandIndex = -1;

	private VisualElement m_lbTimeBg;

	private Label m_lbTime;

	public void OnEnable(zbBossSubjectWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_RootElement, "btn_get", (string)null);
		m_btnActive = UQueryExtensions.Q<Button>(m_RootElement, "btn_active", (string)null);
		m_btnView = UQueryExtensions.Q<Button>(m_RootElement, "btn_why", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title2", (string)null);
		m_progressVal = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbTimeBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "lb_timebg", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_grandPrize = new zbBossSubjectZhanlingItem();
		m_grandPrize = UQueryExtensions.Q<zbBossSubjectZhanlingItem>(m_RootElement, "grandPrize", (string)null);
		m_grandPrize.OnEnable(m_wndParent);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action)OnGet, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActive, (Action)OnActive, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnView, (Action)OnView, "", (TrickleDown)0);
		GameObject gameObject = ((Component)m_wndParent.GetMyGameObjectSceneRoot().transform.Find("zhanlingObject").Find("SkeletonAnimation")).gameObject;
		skeletonAnimation = gameObject.GetComponent<SkeletonAnimation>();
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_SuccessOrder_Ack, OnEventUpdate);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_BossSubject_Ack, OnEventUpdate);
		InitListView();
	}

	private void InitListView()
	{
		m_records = zbSelf.Instance.bossSubjectHandler.m_zhanlingRecords;
		zbSelf.Instance.bossSubjectHandler.SetSkeletonAnimation(skeletonAnimation, "bossSubjectZhanlingbg");
		zbSelf.Instance.bossSubjectHandler.SetVisualElementSprite(m_lbTimeBg, "main_time_bg.png");
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbBossSubjectZhanlingItem obj = new zbBossSubjectZhanlingItem();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbBossZhanlingCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbBossZhanlingCsvRecord>;
			(element as zbBossSubjectZhanlingItem).Refresh(list[index]);
		};
		RefreshGrandPrize();
		((BaseVerticalCollectionView)m_listView).itemsSource = m_records;
	}

	public void Refresh()
	{
	}

	public void RefreshCustom(BossSubjectDetail _bossSubjectDetail, zbBossSubjectCsvRecord _bossSubjectRecord)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		bossSubjectDetail = _bossSubjectDetail;
		bossSubjectRecord = _bossSubjectRecord;
		stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath((EResourceType)bossSubjectRecord.zhanLingPreView[0], bossSubjectRecord.zhanLingPreView[1]);
		((TextElement)m_lbTitle).text = resorceIconPath.strName;
		InitBanner();
		UpdateListView();
		bool flag = bossSubjectDetail.ZhanLingInfo.Active == EBoolState.False;
		((VisualElement)m_btnActive).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((TextElement)m_btnActive).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(bossSubjectDetail.ZhanLingInfo.GoodsId);
	}

	private void InitBanner()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		ZhanLingCommonInfo zhanLingInfo = bossSubjectDetail.ZhanLingInfo;
		(zbBossZhanlingCsvRecord, zbBossZhanlingCsvRecord) zhanlingRecord = zbSelf.Instance.bossSubjectHandler.GetZhanlingRecord(zhanLingInfo.Reach);
		float num = 0f;
		float num2 = 0f;
		if (zhanlingRecord.Item1 == null)
		{
			((TextElement)m_lbLv).text = "0";
			num = zhanLingInfo.Reach;
		}
		else
		{
			((TextElement)m_lbLv).text = zhanlingRecord.Item1.lv.ToString();
			num = zhanLingInfo.Reach - zhanlingRecord.Item1.score;
		}
		if (zhanlingRecord.Item2 == null)
		{
			num2 = num;
			((TextElement)m_lbCount).text = "MAX";
		}
		else
		{
			num2 = ((zhanlingRecord.Item1 != null) ? ((float)(zhanlingRecord.Item2.score - zhanlingRecord.Item1.score)) : ((float)zhanlingRecord.Item2.score));
			((TextElement)m_lbCount).text = zhanLingInfo.Reach + "/" + zhanlingRecord.Item2.score;
		}
		float num3 = num / num2;
		m_progressVal.style.width = new StyleLength(Length.Percent(num3 * 100f));
	}

	private void RefreshGrandPrize()
	{
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	public void UpdatePerSecond()
	{
		RefreshTime();
	}

	private void RefreshTime()
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (bossSubjectDetail != null)
		{
			long num = bossSubjectDetail.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
			((VisualElement)m_lbTime).style.color = StyleColor.op_Implicit((num < 86400) ? Color.red : Color.white);
		}
	}

	private void OnGet()
	{
		if (zbSelf.Instance.bossSubjectHandler.HasZhanLingNotify(bossSubjectDetail))
		{
			zbWsMsgHandler.Instance.Operate_BossSubZhanLingAward_Req(bossSubjectDetail.BaseInfo.UUID);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActive()
	{
		if (bossSubjectDetail.ZhanLingInfo.Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(bossSubjectDetail.ZhanLingInfo.GoodsId);
		}
	}

	private void OnView()
	{
		int[] zhanLingPreView = bossSubjectRecord.zhanLingPreView;
		zbUIMgr.Instance.ShowPreview(zbSelf.Instance.dataHandler.GetPreviewType((EResourceType)zhanLingPreView[0], zhanLingPreView[1]), zhanLingPreView[1]);
	}

	public void UpdateAtShow()
	{
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
			zbBossZhanlingCsvRecord record = m_records[index];
			m_grandPrize.Refresh(record);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}

	private void OnEventUpdate(object data)
	{
		bossSubjectDetail = zbSelf.Instance.bossSubjectHandler.GetBossSubjectDetail();
		if (bossSubjectDetail != null)
		{
			RefreshCustom(bossSubjectDetail, bossSubjectRecord);
			RefreshPreGrandPrizeByScroll(needRefresh: true);
		}
	}
}
