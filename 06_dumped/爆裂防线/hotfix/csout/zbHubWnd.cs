using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHubWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private Button[] m_tabButtons = (Button[])(object)new Button[2];

	private zbHubTabNotice m_moduleNotice;

	private zbHubTabCommunity m_moduleCommunity;

	private Button m_btnCall;

	private Button m_btnShare;

	private Button m_btnClose;

	private VisualElement m_newNotice;

	private Sprite m_tabSelectedBackground;

	protected override void OnEnable()
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
		}
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "btn_bottom", (string)null);
		m_btnCall = UQueryExtensions.Q<Button>(val, "btn_call", (string)null);
		m_btnShare = UQueryExtensions.Q<Button>(val, "btn_share", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCall, (Action)OnCall, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShare, (Action)OnShare, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_tabSelectedBackground = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/vip/bar/bar_tab.png");
		m_newNotice = zbUtil.CreateNewTag((VisualElement)(object)m_tabButtons[0], (Vector2?)new Vector2(-15f, -20f), 1f);
		InitTab(m_tabButtons, m_tabLayers, null);
		val.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
	}

	public void RefreshCustom(int tabIndex = 0)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (zbChannel_Login.IsOpenTSCheck())
		{
			((VisualElement)m_tabButtons[1]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			tabIndex = 0;
		}
		if (tabIndex != 0)
		{
			zbLoginUtil.NoticListReq(open: false);
		}
		SelectTab(tabIndex);
		((VisualElement)m_btnShare).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbChannelUtil.IsOpenShare()));
		RefreshNew();
	}

	public override void DoSelectTab()
	{
		RefreshTabBackground();
		switch (m_currTab)
		{
		case 0:
			if (m_moduleNotice == null)
			{
				m_moduleNotice = new zbHubTabNotice();
				m_moduleNotice.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleNotice.Refresh();
			break;
		case 1:
			if (m_moduleCommunity == null)
			{
				m_moduleCommunity = new zbHubTabCommunity();
				m_moduleCommunity.OnEnable(this, m_tabLayers[m_currTab]);
			}
			break;
		}
		RefreshNew();
	}

	private void RefreshTabBackground()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = (StyleBackground)((i == m_currTab) ? new StyleBackground(m_tabSelectedBackground) : StyleBackground.op_Implicit((StyleKeyword)3));
		}
	}

	public void RefreshNew()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		m_newNotice.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbNewNoticeHandler.HasNewPoint()));
	}

	private void OnCall()
	{
		zbUIMgr.Instance.ShowSupportContact();
	}

	private void OnShare()
	{
		zbSdkHelper.DoShare();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnSetLang_Ack(object data)
	{
		DestroySelfWnd();
	}
}
