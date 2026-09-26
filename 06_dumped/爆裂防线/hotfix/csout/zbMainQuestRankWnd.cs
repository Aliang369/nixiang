using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbMainQuestRankWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_listView;

	private List<RankDetail> m_listItemDatas = new List<RankDetail>();

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/rank/tb_mainQuestRank.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitListView();
		AddDark();
	}

	public void RefreshCustom(RankDetail[] rankList)
	{
		m_listItemDatas = rankList.ToList();
		UpdateListView();
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbMainQuestRankItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbMainQuestRankItem).Refresh(m_listItemDatas[index]);
		};
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
		int num = m_listItemDatas.FindIndex((RankDetail x) => x.RoleBaseInfo.RoleId == zbServerData.sGameData.RoleBaseInfo.RoleId);
		((BaseVerticalCollectionView)m_listView).ScrollToItem(num);
	}

	private void OnClose()
	{
		Hide();
	}
}
