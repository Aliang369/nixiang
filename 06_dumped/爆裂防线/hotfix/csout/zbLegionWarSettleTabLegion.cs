using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarSettleTabLegion
{
	public enum ForceType
	{
		Red = 1,
		Blue
	}

	private zbLegionWarSettleWnd m_wndParent;

	private VisualElement m_RootElement;

	private ListView m_redListView;

	private ListView m_blueListView;

	public void OnEnable(zbLegionWarSettleWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_redListView = UQueryExtensions.Q<ListView>(m_RootElement, "redListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_redListView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_redListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_redListView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_blueListView = UQueryExtensions.Q<ListView>(m_RootElement, "blueListView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_blueListView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_blueListView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_blueListView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		OnEnableListView();
	}

	public void RefreshCustom()
	{
		UpdateListView();
	}

	private void OnEnableListView()
	{
		m_redListView.makeItem = () => (VisualElement)(object)new zbLegionWarSettleLegionItem(m_wndParent);
		m_redListView.bindItem = delegate(VisualElement element, int index)
		{
			IList itemsSource = ((BaseVerticalCollectionView)m_redListView).itemsSource;
			if (itemsSource != null && index >= 0 && index < itemsSource.Count)
			{
				(element as zbLegionWarSettleLegionItem)?.Refresh(((BaseVerticalCollectionView)m_redListView).itemsSource[index] as LegionWarLegionRankDetail, ELegionWarForce.Red);
			}
		};
		m_blueListView.makeItem = () => (VisualElement)(object)new zbLegionWarSettleLegionItem(m_wndParent);
		m_blueListView.bindItem = delegate(VisualElement element, int index)
		{
			IList itemsSource = ((BaseVerticalCollectionView)m_blueListView).itemsSource;
			if (itemsSource != null && index >= 0 && index < itemsSource.Count)
			{
				(element as zbLegionWarSettleLegionItem)?.Refresh(((BaseVerticalCollectionView)m_blueListView).itemsSource[index] as LegionWarLegionRankDetail, ELegionWarForce.Blue);
			}
		};
	}

	public void UpdateListView()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		LegionWarSettleLegionInfoAck legionWarSettleLegionInfoAck = zbSelf.Instance.legionWarHandler.legionWarSettleLegionInfoAck;
		if (legionWarSettleLegionInfoAck != null && legionWarSettleLegionInfoAck.ForceList != null)
		{
			LegionWarSettleLegionInfo legionWarSettleLegionInfo = ((IEnumerable<LegionWarSettleLegionInfo>)legionWarSettleLegionInfoAck.ForceList).FirstOrDefault((LegionWarSettleLegionInfo x) => x.ForceId == ELegionWarForce.Red);
			bool flag = legionWarSettleLegionInfo != null && legionWarSettleLegionInfo.RankList != null && legionWarSettleLegionInfo.RankList.Count > 0;
			((VisualElement)m_redListView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				((BaseVerticalCollectionView)m_redListView).itemsSource = (IList)legionWarSettleLegionInfo.RankList;
			}
			LegionWarSettleLegionInfo legionWarSettleLegionInfo2 = ((IEnumerable<LegionWarSettleLegionInfo>)legionWarSettleLegionInfoAck.ForceList).FirstOrDefault((LegionWarSettleLegionInfo x) => x.ForceId == ELegionWarForce.Blue);
			bool flag2 = legionWarSettleLegionInfo2 != null && legionWarSettleLegionInfo2.RankList != null && legionWarSettleLegionInfo2.RankList.Count > 0;
			((VisualElement)m_blueListView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				((BaseVerticalCollectionView)m_blueListView).itemsSource = (IList)legionWarSettleLegionInfo2.RankList;
			}
		}
	}
}
