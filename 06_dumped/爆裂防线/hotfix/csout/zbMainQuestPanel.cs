using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestPanel
{
	private zbBaseWnd m_wndParent;

	private Button m_btnReceivce;

	private Button m_btnActivate;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private Label m_lbTitle;

	private Label m_lbTitle1;

	private Label m_lbTitle2;

	private VisualElement m_nodeMainQuestLevel;

	private Label m_lbCurrentLv;

	private VisualElement m_btnRedDot;

	private VisualElement m_preRedDot;

	private VisualElement m_nextRedDot;

	private Label m_lbBonus;

	private zbMainQuestItem m_grandPrize;

	private Button m_btnPre;

	private Button m_btnNext;

	private int ItemsCount;

	public int m_passStage = 1;

	private List<zbZhanLingMainCsvRecord> m_records;

	private int m_currentGrandIndex = -1;

	private List<int> m_grandIndexes;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Invalid comparison between Unknown and I4
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_btnReceivce = UQueryExtensions.Q<Button>(root, "btn_receive", (string)null);
		m_btnRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnReceivce, "red_dot", (string)null);
		m_btnActivate = UQueryExtensions.Q<Button>(root, "btn_activate", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(root, (string)null, (string)null);
		m_lbBonus = UQueryExtensions.Q<Label>(root, "lb_bonus", (string)null);
		EnableListView();
		m_btnPre = UQueryExtensions.Q<Button>(root, "btn_pre", (string)null);
		m_preRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnPre, "red_dot", (string)null);
		m_btnNext = UQueryExtensions.Q<Button>(root, "btn_next", (string)null);
		m_nextRedDot = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnNext, "red_dot", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(root, "lb_title", (string)null);
		m_lbTitle1 = UQueryExtensions.Q<Label>(root, "lb_title1", (string)null);
		m_lbTitle2 = UQueryExtensions.Q<Label>(root, "lb_title2", (string)null);
		m_nodeMainQuestLevel = UQueryExtensions.Q<VisualElement>(root, "mainQuestLevel", (string)null);
		if ((int)Application.platform == 8)
		{
			m_nodeMainQuestLevel.style.top = StyleLength.op_Implicit(180f);
		}
		m_lbCurrentLv = UQueryExtensions.Q<Label>(root, "lb_currentLv", (string)null);
		m_grandPrize = new zbMainQuestItem();
		m_grandPrize = UQueryExtensions.Q<zbMainQuestItem>(root, "grandPrize", (string)null);
		m_grandPrize.OnEnable(wndParent);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPre, (Action)OnPre, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNext, (Action)OnNext, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReceivce, (Action)OnReceive, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnActivate, (Action)OnActivate, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		((TextElement)m_lbCurrentLv).text = string.Format(zbUtil.Language("lang.10731"), zbServerData.sGameData.MainQuestInfo.MainLevel);
		m_passStage = zbSelf.Instance.passportHandler.GetCurrentStage();
		RefreshGrandIndexes();
		InitGrandPrize();
		InitBtns();
		InitTitle();
		UpdateListView();
	}

	private void EnableListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbMainQuestItem obj = new zbMainQuestItem();
			obj.OnEnable(m_wndParent);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<zbZhanLingMainCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbZhanLingMainCsvRecord>;
			(element as zbMainQuestItem).Init(activeDetail: zbSelf.Instance.passportHandler.GetZhanlingMainActiveDetailByStage(m_passStage), data: list[index], passStage: m_passStage);
		};
	}

	private void InitTitle()
	{
		string text = "";
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage);
		text = $"Day{zhanlingMainRecordsByStage[0].mainLevel}-{zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel}";
		((TextElement)m_lbTitle).text = text;
		string text2 = "";
		switch (m_passStage)
		{
		case 1:
			text2 = "超值返利4529%";
			break;
		case 2:
			text2 = "超值返利4813%";
			break;
		case 3:
			text2 = "超值返利4667%";
			break;
		case 4:
			text2 = "超值返利3756%";
			break;
		}
		((TextElement)m_lbBonus).text = text2;
	}

	private void InitGrandPrize()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(m_scrollView.scrollOffset.y / 110f);
		int num2 = -1;
		for (int i = 0; i < m_grandIndexes.Count; i++)
		{
			if (m_grandIndexes[i] > num + 5)
			{
				num2 = i;
				break;
			}
		}
		if (num2 != -1 && num2 != m_currentGrandIndex)
		{
			m_currentGrandIndex = num2;
			ZhanLingMainActiveDetail zhanlingMainActiveDetailByStage = zbSelf.Instance.passportHandler.GetZhanlingMainActiveDetailByStage(m_passStage);
			zbZhanLingMainCsvRecord data = m_records[m_grandIndexes[m_currentGrandIndex]];
			m_grandPrize.Init(data, zhanlingMainActiveDetailByStage, m_passStage);
			m_grandPrize.SetMaskVisible(visible: false);
		}
	}

	private void InitBtns()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		m_btnRedDot.visible = zbSelf.Instance.passportHandler.HasRedPointByPassStage(m_passStage);
		bool flag = m_passStage > 1;
		if (flag)
		{
			m_preRedDot.visible = zbSelf.Instance.passportHandler.HasRedPointByPassStage(m_passStage - 1);
		}
		((VisualElement)m_btnPre).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		bool flag2 = m_passStage < zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo.ActiveList.Count && zbSelf.Instance.passportHandler.IsCurrentMainZhanlingDone(m_passStage);
		if (flag2)
		{
			m_nextRedDot.visible = zbSelf.Instance.passportHandler.HasRedPointByPassStage(m_passStage + 1);
		}
		((VisualElement)m_btnNext).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		bool flag3 = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo.ActiveList[m_passStage - 1].Active == EBoolState.False;
		((VisualElement)m_btnActivate).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
		((TextElement)m_btnActivate).text = zbSelf.Instance.vipHandler.GetPriceByGoodsId(zbSelf.Instance.passportHandler.GetZhanlingMainActiveDetailByStage(m_passStage).GoodsId);
	}

	private void UpdateListView()
	{
		List<zbZhanLingMainCsvRecord> list = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage) ?? new List<zbZhanLingMainCsvRecord>();
		((BaseVerticalCollectionView)m_listView).itemsSource = list;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		if (list.Count > 0)
		{
			int currentItemIndex = zbSelf.Instance.passportHandler.GetCurrentItemIndex(m_passStage);
			((BaseVerticalCollectionView)m_listView).ScrollToItem(currentItemIndex);
		}
	}

	private void OnReceive()
	{
		if (zbSelf.Instance.passportHandler.HasRedPointByPassStage(m_passStage))
		{
			ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo;
			List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage);
			int mainLevel = Mathf.Min(zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel, zhanLingMainInfo.Reach);
			zbWsMsgHandler.Instance.RechargeActive_ZhanLingMainAward_Req(mainLevel);
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10426"));
		}
	}

	private void OnActivate()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo.ActiveList[m_passStage - 1].Active == EBoolState.False)
		{
			zbChannel_Pay.OnPay(zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo.ActiveList[m_passStage - 1].GoodsId);
		}
	}

	public void UpdateMainQuestPanel()
	{
		if (!(((BaseVerticalCollectionView)m_listView).fixedItemHeight <= 0f))
		{
			InitGrandPrize();
		}
	}

	private void RefreshGrandIndexes()
	{
		m_records = zbSelf.Instance.passportHandler.GetZhanlingMainRecordsByStage(m_passStage);
		m_grandIndexes = new List<int>();
		for (int i = 0; i < m_records.Count; i++)
		{
			if (m_records[i].isGrand > 0)
			{
				m_grandIndexes.Add(i);
			}
		}
	}

	private void OnPre()
	{
		m_passStage--;
		RefreshGrandIndexes();
		InitGrandPrize();
		InitBtns();
		InitTitle();
		UpdateListView();
	}

	private void OnNext()
	{
		m_passStage++;
		RefreshGrandIndexes();
		InitGrandPrize();
		InitBtns();
		InitTitle();
		UpdateListView();
	}
}
