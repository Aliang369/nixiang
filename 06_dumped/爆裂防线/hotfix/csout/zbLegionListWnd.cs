using System;
using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionListWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Button m_btnSearch;

	private Button m_btnRefresh;

	private Label m_title;

	private Label m_lbName;

	private Label m_lbNumber;

	private Label m_lbActivity;

	private Label m_lbNodata;

	private ListView m_listView;

	private TextField m_TextField;

	private VisualElement m_noListView;

	private zbDebouncedPnl m_inputTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_LegionItem.uxml");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbNumber = UQueryExtensions.Q<Label>(m_RootElement, "lb_number", (string)null);
		m_lbActivity = UQueryExtensions.Q<Label>(m_RootElement, "lb_activity", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 118f;
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "inputPanel", (string)null);
		m_TextField = UQueryExtensions.Q<TextField>(val, "textfiled", (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_lbNodata = UQueryExtensions.Q<Label>(m_RootElement, "lb_noData", (string)null);
		m_btnSearch = UQueryExtensions.Q<Button>(val, "btn_search", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(val, "btn_refresh", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSearch, (Action<ClickEvent>)OnSearch, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action<ClickEvent>)OnRefresh, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Recommend, OnEventUpdateRecommend);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Search, OnEventUpdateSearch);
	}

	private void OnInputClick()
	{
		((Focusable)m_TextField).Focus();
	}

	public override void Refresh()
	{
		zbWsMsgHandler.Instance.Legion_Recommend_Req();
		((BaseField<string>)(object)m_TextField).value = "";
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void UpdateLegionList(RepeatedField<LegionBaseInfo> items)
	{
		((VisualElement)m_listView).visible = items.Count > 0;
		m_noListView.visible = items.Count == 0;
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbLegionListItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLegionListItem).Init(this, ((BaseVerticalCollectionView)m_listView).itemsSource[index] as LegionBaseInfo);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)items;
	}

	private void OnEventUpdateSearch(object data)
	{
		UpdateLegionList(zbSelf.Instance.legionHandler.searchList);
	}

	private void OnEventUpdateRecommend(object data)
	{
		UpdateLegionList(zbSelf.Instance.legionHandler.recommendList);
	}

	private void OnSearch(ClickEvent evt)
	{
		string content = ((TextInputBaseField<string>)(object)m_TextField).text.Trim();
		if (((BaseField<string>)(object)m_TextField).value == "")
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10079"));
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_Search_Req(content);
		}
	}

	private void OnRefresh(ClickEvent evt)
	{
		((BaseField<string>)(object)m_TextField).value = "";
		zbWsMsgHandler.Instance.Legion_Recommend_Req();
	}
}
