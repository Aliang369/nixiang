using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbActReg
{
	private zbAccountWnd m_parent;

	private VisualElement m_regElement;

	private Button m_btnReg;

	private Button m_btnBack;

	private TextField m_textFieldAct;

	private TextField m_textFieldPwd;

	public zbActReg(VisualElement root, zbAccountWnd parent)
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		m_regElement = root;
		m_parent = parent;
		m_btnReg = UQueryExtensions.Q<Button>(m_regElement, "button_reg", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_regElement, "button_back", (string)null);
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>("ui/image/login/account/lg_g1.png");
		m_textFieldAct = UQueryExtensions.Q<TextField>(m_regElement, "field_account", (string)null);
		((TextInputBaseField<string>)(object)m_textFieldAct).isReadOnly = true;
		VisualElement obj = UQueryExtensions.Q((VisualElement)(object)m_textFieldAct, "unity-text-input", (string)null);
		obj.style.backgroundColor = new StyleColor(Color.clear);
		obj.style.backgroundImage = new StyleBackground(val);
		m_textFieldPwd = UQueryExtensions.Q<TextField>(m_regElement, "field_pwd", (string)null);
		VisualElement obj2 = UQueryExtensions.Q((VisualElement)(object)m_textFieldPwd, "unity-text-input", (string)null);
		obj2.style.backgroundColor = new StyleColor(Color.clear);
		obj2.style.backgroundImage = new StyleBackground(val);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReg, (Action<ClickEvent>)OnReg, "", (TrickleDown)0);
		m_parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnBack, "", (TrickleDown)0);
	}

	public void SetVisible(bool visi)
	{
		if (m_regElement != null)
		{
			m_regElement.visible = visi;
		}
	}

	private void OnReg(ClickEvent evt)
	{
		string value = ((BaseField<string>)(object)m_textFieldAct).value;
		string text = ((BaseField<string>)(object)m_textFieldPwd).value ?? "123456";
		zbAccountMgr.account_zb = value;
		zbAccountMgr.pwd_zb = text;
		if (string.IsNullOrEmpty(value))
		{
			zbUIMgr.Instance.BubbleInfo("account can not nil");
			return;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("appid", zbLoginUtil.appId);
		dictionary.Add("userid", zbAccountMgr.userid_zb);
		dictionary.Add("account", value);
		dictionary.Add("password", text);
		string postDataSign = zbLoginUtil.GetPostDataSign(dictionary);
		dictionary.Add("sign", postDataSign);
		zbHttpMsgHandler.Instance.SendPostRequest(zbNetUrl.visitorRegPost_url, dictionary, PostMsgId.E_ZB_MSG_REG);
	}

	public void SetFastLoginInfo()
	{
		((BaseField<string>)(object)m_textFieldAct).value = zbAccountMgr.account_zb;
		((BaseField<string>)(object)m_textFieldPwd).value = "123456";
	}

	private void OnBack(ClickEvent evt)
	{
		if ((Object)(object)m_parent != (Object)null)
		{
			m_parent.ShowLoginPnl();
		}
	}
}
