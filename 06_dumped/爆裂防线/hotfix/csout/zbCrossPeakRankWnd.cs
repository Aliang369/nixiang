using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCrossPeakRankWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_listView;

	private Label m_lbTime;

	private Label m_lbMyRank;

	private Label m_lbNoRank;

	private List<zbPeakRankAwardCsvRecord> m_listItemDatas = new List<zbPeakRankAwardCsvRecord>();

	private zbItem[] m_awards = new zbItem[5];

	private VisualElement m_awardBg;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/crossPeak/tb_crossPeakRankItem.uxml");
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		m_lbMyRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_rank", (string)null);
		m_lbNoRank = UQueryExtensions.Q<Label>(m_RootElement, "lb_norank", (string)null);
		m_awardBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "awardPreview", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitListView();
		AddDark();
		if (m_listItemDatas.Count() <= 0)
		{
			for (int i = 0; i < 50; i++)
			{
				zbPeakRankAwardCsvRecord recordByKey = zbDataConfig.Instance.peakRankAwardCsvTable.GetRecordByKey(i + 1);
				if (recordByKey.showAward == 1)
				{
					m_listItemDatas.Add(recordByKey);
				}
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
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		UpdateListView();
		RoleBaseInfo selfInfo = zbServerData.sGameData.RoleBaseInfo;
		CrossPeakRankDetail crossPeakRankDetail = zbSelf.Instance.crossPeakHandler.combatList.FirstOrDefault((CrossPeakRankDetail r) => r.RoleBaseInfo?.RoleId == selfInfo.RoleId);
		if (crossPeakRankDetail != null)
		{
			((TextElement)m_lbMyRank).text = crossPeakRankDetail.Rank.ToString();
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_awardBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			int[][] awardList = zbDataConfig.Instance.peakRankAwardCsvTable.GetRecordByKey(crossPeakRankDetail.Rank).awardList;
			for (int num = 0; num < 5; num++)
			{
				bool flag = num < awardList.Length;
				((VisualElement)m_awards[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
				if (flag)
				{
					m_awards[num].Refresh(new zbItemData((EResourceType)awardList[num][0], awardList[num][1], awardList[num][2]));
				}
			}
		}
		else
		{
			m_awardBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbMyRank).text = "";
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		UpdatePerSecond();
	}

	private void InitListView()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbCrossPeakRankItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			zbPeakRankAwardCsvRecord nextRankRecord = null;
			if (m_listItemDatas.Count > index + 1)
			{
				nextRankRecord = m_listItemDatas[index + 1];
			}
			(element as zbCrossPeakRankItem).Refresh(m_listItemDatas[index], nextRankRecord);
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
		((TextElement)m_lbTime).text = string.Format(zbUtil.Language("lang.10490"), zbSelf.Instance.crossPeakHandler.GetArenaRefreshTimeStr());
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
