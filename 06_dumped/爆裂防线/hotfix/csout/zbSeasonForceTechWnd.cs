using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonForceTechWnd : zbTabWnd
{
	private VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[3];

	private Button[] m_tabButtons = (Button[])(object)new Button[3];

	private VisualElement[] m_notifyTabs = (VisualElement[])(object)new VisualElement[3];

	private zbSeasonForceTechTabUpgrade m_moduleUpgrade;

	private zbSeasonForceTechTabCombat m_moduleCombat;

	private zbSeasonForceTechTabOffice m_moduleOffice;

	private Button m_btnClose;

	private Sprite m_cachedBtnSprite;

	private Sprite m_cachedBtnSpriteSel;

	public WallSubjectDetail m_data;

	public zbWallSubjectCsvRecord wallSubjectRecord;

	protected override void OnEnable()
	{
		base.OnEnable();
		for (int i = 0; i < 3; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
			m_notifyTabs[i] = zbUtil.CreateNotifyTag((VisualElement)(object)m_tabButtons[i]);
		}
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_cachedBtnSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_208x73_1.png");
		m_cachedBtnSpriteSel = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_208x73_0.png");
		InitTab(m_tabButtons, m_tabLayers, null);
	}

	public void RefreshCustom(int tabIndex = 0)
	{
	}

	public override void DoSelectTab()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_tabButtons.Length; i++)
		{
			((VisualElement)m_tabButtons[i]).style.backgroundImage = new StyleBackground((i == m_currTab) ? m_cachedBtnSpriteSel : m_cachedBtnSprite);
		}
		switch (m_currTab)
		{
		case 0:
			if (m_moduleUpgrade == null)
			{
				m_moduleUpgrade = new zbSeasonForceTechTabUpgrade();
				m_moduleUpgrade.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleUpgrade.Refresh();
			break;
		case 1:
			if (m_moduleCombat == null)
			{
				m_moduleCombat = new zbSeasonForceTechTabCombat();
				m_moduleCombat.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleCombat.Refresh();
			break;
		case 2:
			if (m_moduleOffice == null)
			{
				m_moduleOffice = new zbSeasonForceTechTabOffice();
				m_moduleOffice.OnEnable(this, m_tabLayers[m_currTab]);
			}
			m_moduleOffice.Refresh();
			break;
		}
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (m_data != null)
		{
			m_notifyTabs[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasLevelNotify(m_data)));
			m_notifyTabs[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasZhanLingNotify(m_data)));
			m_notifyTabs[2].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallSubjectHandler.HasRouletteNotify(m_data)));
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
