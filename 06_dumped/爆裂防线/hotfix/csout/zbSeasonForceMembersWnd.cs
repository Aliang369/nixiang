using System;
using System.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonForceMembersWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnContribute;

	private VisualElement m_iconContribute;

	private Button m_btnMerit;

	private VisualElement m_iconMerit;

	private Button m_btnPosition;

	private VisualElement m_iconPosition;

	private ListView m_listView;

	private SortType m_curSort;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_btnContribute = UQueryExtensions.Q<Button>(m_RootElement, "btn_contribute", (string)null);
		m_iconContribute = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnContribute, "icon_arrow", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnContribute, (Action)OnContribute, "", (TrickleDown)0);
		m_btnMerit = UQueryExtensions.Q<Button>(m_RootElement, "btn_merit", (string)null);
		m_iconMerit = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnMerit, "icon_arrow", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMerit, (Action)OnMerit, "", (TrickleDown)0);
		m_btnPosition = UQueryExtensions.Q<Button>(m_RootElement, "btn_position", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPosition, (Action)OnPosition, "", (TrickleDown)0);
		m_iconPosition = UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_btnPosition, "icon_arrow", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_curSort = SortType.contributeDown;
		OnEnableListView();
	}

	public override void Refresh()
	{
		RefreshListView();
		RefreshSortStatus();
	}

	private void RefreshListView()
	{
		((VisualElement)m_listView).Clear();
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)zbSelf.Instance.seasonHandler.memberList;
	}

	private void OnEnableListView()
	{
		m_listView.makeItem = delegate
		{
			zbSeasonForceMembersItem obj = new zbSeasonForceMembersItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbSeasonForceMembersItem).Refresh(((BaseVerticalCollectionView)m_listView).itemsSource[index] as SeasonMemberInfo);
		};
	}

	private void OnContribute()
	{
		m_curSort = ((m_curSort == SortType.contributeDown) ? SortType.contributeUp : SortType.contributeDown);
		RefreshSortStatus();
	}

	private void OnMerit()
	{
		m_curSort = ((m_curSort == SortType.meritDown) ? SortType.meritUp : SortType.meritDown);
		RefreshSortStatus();
	}

	private void OnPosition()
	{
		m_curSort = ((m_curSort == SortType.positionDown) ? SortType.positionUp : SortType.positionDown);
		RefreshSortStatus();
	}

	private void RefreshSortStatus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		m_iconContribute.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_iconMerit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_iconPosition.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		if (m_curSort == SortType.contributeDown || m_curSort == SortType.contributeUp)
		{
			m_iconContribute.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_iconContribute.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit((float)((m_curSort == SortType.contributeDown) ? 90 : 270))));
		}
		else if (m_curSort == SortType.meritDown || m_curSort == SortType.meritUp)
		{
			m_iconMerit.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_iconMerit.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit((float)((m_curSort == SortType.meritDown) ? 90 : 270))));
		}
		else if (m_curSort == SortType.positionDown || m_curSort == SortType.positionUp)
		{
			m_iconPosition.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_iconPosition.style.rotate = StyleRotate.op_Implicit(new Rotate(Angle.op_Implicit((float)((m_curSort == SortType.positionDown) ? 90 : 270))));
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}
}
