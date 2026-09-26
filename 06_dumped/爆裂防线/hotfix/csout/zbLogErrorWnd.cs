using System;
using UnityEngine.UIElements;

public class zbLogErrorWnd : zbBaseWnd
{
	private Label m_lblError;

	private Button m_btnClose;

	private Button m_btnCopy;

	private string m_strLog;

	private Label m_lbVersion;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lblError = UQueryExtensions.Q<Label>(m_RootElement, "lbllog", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btnclose", (string)null);
		m_btnCopy = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_lbVersion = UQueryExtensions.Q<Label>(m_RootElement, "lbversion", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)Hide, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopy, (Action)OnCopy, "", (TrickleDown)0);
	}

	public void RefreshCustom(string strLog)
	{
		m_strLog = strLog;
		((TextElement)m_lblError).text = strLog;
		((TextElement)m_lbVersion).text = "ver:" + zbVersionMgr.GetCurVersion().Replace("v", "") + "(" + zbSdkHelper.GetResVersion() + ")";
	}

	private void OnCopy()
	{
		zbUtil.CopyString(m_strLog);
	}
}
