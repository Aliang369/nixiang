using System;
using UnityEngine.UIElements;

public class zbTrainSelectTipWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnDriver;

	private Button m_btnTaker;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnDriver = UQueryExtensions.Q<Button>(m_RootElement, "btn_driver", (string)null);
		m_btnTaker = UQueryExtensions.Q<Button>(m_RootElement, "btn_taker", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDriver, (Action)OnDriver, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTaker, (Action)OnTaker, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
		AddDark();
	}

	public void RefreshCustom()
	{
	}

	private void OnDriver()
	{
		zbUIMgr.Instance.ShowTrainDriver();
		OnClose();
	}

	private void OnTaker()
	{
		zbWsMsgHandler.Instance.Train_TransportTake_Req(openTaker: true);
		OnClose();
	}

	private void OnClose(object data = null)
	{
		Hide();
	}
}
