using System;
using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionRecommendWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private Button m_btnFind;

	private Button m_btnRefresh;

	private Button m_btnAllApply;

	private Button m_btnCreateLegion;

	private Label m_title;

	private Label m_lbName;

	private Label m_lbNumber;

	private Label m_lbActivity;

	private Label m_lbNodata;

	private ListView m_listView;

	private TextField m_TextField;

	private Label m_lbPlaceHolder;

	private VisualElement m_noListView;

	private zbDebouncedPnl m_inputTouch;

	protected override void OnEnable()
	{
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Expected O, but got Unknown
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
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
		m_lbNodata = UQueryExtensions.Q<Label>(m_RootElement, "lb_noData", (string)null);
		m_btnCreateLegion = UQueryExtensions.Q<Button>(m_RootElement, "btn_createLegion", (string)null);
		m_btnAllApply = UQueryExtensions.Q<Button>(m_RootElement, "btn_allApply", (string)null);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "inputPanel", (string)null);
		m_TextField = UQueryExtensions.Q<TextField>(val, (string)null, (string)null);
		m_lbPlaceHolder = new Label();
		((VisualElement)m_lbPlaceHolder).name = "custom-placeholder";
		((VisualElement)m_lbPlaceHolder).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)m_lbPlaceHolder).style.left = StyleLength.op_Implicit(8f);
		((VisualElement)m_lbPlaceHolder).style.top = StyleLength.op_Implicit(2f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)m_lbPlaceHolder);
		((VisualElement)m_lbPlaceHolder).pickingMode = (PickingMode)1;
		((TextElement)m_lbPlaceHolder).text = zbUtil.Language("lang.10498");
		((VisualElement)m_TextField).Add((VisualElement)(object)m_lbPlaceHolder);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(val, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_btnFind = UQueryExtensions.Q<Button>(val, "btn_find", (string)null);
		m_btnRefresh = UQueryExtensions.Q<Button>(val, "btn_refresh", (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRefresh, (Action<ClickEvent>)OnRefresh, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFind, (Action<ClickEvent>)OnClickFind, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCreateLegion, (Action<ClickEvent>)OnClickCreate, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllApply, (Action<ClickEvent>)OnClickAllApply, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<BlurEvent>((VisualElement)(object)m_TextField, (Action)OnBlur, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<FocusEvent>((VisualElement)(object)m_TextField, (Action)OnFocus, "", (TrickleDown)0);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Recommend, OnEventUpdateRecommend);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Search, OnEventUpdateSearch);
	}

	private void OnInputClick()
	{
		((Focusable)m_TextField).Focus();
	}

	private void OnBlur()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseField<string>)(object)m_TextField).value == "")
		{
			((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnFocus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	public override void Refresh()
	{
		zbWsMsgHandler.Instance.Legion_Recommend_Req();
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Legion);
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnRefresh(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Legion_Recommend_Req();
	}

	private void OnClickCreate(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionCreate();
	}

	private void OnClickAllApply(ClickEvent evt)
	{
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		if (zbServerData.sGameData.LegionSelfInfo.JoinTm - serverTime > 0)
		{
			zbUIMgr.Instance.ShowLegionClearCd();
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_JoinQuick_Req();
		}
	}

	private void OnClickFind(ClickEvent evt)
	{
		string text = ((BaseField<string>)(object)m_TextField).value.Trim();
		if (text == "")
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10083"));
		}
		else if (text.Length > 14)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10073"));
		}
		else if (!zbCharChecker.IsSafeChatMessage(text))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_Search_Req(text);
		}
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

	private void OnEventUpdateRecommend(object data)
	{
		UpdateLegionList(zbSelf.Instance.legionHandler.recommendList);
	}

	private void OnEventUpdateSearch(object data)
	{
		UpdateLegionList(zbSelf.Instance.legionHandler.searchList);
	}
}
