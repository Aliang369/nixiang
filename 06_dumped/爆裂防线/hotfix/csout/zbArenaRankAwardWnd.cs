using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbArenaRankAwardWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_listView;

	private Label m_lbTime;

	private Label m_lbMyRank;

	private Label m_lbNoRank;

	private VisualElement m_awardPreview;

	private List<zbArenaDailyAwardCsvRecord> m_listItemDatas = new List<zbArenaDailyAwardCsvRecord>();

	private zbItem[] m_awards = new zbItem[5];

	private VisualTreeAsset m_rankAwardItemTA;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_rankAwardItemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossPeak/tb_crossPeakRankItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_lbMyRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_rank", (string)null);
		m_lbNoRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_norank", (string)null);
		m_awardPreview = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitListView();
		AddDark();
		if (m_listItemDatas.Count() <= 0)
		{
			zbArenaDailyAwardCsvRecord[] records = zbDataConfig.Instance.arenaDailyAwardCsvTable.GetRecords().records;
			foreach (zbArenaDailyAwardCsvRecord item in records)
			{
				m_listItemDatas.Add(item);
			}
		}
		for (int j = 0; j < 5; j++)
		{
			m_awards[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_awards[j].OnEnable(this);
		}
	}

	public void RefreshCustom()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		UpdateListView();
		int rank = zbServerData.sGameData.ArenaInfo.Rank;
		int dailyArenaCoin = zbSelf.Instance.arenaHandler.GetDailyArenaCoin(rank);
		if (dailyArenaCoin >= 0)
		{
			((TextElement)m_lbMyRank).text = rank.ToString();
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			for (int i = 0; i < 5; i++)
			{
				bool flag = i < 1;
				((VisualElement)m_awards[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (flag)
				{
					m_awards[i].Refresh(new zbItemData(EResourceType.Money, 14L, dailyArenaCoin));
				}
			}
		}
		else
		{
			((TextElement)m_lbMyRank).text = "";
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		UpdatePerSecond();
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbArenaRankAwardItem(m_rankAwardItemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbArenaRankAwardItem).Refresh(m_listItemDatas[index]);
		};
	}

	private void UpdateListView()
	{
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = m_listItemDatas;
		((BaseVerticalCollectionView)m_listView).ScrollToItem(1);
	}

	private void RefreshTime()
	{
		((TextElement)m_lbTime).text = string.Format(zbUtil.Language("lang.10490"), zbSelf.Instance.timeHandler.GetArenaRefreshTimeStr());
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnClose()
	{
		Hide();
	}
}
