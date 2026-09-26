using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbFuncPreviewWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle0;

	private Label m_lbTitle1;

	private ListView m_listView;

	private ScrollView m_scrollView;

	private PlayerSuggestDetail[] m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle0 = UQueryExtensions.Q<Label>(m_RootElement, "lb_title0", (string)null);
		m_lbTitle1 = UQueryExtensions.Q<Label>(m_RootElement, "lb_title1", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		EnableListView();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	private void EnableListView()
	{
		m_scrollView = UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 148f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbFuncPreviewItem obj = new zbFuncPreviewItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbFuncPreviewItem obj = element as zbFuncPreviewItem;
			List<zbFunctionUnlockCsvRecord> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<zbFunctionUnlockCsvRecord>;
			obj.Init(list[index]);
		};
	}

	public void RefreshCustom()
	{
		RefreshTm();
		UpdateListView();
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).itemsSource = zbSelf.Instance.funcUnLockHandler.GetRecordsByPeerageId(zbServerData.sGameData.PeerageInfo.PeerageId);
		((BaseVerticalCollectionView)m_listView).RefreshItems();
		if (((BaseVerticalCollectionView)m_listView).itemsSource != null && ((BaseVerticalCollectionView)m_listView).itemsSource.Count > 0)
		{
			((BaseVerticalCollectionView)m_listView).ScrollToItem(0);
		}
	}

	private void RefreshTm()
	{
		if (m_listView == null || ((VisualElement)m_scrollView).contentContainer == null)
		{
			return;
		}
		foreach (zbFuncPreviewItem item in ((VisualElement)m_scrollView).contentContainer.Children())
		{
			if (item.HasLeftTime())
			{
				item.Update();
			}
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTm();
	}

	private void OnClose()
	{
		Hide();
	}
}
