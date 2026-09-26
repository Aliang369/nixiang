using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonAirdropListWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private ListView m_listView;

	public SeasonAirdropInfo[] m_listItemDatas;

	private ESeasonAirdropQuality m_quality;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		InitListView();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_SeasonAirdrop, OnEventUpdate);
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbSeasonAirdropItem(this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbSeasonAirdropItem).Refresh(m_listItemDatas[index]);
		};
	}

	public void RefreshCustom(ESeasonAirdropQuality quality)
	{
		m_quality = quality;
		if (zbSelf.Instance.seasonHandler.seasonAirdropListAck == null)
		{
			return;
		}
		m_listItemDatas = (from x in (IEnumerable<SeasonAirdropInfo>)zbSelf.Instance.seasonHandler.seasonAirdropListAck.List
			orderby x.Quality, x.DropId
			select x).ToArray();
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		int index = Array.FindIndex(m_listItemDatas, (SeasonAirdropInfo x) => x.Quality == quality);
		if (index < 0)
		{
			return;
		}
		m_RootElement.schedule.Execute((Action)delegate
		{
			((BaseVerticalCollectionView)m_listView).ScrollToItem(index);
			m_RootElement.schedule.Execute((Action)delegate
			{
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				ScrollView obj = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
				obj.scrollOffset = new Vector2(obj.scrollOffset.x, (float)index * ((BaseVerticalCollectionView)m_listView).fixedItemHeight);
			});
		});
	}

	public void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		RefreshCustom(m_quality);
	}
}
