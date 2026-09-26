using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbCombatResume : zbBaseWnd
{
	private Button m_btnConfirm;

	private Button m_btnCancel;

	private Button m_btnClose;

	private zbCombatRecorder recorder;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnCancel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnRestore, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnCancel, "", (TrickleDown)0);
	}

	public void RefreshCustom(zbCombatRecorder _recorder)
	{
		recorder = _recorder;
	}

	private void OnRestore()
	{
		OnClose();
		recorder.Restore();
	}

	private void OnCancel()
	{
		OnClose();
		if (recorder.loadedData.recordCombatSystem.battleInfo.BattleBaseInfo.BattleType == EBattleFuncType.LegionWar)
		{
			zbWsMsgHandler.Instance.LegionWar_FightCancel_Req();
		}
		recorder.ClearRecord();
	}

	private void OnClose()
	{
		Hide();
	}
}
