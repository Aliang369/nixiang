using System;
using UnityEngine.UIElements;

public class zbWorldRadarWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbDesc;

	private Button m_btnConfirm;

	private bool needRefreshTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldEventRadar_Ack, OnEventRadarAck);
	}

	public override void Refresh()
	{
		((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10109");
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0)
		{
			if (zbSelf.Instance.timeHandler.GetServerTime() < zbServerData.sGameData.WorldInfo.EventInfo.NextTm)
			{
				needRefreshTime = true;
				RefreshTime();
			}
			else
			{
				needRefreshTime = false;
				((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10523");
			}
		}
		else
		{
			needRefreshTime = false;
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10520");
		}
	}

	private void RefreshTime()
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0 && needRefreshTime)
		{
			long num = zbServerData.sGameData.WorldInfo.EventInfo.NextTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbDesc).text = string.Format(zbUtil.Language("lang.10522"), zbUtil.FormatTimeHHMMSS(num));
			}
			else
			{
				Refresh();
			}
		}
	}

	private void OnConfirm(ClickEvent evt)
	{
		if (zbServerData.sGameData.WorldInfo.EventInfo.NextTm >= 0 && zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.WorldInfo.EventInfo.NextTm)
		{
			zbWsMsgHandler.Instance.WorldEvent_Radar_Req();
		}
		else
		{
			OnClose(null);
		}
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshTime();
	}

	private void OnEventRadarAck(object data)
	{
		OnClose(null);
	}
}
