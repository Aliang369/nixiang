using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWallConvertWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private ListView m_listView;

	private Button m_btnConfirm;

	private WallDetail[] m_details;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/wall/tb_wallConvert.uxml");
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 158f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(WallDetail[] _wallDetails)
	{
		m_details = _wallDetails;
		UpdateListView();
	}

	public void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbWallConvertItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWallConvertItem).Init(m_details[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_details;
	}

	private void OnClose()
	{
		if (m_details.Length != 0)
		{
			List<int> list = new List<int>();
			WallDetail[] details = m_details;
			foreach (WallDetail wallDetail in details)
			{
				list.Add(wallDetail.WallId);
			}
			zbWsMsgHandler.Instance.Wall_Convert_Req(list.ToArray());
		}
		Hide();
	}
}
