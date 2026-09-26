using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionWarRankAwardTabLegion
{
	private zbLegionWarRankAwardWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private Label m_lbRank;

	private Label m_lbNoRank;

	private zbItem[] m_itemWins = new zbItem[4];

	private zbItem[] m_itemFails = new zbItem[4];

	private zbLegionWarLegionRankCsvRecord[] m_records;

	private List<zbLegionWarRankAwardItem> m_items = new List<zbLegionWarRankAwardItem>();

	public void OnEnable(zbLegionWarRankAwardWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, (string)null, (string)null);
		m_scrollView.verticalScrollerVisibility = (ScrollerVisibility)2;
		m_scrollView.touchScrollBehavior = (TouchScrollBehavior)1;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "mine", (string)null);
		m_lbRank = UQueryExtensions.Q<Label>(val, "lb_rank", (string)null);
		m_lbNoRank = UQueryExtensions.Q<Label>(val, "lb_noRank", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(val, "node_win", (string)null);
		VisualElement val3 = UQueryExtensions.Q<VisualElement>(val, "node_fail", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_itemWins[i] = UQueryExtensions.Q<zbItem>(val2, "item" + i, (string)null);
			m_itemWins[i].OnEnable(m_wndParent);
			m_itemFails[i] = UQueryExtensions.Q<zbItem>(val3, "item" + i, (string)null);
			m_itemFails[i].OnEnable(m_wndParent);
		}
		InitRecordList();
	}

	public void Refresh(int legionRank)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (legionRank > 0)
		{
			((VisualElement)m_lbRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbSelf.Instance.rankHandler.InitRank(m_lbRank, legionRank);
		}
		else
		{
			((VisualElement)m_lbRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		RefreshAwardList(legionRank);
	}

	private void InitRecordList()
	{
		m_records = zbDataConfig.Instance.legionWarLegionRankCsvTable.GetRecords().records.Values.OrderBy((zbLegionWarLegionRankCsvRecord r) => r.rank).ToArray();
		((VisualElement)m_scrollView).Clear();
		zbLegionWarLegionRankCsvRecord[] records = m_records;
		foreach (zbLegionWarLegionRankCsvRecord zbLegionWarLegionRankCsvRecord2 in records)
		{
			zbLegionWarRankAwardItem zbLegionWarRankAwardItem2 = new zbLegionWarRankAwardItem();
			zbLegionWarRankAwardItem2.OnEnable(m_wndParent);
			zbLegionWarRankAwardItem2.Refresh(zbLegionWarLegionRankCsvRecord2.rank.ToString(), zbLegionWarLegionRankCsvRecord2.winAwardList, zbLegionWarLegionRankCsvRecord2.failAwardList);
			m_items.Add(zbLegionWarRankAwardItem2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbLegionWarRankAwardItem2);
		}
	}

	private void RefreshAwardList(int rank)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_itemWins[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_itemFails[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (rank <= 0)
		{
			return;
		}
		zbLegionWarLegionRankCsvRecord recordByKey = zbDataConfig.Instance.legionWarLegionRankCsvTable.GetRecordByKey(rank);
		if (recordByKey == null)
		{
			return;
		}
		int num = 0;
		int[][] winAwardList = recordByKey.winAwardList;
		foreach (int[] array in winAwardList)
		{
			if (num < m_itemWins.Length)
			{
				((VisualElement)m_itemWins[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_itemWins[num].Refresh(new zbItemData((EResourceType)array[0], array[1], array[2]));
			}
			num++;
		}
		int num2 = 0;
		winAwardList = recordByKey.failAwardList;
		foreach (int[] array2 in winAwardList)
		{
			if (num2 < m_itemWins.Length)
			{
				((VisualElement)m_itemFails[num2]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_itemFails[num2].Refresh(new zbItemData((EResourceType)array2[0], array2[1], array2[2]));
			}
			num2++;
		}
	}
}
