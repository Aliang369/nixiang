using System;
using UnityEngine.UIElements;

public class zbLegionWarRankAwardWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	public Button[] m_tabButtons = (Button[])(object)new Button[2];

	private Button m_btnClose;

	private VisualElement m_spriteTabSel;

	private zbLegionWarRankAwardTabPlayer m_modulePlayer;

	private zbLegionWarRankAwardTabLegion m_moduleLegion;

	private int m_playerRank;

	private int m_legionRank;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spriteTabSel = UQueryExtensions.Q<VisualElement>(m_RootElement, "selectTab", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab_btn" + i, (string)null);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		InitTab(m_tabButtons, m_tabLayers, null);
	}

	public void RefreshCustom(int playerRank, int legionRank, int tabIndex)
	{
		m_playerRank = playerRank;
		m_legionRank = legionRank;
		SelectTab(tabIndex);
	}

	public override void DoSelectTab()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		base.DoSelectTab();
		switch (m_currTab)
		{
		case 0:
			if (m_modulePlayer == null)
			{
				m_modulePlayer = new zbLegionWarRankAwardTabPlayer();
				m_modulePlayer.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_modulePlayer.Refresh(m_playerRank);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
			break;
		case 1:
			if (m_moduleLegion == null)
			{
				m_moduleLegion = new zbLegionWarRankAwardTabLegion();
				m_moduleLegion.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleLegion.Refresh(m_legionRank);
			m_spriteTabSel.style.left = StyleLength.op_Implicit(new Length(360f, (LengthUnit)0));
			break;
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
