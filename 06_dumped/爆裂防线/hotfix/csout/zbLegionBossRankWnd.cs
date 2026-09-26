using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionBossRankWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private Label m_title;

	private Label m_lbRank;

	private Label m_lbPlayer;

	private Label m_lbPosition;

	private Label m_lbDmg;

	private Label m_lbTime;

	private ListView m_listView;

	private VisualElement m_NoListView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_legionRankItem.uxml");
		AddDark();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_rank", (string)null);
		m_lbPlayer = UQueryExtensions.Q<Label>(m_RootElement, "lb_player", (string)null);
		m_lbPosition = UQueryExtensions.Q<Label>(m_RootElement, "lb_position", (string)null);
		m_lbDmg = UQueryExtensions.Q<Label>(m_RootElement, "lb_dmg", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 110f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_NoListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(RankDetail[] rankList)
	{
		UpdateListView(rankList);
	}

	private void UpdateListView(RankDetail[] rankList)
	{
		bool flag = rankList.Length != 0;
		((VisualElement)m_listView).Clear();
		((VisualElement)m_listView).visible = flag;
		m_NoListView.visible = !flag;
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionBossRankItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLegionBossRankItem).Init(rankList[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = rankList;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
