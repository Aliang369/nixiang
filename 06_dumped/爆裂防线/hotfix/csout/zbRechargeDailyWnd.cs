using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRechargeDailyWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private Button m_btnTip;

	private Label m_lbTipDesc;

	private RechargeDailyDetail m_data;

	private zbOperateCsvRecord m_record;

	private List<RechargeTargetDetail> m_ListViewDatas = new List<RechargeTargetDetail>();

	private AutoHideTipController m_tipController;

	protected override void OnEnable()
	{
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Invalid comparison between Unknown and I4
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_rechargeDaily.uxml");
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 150f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_tipDesc", (string)null);
		((VisualElement)m_btnTip).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_lbTipDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_tipDesc", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnEventUpdate);
		m_tipController = new AutoHideTipController(m_uiEvent, m_btnTip, m_lbTipDesc);
		m_tipController.HideImmediate();
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Gold
		});
		if ((int)Application.platform == 8)
		{
			((VisualElement)m_btnTip).style.top = StyleLength.op_Implicit(160f);
		}
	}

	public void RefreshCustom(string uuid, zbOperateCsvRecord record)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!(uuid == ""))
		{
			m_data = zbSelf.Instance.operateHandler.GetRechargeDailyByUUID(uuid);
			m_record = record;
			if (m_data != null)
			{
				m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.banner));
				((TextElement)m_lbTitle).text = m_record.name;
				RefreshList();
				RefreshTime();
			}
		}
	}

	public void RefreshList()
	{
		m_ListViewDatas.Clear();
		foreach (RechargeTargetDetail target in m_data.TargetList)
		{
			m_ListViewDatas.Add(target);
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbRechargeDailyItem(this, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbRechargeDailyItem).Refresh(m_ListViewDatas[index], m_data.BaseInfo.UUID);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_ListViewDatas;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_data.BaseInfo.UUID, m_record);
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	protected override void RefreshWhenAutoVisible()
	{
		base.RefreshWhenAutoVisible();
		zbWsMsgHandler.Instance.Operate_Data_Req();
	}
}
