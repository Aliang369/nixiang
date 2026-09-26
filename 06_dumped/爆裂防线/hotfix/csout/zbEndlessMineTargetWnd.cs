using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEndlessMineTargetWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private ListView m_list;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_list = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_list, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_list, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_list).fixedItemHeight = 140f;
		((BaseVerticalCollectionView)m_list).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_list, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_EndlessMineInfo, delegate
		{
			Refresh();
		});
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		((VisualElement)m_list).Clear();
		if (zbSelf.Instance.endlessMineHandler.endlessMineInfo != null && zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo != null)
		{
			List<OperateTargetDetail> operateTargetDetails = ((IEnumerable<OperateTargetDetail>)zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.TargetList).ToList();
			m_list.makeItem = () => (VisualElement)(object)new zbEndlessMineTargetItem(this);
			m_list.bindItem = delegate(VisualElement element, int index)
			{
				zbEndlessMineTargetItem zbEndlessMineTargetItem2 = element as zbEndlessMineTargetItem;
				OperateTargetDetail targetDetail = operateTargetDetails[index];
				zbEndlessMineTargetItem2?.Refresh(targetDetail, zbSelf.Instance.endlessMineHandler.endlessMineInfo.EndlessMineBaseInfo.Depth);
			};
			((BaseVerticalCollectionView)m_list).itemsSource = operateTargetDetails;
		}
	}
}
