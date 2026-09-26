using System;
using UnityEngine.UIElements;

public class zbSeasonRightPanel
{
	private zbSeasonWnd m_wndParent;

	private VisualElement m_root;

	private Button m_btnSeasonTarget;

	private Button m_btnAirdrop;

	private Button m_btnRank;

	private Button m_btnTask;

	private Button m_btnRecharge;

	public void OnEnable(zbSeasonWnd wnd, VisualElement root)
	{
		m_wndParent = wnd;
		m_root = root;
		m_btnSeasonTarget = UQueryExtensions.Q<Button>(m_root, "btn_seasonTarget", (string)null);
		m_btnAirdrop = UQueryExtensions.Q<Button>(m_root, "btn_airdrop", (string)null);
		m_btnRank = UQueryExtensions.Q<Button>(UQueryExtensions.Q<VisualElement>(UQueryExtensions.Q<VisualElement>(m_root, "ui", (string)null), "right", (string)null), "btn_rank", (string)null);
		m_btnTask = UQueryExtensions.Q<Button>(m_root, "btn_task", (string)null);
		m_btnRecharge = UQueryExtensions.Q<Button>(m_root, "btn_recharge", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRank, (Action)OnRank, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTask, (Action)OnTask, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRecharge, (Action)OnRecharge, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSeasonTarget, (Action)OnSeasonTarget, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAirdrop, (Action)OnAirdrop, "", (TrickleDown)0);
	}

	public void Refresh()
	{
	}

	private void OnAirdrop()
	{
		zbWsMsgHandler.Instance.Season_Airdrop_List_Req(openWnd: true);
	}

	private void OnRank()
	{
		zbUIMgr.Instance.ShowSeasonRank();
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowSeasonTask();
	}

	private void OnRecharge()
	{
		zbUIMgr.Instance.ShowSeasonRecharge();
	}

	private void OnSeasonTarget()
	{
		zbUIMgr.Instance.ShowSeasonTarget();
	}
}
