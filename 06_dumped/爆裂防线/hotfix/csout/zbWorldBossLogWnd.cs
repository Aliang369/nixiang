using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldBossLogWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private Label m_title;

	private Label m_player;

	private Label m_dmg;

	private Label m_record;

	private ListView m_listView;

	private VisualElement m_NoListView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/worldBoss/tb_worldBossLogItem.uxml");
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_player = UQueryExtensions.Q<Label>(m_RootElement, "lb_player", (string)null);
		m_dmg = UQueryExtensions.Q<Label>(m_RootElement, "lb_dmg", (string)null);
		m_record = UQueryExtensions.Q<Label>(m_RootElement, "lb_record", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 117f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_NoListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(WorldBossHurtRecordDetail[] items)
	{
		UpdateListView(items);
	}

	private void UpdateListView(WorldBossHurtRecordDetail[] items)
	{
		bool flag = items.Length != 0;
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = flag;
		m_NoListView.visible = !flag;
		m_listView.makeItem = () => (VisualElement)(object)new zbWorldBossLogItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbWorldBossLogItem).Init(items[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = items;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
