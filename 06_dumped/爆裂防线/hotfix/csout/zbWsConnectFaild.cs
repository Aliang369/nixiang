using System;
using UnityEngine.UIElements;

public class zbWsConnectFaild : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnOk;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_ok", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnBntBack, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnBntOk, "", (TrickleDown)0);
		AddDark();
	}

	private void OnBntBack(ClickEvent cvt)
	{
		Hide();
		zbGameStateMachine.Instance.SetState(EGameState.Login);
	}

	public override void Refresh()
	{
		base.Refresh();
		zbWsMsgHandler.Instance.HideTipConnect();
	}

	private void OnBntOk(ClickEvent cvt)
	{
		Hide();
		zbWsMsgHandler.Instance.ShowTipConnect();
		zbWsMsgHandler.Instance.Connect();
	}
}
