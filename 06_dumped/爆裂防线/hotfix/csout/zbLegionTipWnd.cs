using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLegionTipWnd : zbBaseWnd
{
	private Label m_title;

	private Button m_btnClose;

	private Label m_iconFlag;

	private Label m_lbName;

	private Label m_lbActive;

	private Label m_lbActiveVal;

	private Label m_lbJoinVal;

	private Label m_lbNeedReq;

	private Label m_lbMember;

	private Label m_lbLeaderVal;

	private Label m_lbLegionIdVal;

	private Button m_btnCopyLegionId;

	private Label m_lbLegionLv;

	private Label m_lbLegionNotice;

	private LegionBaseInfo m_tipsInfo;

	private LegionSetInfo m_setInfo;

	private Button m_btnConfirm;

	private Button m_btnTouch;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_iconFlag = UQueryExtensions.Q<Label>(m_RootElement, "icon_flag", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbActive = UQueryExtensions.Q<Label>(m_RootElement, "lb_active", (string)null);
		m_lbActiveVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_activeVal", (string)null);
		m_lbJoinVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_joinVal", (string)null);
		m_lbNeedReq = UQueryExtensions.Q<Label>(m_RootElement, "lb_needReq", (string)null);
		m_lbMember = UQueryExtensions.Q<Label>(m_RootElement, "lb_member", (string)null);
		m_lbLeaderVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_leaderVal", (string)null);
		m_lbLegionIdVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionIdVal", (string)null);
		m_btnCopyLegionId = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopyLegionId, (Action)OnCopy, "", (TrickleDown)0);
		m_lbLegionLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionVal", (string)null);
		m_lbLegionNotice = UQueryExtensions.Q<Label>(m_RootElement, "lb_legionNotice", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(LegionTipsInfo legionTipsInfo)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		m_tipsInfo = legionTipsInfo.LegionBaseInfo;
		m_setInfo = legionTipsInfo.LegionSetInfo;
		bool flag = zbSelf.Instance.legionHandler.HasJoinLegion();
		((VisualElement)m_btnConfirm).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		((TextElement)m_lbName).text = m_tipsInfo.LegionName;
		((VisualElement)m_iconFlag).style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(m_tipsInfo.Icon));
		((TextElement)m_lbActiveVal).text = m_tipsInfo.Active.ToString();
		((TextElement)m_lbJoinVal).text = zbUtil.Language("lang.10411") + ":Lv." + m_tipsInfo.JoinLvl;
		((TextElement)m_lbNeedReq).text = ((m_tipsInfo.Confirm == EBoolState.False) ? zbUtil.Language("lang.10044") : zbUtil.Language("lang.10045"));
		((TextElement)m_lbMember).text = m_tipsInfo.MemberCount + "/" + m_tipsInfo.MemberMax;
		((TextElement)m_lbLeaderVal).text = legionTipsInfo.LeaderBaseInfo.RoleName.ToString();
		((TextElement)m_lbLegionIdVal).text = m_tipsInfo.LegionId.ToString();
		((TextElement)m_lbLegionNotice).text = ((m_setInfo.LegionSign == "") ? zbUtil.Language("lang.11285") : m_setInfo.LegionSign);
		((TextElement)m_lbLegionLv).text = "Lv." + m_tipsInfo.Level;
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnConfirm(ClickEvent evt)
	{
		if (CheckCanApplyLegion())
		{
			zbWsMsgHandler.Instance.Legion_Apply_Req(m_tipsInfo.LegionId);
		}
	}

	private void OnCopy()
	{
		zbUtil.CopyString(m_tipsInfo.LegionId.ToString());
	}

	private bool CheckCanApplyLegion()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Legion))
		{
			return false;
		}
		if (zbServerData.sGameData.RoleBaseInfo.Level < m_tipsInfo.JoinLvl)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10516"));
			return false;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		if (zbServerData.sGameData.LegionSelfInfo.JoinTm - serverTime > 0)
		{
			zbUIMgr.Instance.ShowLegionClearCd();
			return false;
		}
		if (zbSelf.Instance.legionHandler.HasJoinLegion())
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10070"));
			return false;
		}
		return true;
	}
}
