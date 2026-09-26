using System.Collections.Generic;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLeaderTipTabGem
{
	private VisualTreeAsset m_itemTA;

	private zbLeaderTipWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_listView;

	private Label m_lbNo;

	private List<int> m_ListViewDatas = new List<int>();

	public void OnEnable(zbLeaderTipWnd wndParent, VisualElement root)
	{
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/leader/tb_leaderTip_gem.uxml");
		m_wndParent = wndParent;
		m_RootElement = root;
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_lbNo = UQueryExtensions.Q<Label>(m_RootElement, "lb_no", (string)null);
	}

	public void Refresh()
	{
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_listView).Clear();
		m_ListViewDatas.Clear();
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid > 0)
					{
						m_ListViewDatas.Add(gemSlot.GemTid);
					}
				}
			}
		}
		m_listView.makeItem = () => (VisualElement)(object)new zbLeaderTipItemGem(m_wndParent, m_itemTA);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLeaderTipItemGem).Refresh(m_ListViewDatas[index]);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = m_ListViewDatas;
		((VisualElement)m_lbNo).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_ListViewDatas.Count > 0));
	}
}
