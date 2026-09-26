using System;
using System.Collections.Generic;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbActLogin
{
	private zbAccountWnd m_parent;

	private VisualElement m_loginElement;

	private Button m_btnGuest;

	private Button m_btnFast;

	private Button m_btnLogin;

	private Button m_btnMorAct;

	private TextField m_textFieldAct;

	private TextField m_textFieldPwd;

	private VisualElement m_actsPnl;

	private List<Label> m_moreActLbl;

	private List<Button> m_moreBtnSel;

	private List<VisualElement> m_moreActRoot;

	public zbActLogin(VisualElement root, zbAccountWnd parent)
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		m_moreActLbl = new List<Label>();
		m_moreBtnSel = new List<Button>();
		m_moreActRoot = new List<VisualElement>();
		base._002Ector();
		m_loginElement = root;
		m_parent = parent;
		m_btnGuest = UQueryExtensions.Q<Button>(m_loginElement, "button_guest", (string)null);
		m_btnFast = UQueryExtensions.Q<Button>(m_loginElement, "button_fast", (string)null);
		m_btnLogin = UQueryExtensions.Q<Button>(m_loginElement, "button_login", (string)null);
		m_btnMorAct = UQueryExtensions.Q<Button>(m_loginElement, "button_pull", (string)null);
		((VisualElement)m_btnGuest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((VisualElement)m_btnLogin).style.left = StyleLength.op_Implicit(350f);
		((VisualElement)m_btnLogin).style.top = StyleLength.op_Implicit(790f);
		if (zbLoginUtil.appChannel == EChannelType.Zb)
		{
			((VisualElement)m_btnGuest).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnLogin).style.left = StyleLength.op_Implicit(505f);
			((VisualElement)m_btnLogin).style.top = StyleLength.op_Implicit(870f);
		}
		m_textFieldAct = UQueryExtensions.Q<TextField>(m_loginElement, "field_account", (string)null);
		string imgPath = "ui/image/login/account/lg_g1.png";
		zbUtil.SetTextFieldStyle(m_textFieldAct, imgPath);
		m_textFieldPwd = UQueryExtensions.Q<TextField>(m_loginElement, "field_pwd", (string)null);
		zbUtil.SetTextFieldStyle(m_textFieldPwd, imgPath);
		m_actsPnl = UQueryExtensions.Q<VisualElement>(m_loginElement, "accounts", (string)null);
		m_actsPnl.visible = false;
		m_moreActLbl.Clear();
		m_moreBtnSel.Clear();
		m_moreActRoot.Clear();
		for (int i = 1; i <= 3; i++)
		{
			VisualElement val = UQueryExtensions.Q<VisualElement>(m_actsPnl, "account" + i, (string)null);
			val.visible = false;
			m_moreActRoot.Add(val);
			Label item = UQueryExtensions.Q<Label>(val, "lblaccount", (string)null);
			m_moreActLbl.Add(item);
			int idx = i - 1;
			Button val2 = UQueryExtensions.Q<Button>(val, "btnSel", (string)null);
			parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)val2, (Action<ClickEvent>)delegate
			{
				OnSelAccount(idx);
			}, "", (TrickleDown)0);
			m_moreBtnSel.Add(val2);
		}
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGuest, (Action<ClickEvent>)OnGuest, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFast, (Action<ClickEvent>)OnFastLogin, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLogin, (Action<ClickEvent>)OnLogin, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMorAct, (Action<ClickEvent>)OnMoreAccount, "", (TrickleDown)0);
	}

	public void SetVisible(bool visi)
	{
		if (m_loginElement != null)
		{
			m_loginElement.visible = visi;
		}
		if (m_actsPnl != null)
		{
			m_actsPnl.visible = false;
		}
	}

	private void SetActsPnlVisible(bool visi)
	{
		if (m_actsPnl != null)
		{
			m_actsPnl.visible = visi;
		}
		foreach (VisualElement item in m_moreActRoot)
		{
			item.visible = false;
		}
	}

	private void OnMoreAccount(ClickEvent evt)
	{
		SetActsPnlVisible(!m_actsPnl.visible);
		if (!m_actsPnl.visible || zbAccountMgr.accountOrder.Count <= 0)
		{
			return;
		}
		int num = 0;
		foreach (string item in zbAccountMgr.accountOrder)
		{
			if (num < 3)
			{
				m_moreActRoot[num].visible = true;
				((TextElement)m_moreActLbl[num]).text = item;
			}
			num++;
		}
	}

	public void RefreshAccountInfo()
	{
		((BaseField<string>)(object)m_textFieldAct).value = zbAccountMgr.GetLastLoginAccount();
	}

	private void OnSelAccount(int idx)
	{
		string value = zbAccountMgr.accountOrder[idx];
		if (!string.IsNullOrEmpty(value))
		{
			((BaseField<string>)(object)m_textFieldAct).value = value;
			SetActsPnlVisible(visi: false);
		}
	}

	private void OnLogin(ClickEvent evt)
	{
		string value = ((BaseField<string>)(object)m_textFieldAct).value;
		string value2 = ((BaseField<string>)(object)m_textFieldPwd).value;
		zbAccountMgr.account_zb = value;
		zbAccountMgr.pwd_zb = value2;
		if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value2))
		{
			zbUIMgr.Instance.BubbleInfo("account can not nil");
		}
		else
		{
			zbLoginUtil.LoginZBAccount();
		}
	}

	private void OnFastLogin(ClickEvent evt)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("appid", zbLoginUtil.appId);
		dictionary.Add("source", zbAccountMgr.source_zb);
		dictionary.Add("device", zbAccountMgr.device_zb);
		dictionary.Add("device_uuid", zbLoginUtil.deviceId);
		dictionary.Add("os", zbAccountMgr.os_zb);
		dictionary.Add("os_vers", zbAccountMgr.os_vers_zb);
		string postDataSign = zbLoginUtil.GetPostDataSign(dictionary);
		dictionary.Add("sign", postDataSign);
		zbHttpMsgHandler.Instance.SendPostRequest(zbNetUrl.visitorPost_url, dictionary, PostMsgId.E_ZB_MSG_VISITOR);
	}

	private void OnGuest(ClickEvent evt)
	{
		zbHttpMsgHandler.Instance.User_VisitorInit_Req();
		if ((Object)(object)m_parent != (Object)null)
		{
			m_parent.Hide();
		}
	}
}
