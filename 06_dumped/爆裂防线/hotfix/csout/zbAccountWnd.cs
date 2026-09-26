using UnityEngine.UIElements;

public class zbAccountWnd : zbBaseWnd
{
	private zbActLogin m_acLoginPnl;

	private zbActReg m_acRegPnl;

	private zbActRegAck m_acRegAckPnl;

	private zbAcCenter m_centerPnl;

	protected override void Awake()
	{
		base.Awake();
		zbAccountMgr.Init();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "login", (string)null);
		VisualElement root2 = UQueryExtensions.Q<VisualElement>(m_RootElement, "register", (string)null);
		VisualElement root3 = UQueryExtensions.Q<VisualElement>(m_RootElement, "regack", (string)null);
		VisualElement root4 = UQueryExtensions.Q<VisualElement>(m_RootElement, "usercenter", (string)null);
		m_acLoginPnl = new zbActLogin(root, this);
		m_acRegPnl = new zbActReg(root2, this);
		m_acRegAckPnl = new zbActRegAck(root3, this);
		m_centerPnl = new zbAcCenter(root4, this);
		ShowLoginPnl();
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_Account_VisitorReg, OnVisitorRegAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Account_Register, OnAccountRegAck);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Account_Login, OnAccountLoginAck);
	}

	public void ShowLoginPnl()
	{
		m_acLoginPnl.SetVisible(visi: true);
		m_acRegPnl.SetVisible(visi: false);
		m_acRegAckPnl.SetVisible(visi: false);
		m_centerPnl.SetVisible(visi: false);
		m_acLoginPnl.RefreshAccountInfo();
	}

	public void ShowRegPnl()
	{
		m_acLoginPnl.SetVisible(visi: false);
		m_acRegPnl.SetVisible(visi: true);
		m_acRegAckPnl.SetVisible(visi: false);
		m_centerPnl.SetVisible(visi: false);
	}

	public void ShowRegAckPnl()
	{
		m_acLoginPnl.SetVisible(visi: false);
		m_acRegPnl.SetVisible(visi: false);
		m_centerPnl.SetVisible(visi: false);
		m_acRegAckPnl.SetVisible(visi: true);
	}

	public void ShowCenterPnl()
	{
		m_acLoginPnl.SetVisible(visi: false);
		m_acRegPnl.SetVisible(visi: false);
		m_centerPnl.SetVisible(visi: true);
		m_acRegAckPnl.SetVisible(visi: false);
		m_centerPnl.SetAccountInfo();
	}

	private void OnVisitorRegAck(object data)
	{
		ShowRegPnl();
		m_acRegPnl.SetFastLoginInfo();
	}

	private void OnAccountRegAck(object data)
	{
		ShowRegAckPnl();
		m_acRegAckPnl.SetAccountInfo();
	}

	private void OnAccountLoginAck(object data)
	{
		Hide();
		zbAccountMgr.SaveLastLoginAccount();
	}
}
