using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionWarTaskWnd : zbTabWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbTime;

	private ListView m_listView;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/legionWar/tb_legionWarTask.uxml");
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 134f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		OnEnableList();
	}

	public override void Refresh()
	{
		base.Refresh();
		RefreshTitle();
		RefreshList();
		RefreshLeftTime();
	}

	private void RefreshTitle()
	{
		switch (zbSelf.Instance.legionWarHandler.GetLegionWarState())
		{
		case ELegionWarState.Preview:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11226"));
			break;
		case ELegionWarState.SignUp:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11157"));
			break;
		case ELegionWarState.Prepare:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11165"));
			break;
		case ELegionWarState.Fighting:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11156"));
			break;
		case ELegionWarState.Settling:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11258"));
			break;
		case ELegionWarState.Finished:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.11259"));
			break;
		case ELegionWarState.WaitNext:
			((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.11247"), zbUtil.Language("lang.10942"));
			break;
		}
	}

	private void RefreshLeftTime()
	{
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(zbSelf.Instance.legionWarHandler.GetLeftTimeByState());
	}

	private void RefreshList()
	{
		List<TaskDetail> itemsSource = (from x in (IEnumerable<TaskDetail>)zbServerData.sGameData.LegionWarBaseInfo.LegionWarTaskList
			orderby x.State switch
			{
				EActiveState.Active => 0, 
				EActiveState.Pending => 1, 
				EActiveState.Done => 2, 
				_ => 99, 
			}, zbDataConfig.Instance.legionWarTaskCsvTable.GetRecordByKey(x.TaskId).sortIndex
			select x).ToList();
		((BaseVerticalCollectionView)m_listView).itemsSource = itemsSource;
	}

	private void OnEnableList()
	{
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionWarTaskItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLegionWarTaskItem).Init(((BaseVerticalCollectionView)m_listView).itemsSource[index] as TaskDetail);
		};
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshLeftTime();
	}

	private void OnClose()
	{
		Hide();
	}
}
