using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbWorldResourceCreateWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnConfirm;

	private Label m_lbName;

	private VisualElement m_icon;

	private Label m_lbDesc;

	private Button m_btnTouch;

	private WorldMapGridDetail m_gridDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_icon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldResourceActivate_Ack, OnEventUpdate);
	}

	public void RefreshCustom(WorldMapGridDetail gridDetail)
	{
		m_gridDetail = gridDetail;
		zbWorldBuildingCsvRecord recordByKey = zbDataConfig.Instance.worldBuildingCsvTable.GetRecordByKey(m_gridDetail.BuildingId);
		if (recordByKey != null)
		{
			((TextElement)m_lbName).text = zbUtil.Language(recordByKey.name);
			((TextElement)m_lbDesc).text = zbUtil.Language(recordByKey.desc);
		}
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnConfirm()
	{
		zbWsMsgHandler.Instance.WorldResource_Activate_Req(m_gridDetail.GridId);
	}

	private void OnEventUpdate(object obj)
	{
		OnClose();
	}
}
