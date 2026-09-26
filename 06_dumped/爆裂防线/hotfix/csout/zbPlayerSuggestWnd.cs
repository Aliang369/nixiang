using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbPlayerSuggestWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnAllClose;

	private Button m_btnClose;

	private ListView m_listView;

	private PlayerSuggestDetail[] m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/comm/tb_playerSuggest.uxml");
		m_btnAllClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_allClose", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(PlayerSuggestDetail[] playerSuggestDetails)
	{
		m_data = playerSuggestDetails;
		UpdateListView();
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbPlayerSuggestItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbPlayerSuggestItem).Init(m_data[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_data;
	}

	private void OnClose()
	{
		Hide();
	}
}
