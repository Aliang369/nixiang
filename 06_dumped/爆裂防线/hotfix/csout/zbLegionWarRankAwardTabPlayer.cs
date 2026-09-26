using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionWarRankAwardTabPlayer
{
	private zbLegionWarRankAwardWnd m_wndParent;

	private VisualElement m_RootElement;

	private ScrollView m_scrollView;

	private Label m_lbRank;

	private Label m_lbNoRank;

	private zbItem[] m_itemWins = new zbItem[4];

	private zbItem[] m_itemFails = new zbItem[4];

	private zbLegionWarPlayerRankCsvRecord[] m_records;

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

	public void Refresh(int playerRank)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (playerRank > 0)
		{
			((VisualElement)m_lbRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbSelf.Instance.rankHandler.InitRank(m_lbRank, playerRank);
		}
		else
		{
			((VisualElement)m_lbRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbNoRank).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		RefreshAwardList(playerRank);
	}

	private void InitRecordList()
	{
		m_records = zbDataConfig.Instance.legionWarPlayerRankCsvTable.GetRecords().records.Values.OrderBy((zbLegionWarPlayerRankCsvRecord r) => r.rank).ToArray();
		((VisualElement)m_scrollView).Clear();
		zbLegionWarPlayerRankCsvRecord[] records = m_records;
		foreach (zbLegionWarPlayerRankCsvRecord zbLegionWarPlayerRankCsvRecord2 in records)
		{
			zbLegionWarRankAwardItem zbLegionWarRankAwardItem2 = new zbLegionWarRankAwardItem();
			zbLegionWarRankAwardItem2.OnEnable(m_wndParent);
			zbLegionWarRankAwardItem2.Refresh(zbLegionWarPlayerRankCsvRecord2.showRank, zbLegionWarPlayerRankCsvRecord2.winAwardList, zbLegionWarPlayerRankCsvRecord2.failAwardList);
			m_items.Add(zbLegionWarRankAwardItem2);
			((VisualElement)m_scrollView).Add((VisualElement)(object)zbLegionWarRankAwardItem2);
		}
	}

	private void RefreshAwardList(int rank)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 4; i++)
		{
			((VisualElement)m_itemWins[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_itemFails[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (rank <= 0)
		{
			return;
		}
		zbLegionWarPlayerRankCsvRecord zbLegionWarPlayerRankCsvRecord2 = null;
		foreach (zbLegionWarPlayerRankCsvRecord value in zbDataConfig.Instance.legionWarPlayerRankCsvTable.GetRecords().records.Values)
		{
			if (rank <= value.rank)
			{
				zbLegionWarPlayerRankCsvRecord2 = value;
				break;
			}
		}
		if (zbLegionWarPlayerRankCsvRecord2 == null)
		{
			return;
		}
		int num = 0;
		int[][] winAwardList = zbLegionWarPlayerRankCsvRecord2.winAwardList;
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
		winAwardList = zbLegionWarPlayerRankCsvRecord2.failAwardList;
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
