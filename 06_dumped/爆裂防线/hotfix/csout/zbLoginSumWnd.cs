using System;
using System.Collections;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLoginSumWnd : zbBaseWnd
{
	private VisualTreeAsset m_itemTA;

	private VisualElement m_spriteBanner;

	private Label m_lbTitle;

	private Label m_lbTime;

	private Button m_btnClose;

	private ListView m_listView;

	private LoginSumDetail m_data;

	private zbOperateCsvRecord m_record;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/operate/tb_loginSum.uxml");
		m_spriteBanner = UQueryExtensions.Q<VisualElement>(m_RootElement, "banner", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_listView = UQueryExtensions.Q<ListView>(m_RootElement, (string)null, (string)null);
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).horizontalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).verticalScrollerVisibility = (ScrollerVisibility)2;
		UQueryExtensions.Q<ScrollView>((VisualElement)(object)m_listView, (string)null, (string)null).touchScrollBehavior = (TouchScrollBehavior)1;
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_Operate, OpEventUpdateOperate);
	}

	public void RefreshCustom(LoginSumDetail loginSumDetail)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		m_data = loginSumDetail;
		m_record = zbDataConfig.Instance.operateCsvTable.GetRecordByKey(10);
		m_spriteBanner.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_record.banner));
		((TextElement)m_lbTitle).text = zbUtil.Language(m_record.name);
		UpdateListView();
		RefreshTime();
	}

	private void OnClose()
	{
		Hide();
	}

	private void UpdateListView()
	{
		((VisualElement)m_listView).Clear();
		m_listView.makeItem = () => (VisualElement)(object)new zbLoginSumItem(m_itemTA, this);
		m_listView.bindItem = delegate(VisualElement element, int index)
		{
			(element as zbLoginSumItem).Init(m_data.TargetList[index], m_data);
		};
		((BaseVerticalCollectionView)m_listView).itemsSource = null;
		((BaseVerticalCollectionView)m_listView).itemsSource = (IList)m_data.TargetList;
	}

	private void OpEventUpdateOperate(object data)
	{
		LoginSumDetail loginSumInfoByUUID = zbSelf.Instance.operateHandler.GetLoginSumInfoByUUID(m_data.BaseInfo.UUID);
		RefreshCustom(loginSumInfoByUUID);
	}

	private void RefreshTime()
	{
		if (m_data != null)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(m_data.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime());
		}
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}
}
