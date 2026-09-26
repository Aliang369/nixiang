using System;
using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFriendItem : zbViewBaseItem
{
	private RoleBaseInfo m_roleBaseInfo;

	private FriendsDetail m_friendsDetail;

	private zbFriendWnd m_Parent;

	private VisualElement m_headFrame;

	private VisualElement m_head;

	private Label m_lbLevel;

	private VisualElement m_btnNoode0;

	private Button m_btnNoode1;

	private VisualElement m_btnNoode2;

	private Label m_lbName;

	private Label m_lbFv;

	private Label m_lbTime;

	private Button m_btnSend;

	private Button m_btnGet;

	private Button m_btnMessage;

	private Button m_btnConfirm;

	private Button m_btnCancel;

	private int m_currTab;

	private Button m_btnTouch;

	private RoleLoginInfo m_loginInfo;

	private bool m_isApplied;

	private Label m_lbApplyState;

	private readonly Dictionary<EBoolState, (bool visible, string texturePath, bool bGray)> ActionHim_State = new Dictionary<EBoolState, (bool, string, bool)>
	{
		{
			EBoolState.False,
			(true, "ui/image/friend/friend_loveSend.png", false)
		},
		{
			EBoolState.True,
			(true, "ui/image/friend/friend_option2.png", true)
		}
	};

	public zbFriendItem(zbFriendWnd parent, VisualTreeAsset tAsset)
	{
		((VisualElement)this).Add((VisualElement)(object)tAsset.Instantiate());
		m_Parent = parent;
		m_lbName = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Name", (string)null);
		m_lbFv = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_Fv", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>((VisualElement)(object)this, "Label_Time", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "headFrame", (string)null);
		m_head = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "head", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_level", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_touch", (string)null);
		m_btnNoode0 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "btn_noode0", (string)null);
		m_btnNoode1 = UQueryExtensions.Q<Button>((VisualElement)(object)this, "btn_noode1", (string)null);
		m_btnNoode2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "btn_noode2", (string)null);
		m_btnSend = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_send", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_get", (string)null);
		m_btnMessage = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_message", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_btnNoode2, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_btnNoode2, "btn_cancel", (string)null);
		m_lbApplyState = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_applyState", (string)null);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSend, (Action<ClickEvent>)OnSend, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMessage, (Action<ClickEvent>)OnChat, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnCancel, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNoode1, (Action<ClickEvent>)OnAdd, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(RoleBaseInfo info, int currTab, RoleLoginInfo loginInfo = null, bool isApplied = false)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		m_roleBaseInfo = info;
		m_currTab = currTab;
		m_loginInfo = loginInfo;
		m_isApplied = isApplied;
		refreshItems();
		if (currTab == 0 && m_friendsDetail != null)
		{
			UpdateActionHim(m_friendsDetail.ActionHim);
		}
		switch (currTab)
		{
		case 0:
			m_btnNoode0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnNoode1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_btnNoode2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			break;
		case 1:
			m_btnNoode0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnNoode1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_isApplied || zbSelf.Instance.friendHandler.IsMyFriend(m_roleBaseInfo.RoleId)) ? 1 : 0));
			m_btnNoode2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			break;
		default:
			m_btnNoode0.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnNoode1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_btnNoode2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			break;
		}
		((VisualElement)m_btnGet).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void refreshItems()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		foreach (FriendsDetail friends in zbSelf.Instance.friendHandler.friendsList)
		{
			if (friends.RoleBaseInfo.RoleId == m_roleBaseInfo.RoleId)
			{
				m_friendsDetail = friends;
				break;
			}
		}
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_roleBaseInfo.HeadId);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_roleBaseInfo.HeadFrameId);
		m_head.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		m_headFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		((TextElement)m_lbLevel).text = m_roleBaseInfo.Level.ToString();
		((TextElement)m_lbName).text = m_roleBaseInfo.RoleName;
		((TextElement)m_lbFv).text = m_roleBaseInfo.TeamAttack.ToString();
		if (m_currTab == 0 && m_friendsDetail != null)
		{
			UpdateActionHim(m_friendsDetail.ActionHim);
			RefreshOnlineState(m_friendsDetail.RoleLoginInfo, m_friendsDetail.RoleBaseInfo.RoleId);
		}
		else if (m_currTab == 1 && m_roleBaseInfo != null && m_loginInfo != null)
		{
			RefreshOnlineState(m_loginInfo, m_roleBaseInfo.RoleId);
		}
		((VisualElement)m_lbApplyState).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!m_isApplied));
		if (m_isApplied)
		{
			((TextElement)m_lbApplyState).text = zbUtil.Language("lang.10610");
		}
	}

	private void RefreshOnlineState(RoleLoginInfo loginInfo, long roleId)
	{
		if (loginInfo.State == ERoleState.Online)
		{
			((TextElement)m_lbTime).text = "<color=#2DF421>" + zbUtil.Language("lang.10034") + "</color>";
		}
		else
		{
			((TextElement)m_lbTime).text = zbSelf.Instance.friendHandler.GetPassTimeShort(zbSelf.Instance.timeHandler.GetServerTime() - loginInfo.LoginTm);
		}
	}

	private void OnSend(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Friends_ActionGive_Req((int)m_roleBaseInfo.RoleId);
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10437"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
	}

	private void OnGet(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Friends_ActionCollect_Req((int)m_roleBaseInfo.RoleId);
		zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10438"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
	}

	private void OnChat(ClickEvent evt)
	{
		m_Parent.Hide();
		zbUIMgr.Instance.ShowChat();
	}

	private void OnConfirm(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Friends_ApplyPass_Req((int)m_roleBaseInfo.RoleId);
	}

	private void OnCancel(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Friends_ApplyRefuse_Req((int)m_roleBaseInfo.RoleId);
	}

	private void OnTouch(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleBaseInfo.RoleId);
	}

	private void UpdateActionHim(EBoolState state)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (ActionHim_State.TryGetValue(state, out (bool, string, bool) value))
		{
			((VisualElement)m_btnSend).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!value.Item1));
			if (value.Item1)
			{
				((VisualElement)m_btnSend).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(value.Item2));
				((VisualElement)m_btnSend).SetEnabled(!value.Item3);
			}
		}
	}

	private void OnAdd(ClickEvent evt)
	{
		if (zbSelf.Instance.friendHandler.friendsList.Count >= zbSelf.Instance.friendHandler.Max_Friends_Number)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10075"));
		}
		else if (m_roleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
		{
			if (!zbFriendHandler.AppliedPlayerIds.Contains((int)m_roleBaseInfo.RoleId))
			{
				zbFriendHandler.AppliedPlayerIds.Add((int)m_roleBaseInfo.RoleId);
			}
			zbWsMsgHandler.Instance.Friends_Apply_Req((int)m_roleBaseInfo.RoleId);
			zbSelf.Instance.friendHandler.ShowFriendAlreadyAppliedTip();
		}
	}

	private void OnShield()
	{
	}
}
