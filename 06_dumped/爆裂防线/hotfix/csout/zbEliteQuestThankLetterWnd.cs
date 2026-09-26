using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEliteQuestThankLetterWnd : zbBaseWnd
{
	private Button m_btnClose;

	private VisualElement m_noListView;

	private ListView m_listView;

	private Label m_lbCount;

	private Button m_btnGetTili;

	private List<HelpRecordDetail> m_data;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		EnableListView();
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_count", (string)null);
		m_btnGetTili = UQueryExtensions.Q<Button>(m_RootElement, "btn_getTiLi", (string)null);
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGetTili, (Action)OnGetTili, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_QuestHelp_Award, OnUpdateEventQuestHelp);
	}

	private void EnableListView()
	{
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 128f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_listView.makeItem = delegate
		{
			zbEliteQuestThankLetterItem obj = new zbEliteQuestThankLetterItem();
			obj.OnEnable(this);
			return (VisualElement)(object)obj;
		};
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			List<HelpRecordDetail> list = ((BaseVerticalCollectionView)m_listView).itemsSource as List<HelpRecordDetail>;
			(element as zbEliteQuestThankLetterItem).Init(list[index]);
		};
	}

	public void RefreshCustom(HelpRecordDetail[] recordList)
	{
		m_data = recordList.ToList();
		((TextElement)m_lbCount).text = zbUtil.Language("lang.10466") + zbServerData.sGameData.HelpInfo.HelpAwardCount;
		UpdateListview();
	}

	private void UpdateListview()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		List<HelpRecordDetail> data = m_data;
		((VisualElement)m_listView).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(data.Count <= 0));
		m_noListView.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(data.Count > 0));
		((BaseVerticalCollectionView)m_listView).itemsSource = data;
		((BaseVerticalCollectionView)m_listView).RefreshItems();
	}

	private void OnUpdateEventQuestHelp(object data)
	{
		RefreshCustom(zbSelf.Instance.eliteQuestHandler.eliteQuestHelpRecords);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnGetTili()
	{
		zbWsMsgHandler.Instance.EliteQuest_HelpAwardFast_Req();
	}
}
