using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbFriendWnd : zbTabWnd
{
	public VisualTreeAsset m_itemTA;

	private Button m_btnClose;

	private readonly VisualElement[] m_tabLayers = (VisualElement[])(object)new VisualElement[4];

	public readonly Button[] m_tabButtons = (Button[])(object)new Button[4];

	private zbFriendTabList m_panelFriend;

	public zbFriendTabAdd m_panelAdd;

	private zbFriendTabApply m_panelApply;

	private zbFriendTabShield m_panelShield;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_itemTA = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/friend/tb_friendItem.uxml");
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		for (int i = 0; i < 4; i++)
		{
			m_tabLayers[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "panel" + i, (string)null);
			m_tabButtons[i] = UQueryExtensions.Q<Button>(m_RootElement, "tab" + i, (string)null);
		}
		InitTab(m_tabButtons, m_tabLayers, null);
		CreateMoneyPanel(new EMoneyType[1] { EMoneyType.Tili });
		AddDark();
	}

	public override void Refresh()
	{
		base.Refresh();
		SelectTab(0);
		zbGuide.Instance.SetGuideBool(EGuideStep.guide_step_Friend);
	}

	public override void DoSelectTab()
	{
		switch (m_currTab)
		{
		case 0:
			if (m_panelFriend == null)
			{
				m_panelFriend = new zbFriendTabList(m_tabLayers[0], this);
			}
			m_panelFriend.Refresh(m_currTab);
			break;
		case 1:
			if (m_panelAdd == null)
			{
				m_panelAdd = new zbFriendTabAdd(m_tabLayers[1], this);
			}
			m_panelAdd.Refresh(m_currTab);
			break;
		case 2:
			if (m_panelApply == null)
			{
				m_panelApply = new zbFriendTabApply(m_tabLayers[2], this);
			}
			m_panelApply.Refresh(m_currTab);
			break;
		case 3:
			if (m_panelShield == null)
			{
				m_panelShield = new zbFriendTabShield(m_tabLayers[3], this);
			}
			m_panelShield.Refresh(m_currTab);
			break;
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_tabButtons[0], "red_dot", (string)null).visible = zbSelf.Instance.friendHandler.HasFriendListRedPoint();
		UQueryExtensions.Q<VisualElement>((VisualElement)(object)m_tabButtons[2], "red_dot", (string)null).visible = zbSelf.Instance.friendHandler.HasFriendsApply();
	}
}
