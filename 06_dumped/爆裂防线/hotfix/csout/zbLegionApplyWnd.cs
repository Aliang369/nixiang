using System;
using System.Collections;
using Google.Protobuf.Collections;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionApplyWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private Label m_title;

	private Label m_lbPlayerInfo;

	private Label m_lbApplyTime;

	private Label m_lbOperate;

	private Label m_lbApply;

	private Label m_lbApplyVal;

	private ListView m_listView;

	private VisualElement noListView;

	private VisualElement m_content;

	private VisualElement m_noListView;

	private Button m_btnBack;

	private Button m_btnAllPass;

	private Button m_btnAllRefuse;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_LegionApplyItem.uxml");
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbPlayerInfo = UQueryExtensions.Q<Label>(m_RootElement, "lb_playerInfo", (string)null);
		m_lbApplyTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_applyTime", (string)null);
		m_lbOperate = UQueryExtensions.Q<Label>(m_RootElement, "lb_operate", (string)null);
		m_lbApply = UQueryExtensions.Q<Label>(m_RootElement, "lb_apply", (string)null);
		m_lbApplyVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_applyVal", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, "listView", (string)null);
		((BaseVerticalCollectionView)m_listView).virtualizationMethod = (CollectionVirtualizationMethod)0;
		((BaseVerticalCollectionView)m_listView).fixedItemHeight = 118f;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_content = UQueryExtensions.Q<VisualElement>(m_RootElement, "content", (string)null);
		m_noListView = UQueryExtensions.Q<VisualElement>(m_RootElement, "noListView", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnAllPass = UQueryExtensions.Q<Button>(m_RootElement, "btn_allPass", (string)null);
		m_btnAllRefuse = UQueryExtensions.Q<Button>(m_RootElement, "btn_allRefuse", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllPass, (Action<ClickEvent>)OnAllPass, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAllRefuse, (Action<ClickEvent>)OnAllRefuse, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_ApplyList, OnEventUpdateApplyList);
	}

	public override void Refresh()
	{
		zbWsMsgHandler.Instance.Legion_ApplyList_Req();
	}

	private void UpdateApplyList(RepeatedField<LegionApplyDetail> items)
	{
		if (items != null)
		{
			bool flag = items.Count == 0;
			((VisualElement)m_listView).visible = !flag;
			m_noListView.visible = flag;
			m_listView.makeItem = () => (VisualElement)(object)new zbLegionApplyItem(m_itemTA, this);
			m_listView.bindItem = delegate(VisualElement element, int index)
			{
				(element as zbLegionApplyItem).Init(items[index]);
			};
			((BaseVerticalCollectionView)m_listView).itemsSource = null;
			((BaseVerticalCollectionView)m_listView).itemsSource = (IList)items;
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void OnConfirm(int playerId)
	{
		zbWsMsgHandler.Instance.Legion_ApplyPass_Req(playerId);
	}

	public void OnCancel(int playerId)
	{
		zbWsMsgHandler.Instance.Legion_ApplyRefuse_Req(playerId);
	}

	private void OnAllPass(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Legion_ApplyPassAll_Req();
	}

	private void OnAllRefuse(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Legion_ApplyRefuseAll_Req();
	}

	private void OnEventUpdateApplyList(object data)
	{
		UpdateApplyList(zbSelf.Instance.legionHandler.legionApplyList);
	}
}
