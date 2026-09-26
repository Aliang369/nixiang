using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionDetailWnd : zbBaseWnd
{
	private Button m_btnBack;

	private Button m_btnDetail;

	private Button m_btnNotice;

	private Label m_lbTitle;

	private Label m_lbIcon;

	private Label m_lbName;

	private Label m_lbId;

	private Label m_lbIdVal;

	private Label m_lbLeader;

	private Label m_lbLeaderVal;

	private Label m_lbLv;

	private Label m_lbLvVal;

	private Label m_lbPro;

	private Label m_lbProVal;

	private Label m_lbProText;

	private Label m_lbLvBar;

	private Label m_lbMember;

	private Label m_lbMemberVal;

	private Label m_lbActivity;

	private Label m_lbActivityVal;

	private Label m_lbPowerVal;

	private Label m_lbNoticeTitle;

	private Label m_lbNoticeContent;

	private Button m_btnEditLegion;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnDetail = UQueryExtensions.Q<Button>(m_RootElement, "btn_detail", (string)null);
		m_btnNotice = UQueryExtensions.Q<Button>(m_RootElement, "btn_notice", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_lbIcon = UQueryExtensions.Q<Label>(m_RootElement, "icon_flag", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbId = UQueryExtensions.Q<Label>(m_RootElement, "lb_id", (string)null);
		m_lbIdVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_idVal", (string)null);
		m_lbLeader = UQueryExtensions.Q<Label>(m_RootElement, "lb_leader", (string)null);
		m_lbLeaderVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_leaderVal", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_lbLvVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvVal", (string)null);
		m_lbPro = UQueryExtensions.Q<Label>(m_RootElement, "lb_pro", (string)null);
		m_lbProVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_proVal", (string)null);
		m_lbProText = UQueryExtensions.Q<Label>(m_RootElement, "lb_proText", (string)null);
		m_lbLvBar = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvBar", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbMemberVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_memberVal", (string)null);
		m_lbActivity = UQueryExtensions.Q<Label>(m_RootElement, "lb_activity", (string)null);
		m_lbActivityVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_activityVal", (string)null);
		m_lbNoticeTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_noticeTitle", (string)null);
		m_lbNoticeContent = UQueryExtensions.Q<Label>(m_RootElement, "lb_noticeContent", (string)null);
		m_btnEditLegion = UQueryExtensions.Q<Button>(m_RootElement, "btn_editLegion", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_lbNoticeContent, (Action<ClickEvent>)OnClickNoticeEdit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetail, (Action<ClickEvent>)OnClickDetailBtn, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNotice, (Action<ClickEvent>)OnClickNoticeEdit, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEditLegion, (Action)OnEdit, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_ChangeInfo, OnUpdateEvent);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Data, OnUpdateEventLegionNotice);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_Members, OnUpdateEventLegionMember);
	}

	public override void Refresh()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		UpdateLegionDetail();
		((VisualElement)m_btnNotice).visible = zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) != ELegionOfficial.Member;
		((VisualElement)m_btnEditLegion).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) != ELegionOfficial.Leader));
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnClickNoticeEdit(ClickEvent evt)
	{
		if (zbSelf.Instance.legionHandler.GetOfficialById((int)zbServerData.sGameData.RoleBaseInfo.RoleId) != ELegionOfficial.Member)
		{
			zbUIMgr.Instance.ShowLegionNotice();
		}
	}

	private void OnClickDetailBtn(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowLegionPrivilege();
	}

	private void UpdateLegionDetail()
	{
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		bool num = zbSelf.Instance.legionHandler.memberList != null && zbSelf.Instance.legionHandler.memberList.Count > 0;
		LegionBaseInfo legionBaseInfo = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo;
		LegionSetInfo setInfo = zbSelf.Instance.legionHandler.legionInfo.LegionSetInfo;
		((TextElement)m_lbName).text = legionBaseInfo.LegionName;
		((TextElement)m_lbIdVal).text = legionBaseInfo.LegionId.ToString();
		if (num)
		{
			LegionMemberDetail legionMemberDetail = ((IEnumerable<LegionMemberDetail>)zbSelf.Instance.legionHandler.memberList).First((LegionMemberDetail m) => m.RoleBaseInfo.RoleId == setInfo.LeaderId);
			((TextElement)m_lbLeaderVal).text = legionMemberDetail.RoleBaseInfo.RoleName;
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_Members_Req(bOpenLegionMembersWnd: true);
		}
		((TextElement)m_lbMemberVal).text = legionBaseInfo.MemberCount + "/" + legionBaseInfo.MemberMax;
		((TextElement)m_lbActivityVal).text = legionBaseInfo.Active.ToString();
		LegionInfo legionInfo = zbSelf.Instance.legionHandler.legionInfo;
		int level = legionInfo.LegionBaseInfo.Level;
		((TextElement)m_lbLvVal).text = $"Lv.{level}";
		zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(level);
		zbLegionLevelCsvRecord recordByKey = zbDataConfig.Instance.legionLevelCsvTable.GetRecordByKey(level + 1);
		bool num2 = recordByKey != null;
		int legionExp = legionInfo.LegionSetInfo.LegionExp;
		if (num2)
		{
			int exp = recordByKey.exp;
			((TextElement)m_lbProText).text = $"{legionExp} / {exp}";
			((VisualElement)m_lbProVal).style.width = new StyleLength(new Length((float)legionExp / (float)exp * 100f, (LengthUnit)1));
			((TextElement)m_lbNoticeContent).text = setInfo.LegionSign;
		}
		else
		{
			((TextElement)m_lbProText).text = $"{legionExp} / Max";
			((VisualElement)m_lbProVal).style.width = new StyleLength(new Length(100f, (LengthUnit)1));
		}
		((VisualElement)m_lbIcon).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(legionBaseInfo.Icon));
	}

	private void OnUpdateEventLegionNotice(object data)
	{
		((TextElement)m_lbNoticeContent).text = zbSelf.Instance.legionHandler.legionInfo.LegionSetInfo.LegionSign;
	}

	private void OnUpdateEventLegionMember(object data)
	{
		Refresh();
	}

	private void OnUpdateEvent(object data)
	{
		Refresh();
	}

	private void OnEdit()
	{
		zbUIMgr.Instance.ShowLegionChangeInfo();
	}
}
