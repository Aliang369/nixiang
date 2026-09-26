using System;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMainQuestJoinWnd : zbTabWnd
{
	public VisualTreeAsset m_itemTA;

	private readonly VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[2];

	private readonly Button[] m_tabButtons = (Button[])(object)new Button[2];

	private Button m_Button_close;

	private Button m_Button_AllClose;

	private Sprite m_cachedBtnSprite;

	private Sprite m_cachedBtnSpriteSel;

	private zbMainQuestJoinLegion m_panelLegion;

	private zbMainQuestJoinFriend m_panelFriend;

	public EFuncType m_funcType;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/sub/tb_mainQuestJoin.uxml");
		m_Button_close = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_Button_AllClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
		}
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_close, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_AllClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_cachedBtnSprite = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_192x72_1.png");
		m_cachedBtnSpriteSel = zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/tab_192x72_0.png");
		InitTab(m_tabButtons, m_tabLayers, null);
		AddDark();
	}

	public override bool IsCanOpen(int tabIndex)
	{
		if (tabIndex == 1 && zbServerData.sGameData.LegionSelfInfo.LegionId <= 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.EErrorCode.30523"));
			return false;
		}
		return true;
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
			if (m_panelFriend == null)
			{
				m_panelFriend = new zbMainQuestJoinFriend(m_tabLayers[0], this);
			}
			m_panelFriend.Refresh(m_currTab);
			break;
		case 1:
			if (m_panelLegion == null)
			{
				m_panelLegion = new zbMainQuestJoinLegion(m_tabLayers[1], this);
			}
			m_panelLegion.Refresh(m_currTab);
			if (zbSelf.Instance.legionHandler.memberList == null)
			{
				zbWsMsgHandler.Instance.Legion_Members_Req();
			}
			else
			{
				m_panelLegion.RefreshList();
			}
			break;
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	public void RefreshCustom(int tabIndex, EFuncType funcType)
	{
		m_funcType = funcType;
		SelectTab(0);
	}
}
