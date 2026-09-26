using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbArenaRecordWnd : zbBaseWnd
{
	private SpriteRenderer m_spriteFoothold;

	private zbPlayerAvatar m_spineAvatar;

	private zbArenaBottom m_bottomPanel;

	private ListView m_listView;

	private VisualElement m_NoListView;

	private Button m_btnClose;

	private VisualTreeAsset m_recordItemTA;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_recordItemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/arena/tb_arenaRecord.uxml");
		m_spriteFoothold = ((Component)m_GameObjectSceneRoot.transform.Find("bottomBg").Find("foothold")).GetComponent<SpriteRenderer>();
		m_spineAvatar = ((Component)((Component)m_spriteFoothold).transform.Find("avatar")).GetComponent<zbPlayerAvatar>();
		m_bottomPanel = new zbArenaBottom();
		m_bottomPanel = UQueryExtensions.Q<zbArenaBottom>(m_RootElement, "arenaBottom", (string)null);
		m_bottomPanel.OnEnable(this, m_spriteFoothold, m_spineAvatar);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_NoListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "list_no", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(ArenaRecordDetail[] items)
	{
		m_bottomPanel.Refresh();
		UpdateListView(items);
	}

	private void UpdateListView(ArenaRecordDetail[] items)
	{
		bool flag = items.Length != 0;
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = flag;
		m_NoListView.visible = !flag;
		m_listView.makeItem = () => (VisualElement)(object)new zbArenaRecordItem(m_recordItemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbArenaRecordItem).Init(items[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
