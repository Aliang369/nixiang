using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class zbLoginWnd : zbBaseWnd
{
	private Button m_btnLogin;

	private Button m_btnHykb;

	private Button m_btnNotice;

	private Button m_btnAccount;

	private Button m_btnAcdel;

	private Button m_btnCall;

	private Button m_btnServer;

	private Button m_btnStyle;

	private Button m_btnTip12;

	private Label m_lblGuid;

	private Label m_lbVersion;

	private static bool m_bInit;

	private Label m_lbAgreement;

	private Label m_lbTipLine3Right;

	private VisualElement m_tipAgreement;

	private VisualElement m_spriteOk;

	private Button m_btnPrivacy;

	protected override void Awake()
	{
		base.Awake();
		if (string.IsNullOrEmpty(zbGameSave.Instance.GetString(zbLoginUtil.KEY_FIRST_START_GAME)))
		{
			zbChannel_Report.ReportGameData("1");
			zbGameSave.Instance.SetString(zbLoginUtil.KEY_FIRST_START_GAME, "1");
		}
		else
		{
			zbChannel_Report.ReportGameData("2");
		}
	}

	protected override void OnEnable()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0476: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnLogin = UQueryExtensions.Q<Button>(m_RootElement, "button_login", (string)null);
		m_btnHykb = UQueryExtensions.Q<Button>(m_RootElement, "button_hykb", (string)null);
		((VisualElement)m_btnHykb).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbLoginUtil.appChannel != EChannelType.Hykb));
		((VisualElement)m_btnLogin).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbLoginUtil.appChannel == EChannelType.Hykb));
		m_lblGuid = UQueryExtensions.Q<Label>(m_RootElement, "lblguid", (string)null);
		m_lbVersion = UQueryExtensions.Q<Label>(m_RootElement, "lbversion", (string)null);
		m_btnTip12 = UQueryExtensions.Q<Button>(m_RootElement, "button_12", (string)null);
		((VisualElement)m_lblGuid).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "leftbtn", (string)null);
		m_btnNotice = UQueryExtensions.Q<Button>(val, "button_notice", (string)null);
		m_btnAccount = UQueryExtensions.Q<Button>(val, "button_account", (string)null);
		m_btnAcdel = UQueryExtensions.Q<Button>(val, "button_acdel", (string)null);
		VisualElement val2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "rightbtn", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(val2, "button_call", (string)null);
		m_btnServer = UQueryExtensions.Q<Button>(val2, "button_server", (string)null);
		m_btnStyle = UQueryExtensions.Q<Button>(val2, "button_style", (string)null);
		m_lbAgreement = UQueryExtensions.Q<Label>(m_RootElement, "lb_privacy", (string)null);
		SetupAgreementLink();
		m_lbTipLine3Right = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip_line3_right", (string)null);
		((TextElement)m_lbTipLine3Right).enableRichText = true;
		((TextElement)m_lbTipLine3Right).text = "<u>ICP备案号：沪ICP备2024082415号-2A</u>";
		m_tipAgreement = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip_agreement", (string)null);
		m_btnPrivacy = UQueryExtensions.Q<Button>(m_tipAgreement, "btn_privacy", (string)null);
		m_spriteOk = UQueryExtensions.Q<VisualElement>(m_tipAgreement, "sprite_ok", (string)null);
		bool flag = zbGameSave.Instance.GetBool(zbLoginUtil.KEY_PRIVACY_AGREEMENT_ACCEPTED, defaultValue: true);
		m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLogin, (Action<ClickEvent>)OnLogin, zbAudioDefine.UI_ENTERGAME, (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHykb, (Action<ClickEvent>)OnLogin, zbAudioDefine.UI_ENTERGAME, (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNotice, (Action<ClickEvent>)OnNotice, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAccount, (Action<ClickEvent>)OnAccount, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAcdel, (Action<ClickEvent>)OnAcdel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action<ClickEvent>)OnCall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnServer, (Action<ClickEvent>)OnServer, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnStyle, (Action<ClickEvent>)OnStyle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTip12, (Action<ClickEvent>)OnTip12, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPrivacy, (Action<ClickEvent>)OnPrivacyTouched, "", (TrickleDown)0);
		((VisualElement)m_btnCall).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		((VisualElement)m_btnNotice).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		((VisualElement)m_btnTip12).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		m_tipAgreement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		((VisualElement)m_btnAcdel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannel_Login.IsOpenTSCheck()));
		bool flag2 = false;
		if (zbChannelUtil.IsNeedUseZbLogin() || zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.Hykb || zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			flag2 = true;
		}
		((VisualElement)m_btnAccount).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
		m_gameEevent.RegisterEvent(EGameEvent.Event_Login_Ack, OnLoginAck);
		m_gameEevent.RegisterEvent(EGameEvent.VisitorBind_Ack, OnLoginAck);
		m_gameEevent.RegisterEvent(EGameEvent.Server_List_Ack, OnServer_List_Ack);
	}

	public void OnServer_List_Ack(object obj)
	{
		UpdateServerName();
	}

	private void UpdateServerName()
	{
		string text = zbUtil.Language("lang.10419");
		if (zbLoginUtil.serverListAckData != null)
		{
			foreach (ServerDetail server in zbLoginUtil.serverListAckData.ServerList)
			{
				if (server.ServerId == zbLoginUtil.selectServerId)
				{
					text = server.ServerName;
					break;
				}
			}
		}
		((TextElement)m_btnServer).text = text;
	}

	public void OnLogin(ClickEvent evt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (zbChannel_Login.IsOpenTSCheck())
		{
			zbChannel_Login.OnUserReadyReq();
		}
		else if (StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1) == m_spriteOk.style.display)
		{
			zbUIMgr.Instance.BubbleInfo("请阅读并同意隐私政策和用户协议");
		}
		else
		{
			zbChannel_Login.OnUserReadyReq();
		}
	}

	public void OnNotice(ClickEvent evt)
	{
		zbLoginUtil.NoticListReq(open: true);
	}

	public void OnAccount(ClickEvent evt)
	{
		if (zbChannelUtil.IsNeedUseZbLogin())
		{
			(zbUIMgr.Instance.ShowAccount() as zbAccountWnd).ShowCenterPnl();
		}
		else if (zbChannelUtil.IsNeedShowAcSdkWnd())
		{
			zbSdkHelper.SetLoginState(state: false);
			zbSdkHelper.SwitchAccount();
			zbUIMgr.Instance.ShowSdkAccount();
		}
		else
		{
			zbSdkHelper.SetLoginState(state: false);
			zbSdkHelper.SwitchAccount();
		}
	}

	public void OnAcdel(ClickEvent evt)
	{
		zbUtil.LinkToURLWithParams("https://blfx.mydreamjoy.com/community/#/account_deletion");
	}

	public void OnCall(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	public void OnServer(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowServerList();
	}

	public void OnStyle(ClickEvent evt)
	{
	}

	public void OnTip12(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowIdentity();
	}

	public void OnPrivacyTouched(ClickEvent evt)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (m_spriteOk.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbGameSave.Instance.SetBool(zbLoginUtil.KEY_PRIVACY_AGREEMENT_ACCEPTED, value: true);
		}
		else
		{
			m_spriteOk.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbGameSave.Instance.SetBool(zbLoginUtil.KEY_PRIVACY_AGREEMENT_ACCEPTED, value: false);
		}
	}

	public override void Refresh()
	{
		base.Refresh();
		if (!m_bInit)
		{
			zbChannel_Login.OnSdkUserLogin();
			m_bInit = true;
		}
		zbHttpMsgHandler.Instance.Server_List_Req();
		((TextElement)m_lbVersion).text = "ver:" + zbVersionMgr.GetCurVersion().Replace("v", "") + "(" + zbSdkHelper.GetResVersion() + ")";
		UpdateServerName();
		SetLoginUIText();
	}

	private void SetLoginUIText(object data = null)
	{
		((TextElement)m_btnNotice).text = zbUtil.Language("lang.10415");
		((TextElement)m_btnAccount).text = zbUtil.Language("lang.10416");
		((TextElement)m_btnAcdel).text = zbUtil.Language("lang.10373");
		((TextElement)m_btnCall).text = zbUtil.Language("lang.10418");
		((TextElement)m_btnServer).text = zbUtil.Language("lang.10419");
		((TextElement)m_btnStyle).text = zbUtil.Language("lang.10421");
		((TextElement)m_btnLogin).text = zbUtil.Language("lang.10422");
		zbFontReplacer.Instance.ReplaceFontInUI(m_RootElement);
	}

	private void OnLoginAck(object data)
	{
		RefreshGuid();
	}

	public void RefreshGuid()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		((VisualElement)m_lblGuid).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (zbLoginUtil.appChannel == EChannelType.Zb)
		{
			text = $"zb:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Visitor)
		{
			text = $"Guest:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Ios || zbLoginUtil.appChannel == EChannelType.TapTap || zbLoginUtil.appChannel == EChannelType.Dyunion)
		{
			text = $"TapTap:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.HuaWei)
		{
			text = $"HW:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Vivo)
		{
			text = $"VIVO:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.XiaoMi)
		{
			text = $"MI:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Oppo)
		{
			text = $"OPPO:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Harmony)
		{
			text = $"HM:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.Hykb)
		{
			text = $"HYKB:{zbLoginUtil.guid}";
		}
		else if (zbLoginUtil.appChannel == EChannelType.AliGames)
		{
			text = $"JY:{zbLoginUtil.guid}";
		}
		((TextElement)m_lblGuid).text = text;
	}

	private void SetupAgreementLink()
	{
		if (m_lbAgreement == null)
		{
			return;
		}
		((TextElement)m_lbAgreement).enableRichText = true;
		((TextElement)m_lbAgreement).text = "我已详细阅读并同意<color=#FFD700><link=\"user_service\"><u>《用户协议》</u></link></color>和<color=#FFD700><link=\"user_privacy\"><u>《隐私政策》</u></link></color>";
		EventCallback<PointerUpLinkTagEvent> val = delegate(PointerUpLinkTagEvent evt)
		{
			if (evt.linkID == "user_service")
			{
				zbUtil.LinkToURLWithoutParams("https://blfx.mydreamjoy.com/web/user_service.html");
			}
			if (evt.linkID == "user_privacy")
			{
				zbUtil.LinkToURLWithoutParams("https://blfx.mydreamjoy.com/web/user_privacy.html");
			}
		};
		if (((VisualElement)m_lbAgreement).userData is EventCallback<PointerUpLinkTagEvent> val2)
		{
			((CallbackEventHandler)m_lbAgreement).UnregisterCallback<PointerUpLinkTagEvent>(val2, (TrickleDown)0);
		}
		((VisualElement)m_lbAgreement).userData = val;
		((CallbackEventHandler)m_lbAgreement).RegisterCallback<PointerUpLinkTagEvent>(val, (TrickleDown)0);
	}
}
