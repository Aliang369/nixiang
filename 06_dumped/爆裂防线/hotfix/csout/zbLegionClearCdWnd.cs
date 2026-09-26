using System;
using UnityEngine.UIElements;

public class zbLegionClearCdWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_title;

	private Label m_lbClearCd;

	private Label m_lbTime;

	private Label m_lbConfirm;

	private Button m_btnConfirm;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbClearCd = UQueryExtensions.Q<Label>(m_RootElement, "lb_clearCd", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_lbConfirm = UQueryExtensions.Q<Label>(m_RootElement, "lb_confirm", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		((TextElement)m_lbConfirm).text = string.Format(zbUtil.Language("lang.10489"), ((object)zbSelf.Instance.legionHandler.GetLegionBase()[(object)"clearCDDiamond"]).ToString());
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnClearCd, "", (TrickleDown)0);
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long num = zbServerData.sGameData.LegionSelfInfo.JoinTm;
		long num2 = num - serverTime;
		if (num != 0L && num2 > 0)
		{
			((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num2);
		}
	}

	private void OnClearCd()
	{
		zbWsMsgHandler.Instance.Legion_ClearJoinCD_Req();
	}
}
