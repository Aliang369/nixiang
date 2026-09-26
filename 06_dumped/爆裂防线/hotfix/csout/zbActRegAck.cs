using System;
using UnityEngine.UIElements;

public class zbActRegAck
{
	private zbAccountWnd m_parent;

	private VisualElement m_regAckElement;

	private Button m_btnLoginGame;

	private Label m_lblAc;

	private Label m_lblPwd;

	public zbActRegAck(VisualElement root, zbAccountWnd parent)
	{
		m_regAckElement = root;
		m_parent = parent;
		m_btnLoginGame = UQueryExtensions.Q<Button>(m_regAckElement, "button_login", (string)null);
		m_lblAc = UQueryExtensions.Q<Label>(m_regAckElement, "lblaccount", (string)null);
		m_lblPwd = UQueryExtensions.Q<Label>(m_regAckElement, "lblpwd", (string)null);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLoginGame, (Action<ClickEvent>)OnLoginGame, "", (TrickleDown)0);
	}

	public void SetVisible(bool visi)
	{
		if (m_regAckElement != null)
		{
			m_regAckElement.visible = visi;
		}
	}

	public void SetAccountInfo()
	{
		((TextElement)m_lblAc).text = zbAccountMgr.account_zb;
		((TextElement)m_lblPwd).text = zbAccountMgr.pwd_zb;
		zbAccountMgr.SaveAccount(zbAccountMgr.account_zb, zbAccountMgr.pwd_zb);
	}

	private void OnLoginGame(ClickEvent evt)
	{
		zbLoginUtil.LoginZBAccount();
	}
}
