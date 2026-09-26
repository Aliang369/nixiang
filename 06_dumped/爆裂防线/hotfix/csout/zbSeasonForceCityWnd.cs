using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonForceCityWnd : zbBaseWnd
{
	private MultiColumnListView m_listView;

	private Button m_btnClose;

	private Button m_btnTouch;

	public FacilityRoomDetail m_roomDetail;

	private FacilityShopInfo m_shopInfo;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_listView = UQueryExtensions.Q<MultiColumnListView>(m_RootElement, "listView", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
	}

	private void RefreshList()
	{
	}

	private void OnClose()
	{
		Hide();
	}
}
