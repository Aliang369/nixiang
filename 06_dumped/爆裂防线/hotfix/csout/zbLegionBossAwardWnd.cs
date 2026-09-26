using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class zbLegionBossAwardWnd : zbBaseWnd
{
	public class BossChallengeInfo
	{
		public string name;

		public int[][] awardList;

		public int id;
	}

	private VisualTreeAsset m_itemTA;

	private Button m_btnBack;

	private Label m_lbTitle;

	private ListView m_listView;

	private Label m_lbInfo;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_LegionAwardItem.uxml");
		AddDark();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbInfo = UQueryExtensions.Q<Label>(m_RootElement, "lb_info", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 95f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		InitializeListView();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_BossAward, OnUpdateEventLegionBossAward);
	}

	private void InitializeListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionBossAwardItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbLegionBossAwardItem obj = element as zbLegionBossAwardItem;
			BossChallengeInfo detail = ((BaseVerticalCollectionView)m_listView).itemsSource[index] as BossChallengeInfo;
			obj.Init(detail);
		};
	}

	public override void Refresh()
	{
		UpdateListView();
	}

	private void UpdateListView()
	{
		BossChallengeInfo[] itemsSource = zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecords().records.Select((KeyValuePair<int, zbLegionBossChallengeCsvRecord> r) => new BossChallengeInfo
		{
			awardList = r.Value.awardList.ToArray(),
			name = r.Value.name,
			id = r.Value.bossid
		}).ToArray();
		((BaseVerticalCollectionView)m_listView).itemsSource = itemsSource;
	}

	private void OnUpdateEventLegionBossAward(object data)
	{
		zbWsMsgHandler.Instance.Legion_Data_Req();
		Refresh();
	}

	private void OnClose()
	{
		Hide();
	}
}
