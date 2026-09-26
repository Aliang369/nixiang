using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonRechargeWnd : zbTabWnd
{
	private Button m_btnBack;

	private VisualElement[] m_panels = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_btns = (Button[])(object)new Button[3];

	private zbSeasonRechargeTabZhanling m_zhanlingModule;

	private zbSeasonRechargeTabPrivilegeCard m_privilegeCardModule;

	private zbSeasonRechargeTabPackage m_packageModule;

	private Sprite m_balanceTabSeBg;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_panels[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_btns[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn" + i, (string)null);
		}
		m_balanceTabSeBg = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/combat/balance_tabse.png");
		InitTab(m_btns, m_panels, null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom()
	{
	}

	public override void DoSelectTab()
	{
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			if (m_zhanlingModule == null)
			{
				m_zhanlingModule = new zbSeasonRechargeTabZhanling();
				m_zhanlingModule.OnEnable(this, m_panels[0]);
			}
			m_zhanlingModule.Refresh();
			break;
		case 1:
			if (m_privilegeCardModule == null)
			{
				m_privilegeCardModule = new zbSeasonRechargeTabPrivilegeCard();
				m_privilegeCardModule.OnEnable(this, m_panels[1]);
			}
			m_privilegeCardModule.Refresh();
			break;
		case 2:
			if (m_packageModule == null)
			{
				m_packageModule = new zbSeasonRechargeTabPackage();
				m_packageModule.OnEnable(this, m_panels[1]);
			}
			m_packageModule.Refresh();
			break;
		}
		SetTabBg();
	}

	private void SetTabBg()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 3; i++)
		{
			((VisualElement)m_btns[i]).style.backgroundImage = new StyleBackground((i == m_currTab) ? m_balanceTabSeBg : null);
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
