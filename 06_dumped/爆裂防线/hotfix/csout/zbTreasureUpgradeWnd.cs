using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTreasureUpgradeWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private ListView m_listView;

	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/treasure/tb_treasureUpgrade.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		m_RootElement.schedule.Execute((Action)delegate
		{
			UpdateListView(zbSelf.Instance.treasureHandler.m_changes);
		}).StartingIn(1L);
	}

	private void UpdateListView(TreasureLevelChange[] items)
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbTreasureUpgradeItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			if (index >= 0 && index < items.Length)
			{
				(element as zbTreasureUpgradeItem).Refresh(items[index]);
			}
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
		ScrollToTop();
	}

	private void ScrollToTop()
	{
		ScrollView scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		if (scrollView != null)
		{
			((VisualElement)m_listView).schedule.Execute((Action)delegate
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				scrollView.scrollOffset = Vector2.zero;
			}).ExecuteLater(1L);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
