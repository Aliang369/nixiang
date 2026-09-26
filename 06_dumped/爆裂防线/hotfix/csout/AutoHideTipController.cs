using System;
using UnityEngine.UIElements;

public class AutoHideTipController
{
	private Button m_btn;

	private Label m_tipLabel;

	private bool m_tipVisible;

	private IVisualElementScheduledItem m_hideSchedule;

	private zbUIEvent m_uiEvent;

	private int m_autoHideMs;

	public AutoHideTipController(zbUIEvent uIEvent, Button tipButton, Label tipLabel, int autoHideMs = 3000)
	{
		m_uiEvent = uIEvent;
		m_btn = tipButton;
		m_tipLabel = tipLabel;
		m_autoHideMs = autoHideMs;
		if (m_btn != null)
		{
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btn, (Action<ClickEvent>)OnClick, "", (TrickleDown)0);
		}
		HideImmediate();
	}

	private void OnClick(ClickEvent evt)
	{
		if (m_tipVisible)
		{
			HideImmediate();
		}
		else
		{
			ShowAndAutoHide();
		}
	}

	public void ShowAndAutoHide()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_tipLabel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_tipVisible = true;
		IVisualElementScheduledItem hideSchedule = m_hideSchedule;
		if (hideSchedule != null)
		{
			hideSchedule.Pause();
		}
		m_hideSchedule = ((VisualElement)m_tipLabel).schedule.Execute((Action)HideImmediate).StartingIn((long)m_autoHideMs);
	}

	public void HideImmediate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_tipLabel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_tipVisible = false;
		IVisualElementScheduledItem hideSchedule = m_hideSchedule;
		if (hideSchedule != null)
		{
			hideSchedule.Pause();
		}
		m_hideSchedule = null;
	}
}
