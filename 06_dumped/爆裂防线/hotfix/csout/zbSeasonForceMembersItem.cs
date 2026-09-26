using System;
using Proto.Types;
using UnityEngine.UIElements;

public class zbSeasonForceMembersItem : zbViewBaseItem
{
	private zbSeasonForceMembersWnd m_wndParent;

	private VisualElement m_playerInfo;

	private Label m_lbContributeVal;

	private Label m_lbMerit;

	private Button m_btnPosition;

	private SeasonMemberInfo m_data;

	public zbSeasonForceMembersItem()
	{
		VisualTreeAsset val = zbAssetLoader.Instance.LoadResouce<VisualTreeAsset>("ui/uxml/season/tb_seasonForceMembers.uxml");
		((VisualElement)this).Add((VisualElement)(object)val.Instantiate());
	}

	public void OnEnable(zbSeasonForceMembersWnd wndParent)
	{
		m_wndParent = wndParent;
		m_lbContributeVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_contributeVal", (string)null);
		m_lbMerit = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_merit", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		m_btnPosition = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_position", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPosition, (Action)OnPosition, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Refresh(SeasonMemberInfo data)
	{
		m_data = data;
		zbSelf.Instance.rankHandler.InitPlayerInfo(m_playerInfo, m_data.RoleBaseInfo);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void OnTouch()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_data.RoleBaseInfo.RoleId);
	}

	private void OnPosition()
	{
		zbUIMgr.Instance.ShowSeasonForceMemberManage(m_data);
	}
}
