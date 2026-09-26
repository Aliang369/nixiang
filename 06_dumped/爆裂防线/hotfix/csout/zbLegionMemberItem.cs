using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionMemberItem : zbViewBaseItem
{
	private Label m_iconFrame;

	private Label m_iconAvatar;

	private Label m_iconPower;

	private Label m_lbName;

	private Label m_lbContributeVal;

	private Label m_operatePanel;

	private Label m_lbStatus;

	private Button m_btnAward;

	private Button m_btnOperate;

	private Button m_btnPosition;

	private Button m_btnCheck;

	private VisualElement m_iconPosition;

	private zbLegionMembersWnd m_parent;

	private LegionMemberDetail m_item;

	private VisualElement m_playerInfo;

	[NonSerialized]
	public int Index;

	public zbLegionMemberItem(VisualTreeAsset tAsset, zbLegionMembersWnd parent)
	{
		m_parent = parent;
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_iconFrame = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_frame", (string)null);
		m_iconAvatar = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_avatar", (string)null);
		m_iconPower = UQueryExtensions.Q<Label>((VisualElement)(object)this, "icon_power", (string)null);
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_name", (string)null);
		m_lbContributeVal = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_contributeVal", (string)null);
		m_operatePanel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "operate_panel", (string)null);
		m_lbStatus = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_status", (string)null);
		m_iconPosition = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "icon_position", (string)null);
		m_btnAward = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_award", (string)null);
		m_btnPosition = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_position", (string)null);
		m_btnOperate = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_operate", (string)null);
		m_btnCheck = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_check", (string)null);
		m_playerInfo = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "playerInfo", (string)null);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOperate, (Action)OnClick, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPosition, (Action)OnClickPosition, "", (TrickleDown)0);
		parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheck, (Action)OnCheck, "", (TrickleDown)0);
	}

	public void Init(LegionMemberDetail item, int index)
	{
		Index = index;
		m_item = item;
		((TextElement)m_lbName).text = item.RoleBaseInfo.RoleName;
		((TextElement)m_iconPower).text = item.RoleBaseInfo.TeamAttack.ToString();
		((TextElement)m_lbContributeVal).text = item.Active.ToString();
		RefreshOnlineState(item.RoleLoginInfo, item.RoleBaseInfo.RoleId);
		OperateBtnShow();
		InitPlayerInfo();
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	private void InitPlayerInfo()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbName).text = m_item.RoleBaseInfo.RoleName;
		((TextElement)m_iconPower).text = m_item.RoleBaseInfo.TeamAttack.ToString();
		bool flag = zbSelf.Instance.legionHandler.GetOfficialById((int)m_item.RoleBaseInfo.RoleId) == ELegionOfficial.Leader;
		bool flag2 = zbSelf.Instance.legionHandler.GetOfficialById((int)m_item.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser;
		m_iconPosition.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!(flag | flag2)));
		if (flag | flag2)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? "ui/image/legion/icon_leader.png" : "ui/image/legion/icon_adviser.png");
			m_iconPosition.style.backgroundImage = new StyleBackground(val);
		}
		string icon = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(m_item.RoleBaseInfo.HeadId).icon;
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null).style.backgroundImage = new StyleBackground(val2);
		string icon2 = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(m_item.RoleBaseInfo.HeadFrameId).icon;
		Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(icon2);
		UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null).style.backgroundImage = new StyleBackground(val3);
		((TextElement)UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null)).text = m_item.RoleBaseInfo.Level.ToString();
	}

	private void RefreshOnlineState(RoleLoginInfo loginInfo, long roleId)
	{
		if (loginInfo.State == ERoleState.Online)
		{
			((TextElement)m_lbStatus).text = "<color=#2DF421>" + zbUtil.Language("lang.10034") + "</color>";
		}
		else
		{
			((TextElement)m_lbStatus).text = zbSelf.Instance.friendHandler.GetPassTimeShort(zbSelf.Instance.timeHandler.GetServerTime() - loginInfo.LoginTm);
		}
	}

	private void OnClick()
	{
		if (zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Member)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10080"));
		}
		else
		{
			m_parent.OnClickOperate(this);
		}
	}

	private void OnClickPosition()
	{
		zbUIMgr.Instance.ShowLegionPosition(m_item.RoleBaseInfo.RoleId);
		HidePanel();
	}

	private void OnCheck()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_item.RoleBaseInfo.RoleId);
	}

	public void ShowPanel()
	{
		((VisualElement)m_operatePanel).visible = true;
	}

	public void HidePanel()
	{
		((VisualElement)m_operatePanel).visible = false;
	}

	private void OperateBtnShow()
	{
		((VisualElement)m_btnOperate).visible = ((m_item.RoleBaseInfo.RoleId != zbServerData.sGameData.RoleBaseInfo.RoleId) ? true : false);
		if (zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser && m_item.RoleBaseInfo.RoleId == zbSelf.Instance.legionHandler.legionInfo.LegionSetInfo.LeaderId)
		{
			((VisualElement)m_btnOperate).visible = false;
		}
		if (zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser && zbSelf.Instance.legionHandler.GetOfficialById((int)m_item.RoleBaseInfo.RoleId) == ELegionOfficial.Adviser)
		{
			((VisualElement)m_btnOperate).visible = false;
		}
	}
}
