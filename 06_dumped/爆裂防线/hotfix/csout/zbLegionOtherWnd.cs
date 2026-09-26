using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionOtherWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Label m_lbTitle;

	private Label m_lbList;

	private Button m_btnList;

	private Button m_btnQuit;

	private Button m_btnLimit;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_btnList = UQueryExtensions.Q<Button>(m_RootElement, "btn_list", (string)null);
		m_btnQuit = UQueryExtensions.Q<Button>(m_RootElement, "btn_quit", (string)null);
		m_btnLimit = UQueryExtensions.Q<Button>(m_RootElement, "btn_limit", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnList, (Action)OnClickListBtn, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnQuit, (Action)OnClickQuitBtn, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLimit, (Action)OnClickLimit, "", (TrickleDown)0);
	}

	public override void Refresh()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		bool flag = zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Leader;
		((VisualElement)m_btnLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnClickLimit()
	{
		zbUIMgr.Instance.ShowLegionApplyLimited();
	}

	private void OnClickListBtn()
	{
		zbUIMgr.Instance.ShowLegionList();
	}

	private void OnClickQuitBtn()
	{
		zbUIMgr.Instance.ShowLegionLeave();
	}
}
