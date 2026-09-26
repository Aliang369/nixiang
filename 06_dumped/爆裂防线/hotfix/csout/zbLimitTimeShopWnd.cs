using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLimitTimeShopWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	private Button m_btnClose;

	private List<LimitTimeShopItemDetail> m_ListViewDatas = new List<LimitTimeShopItemDetail>();

	private LimitTimeShopDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_limitTimeShop.uxml");
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Diamond });
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OnUpdateEvent);
	}

	public void RefreshCustom(LimitTimeShopDetail data)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		m_data = data;
		RefreshLeftTime();
		UpdateListView();
		zbOperateCsvRecord recordByKey = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(8);
		if (recordByKey != null)
		{
			m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.banner));
			((TextElement)m_lbTitle).text = recordByKey.name;
		}
	}

	public void UpdateListView()
	{
		m_ListViewDatas.Clear();
		((VisualElement)m_listView).Clear();
		foreach (LimitTimeShopItemDetail item in m_data.ItemList)
		{
			m_ListViewDatas.Add(item);
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbLimitTimeShopItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLimitTimeShopItem).Refresh(m_ListViewDatas[index], m_data.BaseInfo.UUID);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_ListViewDatas;
	}

	private void OnClose()
	{
		Hide();
	}

	private void RefreshLeftTime()
	{
		if (m_data != null)
		{
			long seconds = m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(seconds);
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnUpdateEvent(object data)
	{
		if (m_data != null)
		{
			LimitTimeShopDetail timeShopInfoByUUID = zbSelf.Instance.operateHandler.GetTimeShopInfoByUUID(m_data.BaseInfo.UUID);
			RefreshCustom(timeShopInfoByUUID);
		}
	}
}
