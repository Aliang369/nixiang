using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFriendTiliItem : zbViewBaseItem
{
	private FriendsDetail m_friendsDetail;

	private readonly Label m_lbLevel;

	private readonly Label m_lbName;

	private readonly zbFriendTiLiWnd m_Parent;

	private readonly VisualElement m_headFrame;

	private readonly VisualElement m_head;

	private readonly VisualElement m_btnNoode0;

	private readonly Button m_btnNoode1;

	private readonly VisualElement m_btnNoode2;

	private readonly Label m_lbFv;

	private readonly Label m_lbTime;

	private readonly Button m_btnSend;

	private readonly Button m_btnGet;

	private readonly Button m_btnMessage;

	private readonly Button m_btnTouch;

	private readonly Label m_lbApplyState;

	public zbFriendTiliItem(zbFriendTiLiWnd parent, VisualTreeAsset tAsset)
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
		m_lbApplyState = UQueryExtensions.Q<Label>((VisualElement)(object)this, "lb_applyState", (string)null);
		m_btnNoode2 = UQueryExtensions.Q<VisualElement>((VisualElement)(object)this, "btn_noode2", (string)null);
		m_btnGet = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_get", (string)null);
		m_btnSend = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_send", (string)null);
		m_btnMessage = UQueryExtensions.Q<Button>(m_btnNoode0, "btn_message", (string)null);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnTouch, "", (TrickleDown)0);
		m_Parent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGet, (Action<ClickEvent>)OnGet, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)this);
	}

	public void Init(FriendsDetail info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		m_friendsDetail = info;
		((VisualElement)m_btnSend).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnMessage).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_btnNoode1).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbApplyState).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnNoode2.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		refreshItems();
	}

	private void refreshItems()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_friendsDetail.RoleBaseInfo.HeadId);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_friendsDetail.RoleBaseInfo.HeadFrameId);
		m_head.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		m_headFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		((TextElement)m_lbLevel).text = m_friendsDetail.RoleBaseInfo.Level.ToString();
		((TextElement)m_lbName).text = m_friendsDetail.RoleBaseInfo.RoleName;
		((TextElement)m_lbFv).text = m_friendsDetail.RoleBaseInfo.TeamAttack.ToString();
		if (m_friendsDetail != null)
		{
			RefreshOnlineState(m_friendsDetail.RoleLoginInfo, m_friendsDetail.RoleBaseInfo.RoleId);
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

	private void OnTouch(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_friendsDetail.RoleBaseInfo.RoleId);
	}

	private void OnGet(ClickEvent evt)
	{
		if (zbServerData.sGameData.FriendsInfo.CollectCount < zbSelf.Instance.friendHandler.Max_Friends_Collect_Number)
		{
			zbWsMsgHandler.Instance.Friends_ActionCollect_Req((int)m_friendsDetail.RoleBaseInfo.RoleId);
			m_Parent.m_pendingFriendList.Remove(m_friendsDetail);
			((BaseVerticalCollectionView)m_Parent.m_listView).RefreshItems();
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10438"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10678"), zbDataConfig.Instance.moneyCsvTable.GetRecordByKey(4).name));
		}
	}
}
