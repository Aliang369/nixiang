using System;
using UnityEngine.UIElements;

public class zbDungeonResumeWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbDesc;

	private Button m_btnConfirm;

	private Button m_btnCancel;

	private Button m_btnClose;

	private bool m_isWorldDungeon;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnCancel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnRestore, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnCancel, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Dungeon_Close_Ack, OnDungeonClose);
		AddDark();
	}

	public void RefreshCustom(bool isWorldDungeon = false)
	{
		m_isWorldDungeon = isWorldDungeon;
	}

	private void OnRestore()
	{
		if (m_isWorldDungeon)
		{
			zbWsMsgHandler.Instance.WorldDungeon_Continue_Req();
		}
	}

	private void OnCancel()
	{
		if (m_isWorldDungeon)
		{
			zbWsMsgHandler.Instance.WorldDungeon_Close_Req();
		}
		zbCombatRecorder obj = new zbCombatRecorder();
		obj.Init();
		obj.ClearRecord();
	}

	private void OnDungeonClose(object data)
	{
		Hide();
		zbDungeonMgr.Instance.ResetDungeonInfo();
		if (m_isWorldDungeon)
		{
			zbUIMgr.Instance.ShowDungeonBalance(zbSelf.Instance.worldDungeonHandler);
		}
	}
}
