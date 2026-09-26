using System;
using UnityEngine.UIElements;

public class zbWorldReconWorkerListWnd : zbBaseWnd
{
	private Button m_btnClose;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
	}

	private void OnClose()
	{
		Hide();
	}
}
