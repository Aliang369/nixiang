using System;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbSelfWnd : zbBaseWnd
{
	private Button m_Button_close;

	private VisualElement m_VisualElement_LegionIcon;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private VisualElement m_spriteProgress;

	private Button m_vipLv;

	private Label m_lbVip;

	private Label m_Label_Name;

	private Label m_Label_ID;

	private Label m_Label_Fight;

	private Label m_Label_DungeonProgress;

	private Label m_Label_DungeonProgress2;

	private Label m_Label_LegionName;

	private Label m_Label_LegionLevel;

	private Label m_Label_LegionID;

	private Label m_lbLeftTime;

	private Label m_lbUTC;

	private Label m_lbUTCTime;

	private Label m_lbVersion;

	private Label m_lbLevel;

	private Label m_lbServer;

	private Button m_Button_Head;

	private Button m_Button_Rename;

	private Button m_Button_Code;

	private Button m_btnSet;

	private Button m_btnCommunity;

	private Button m_Button_Logout;

	private Button m_btnRoleTip;

	private VisualElement m_joined;

	private Label m_notJoined;

	private VisualElement m_spriteRedDot;

	private Button m_btnCopy;

	private float maxWidth = 96f;

	private VisualElement m_notifyButtonRename;

	protected override void OnEnable()
	{
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_VisualElement_LegionIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "VisualElement_LegionIcon", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "head", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_vipLv = UQueryExtensions.Q<Button>(m_RootElement, "btn_viptq", (string)null);
		m_lbVip = UQueryExtensions.Q<Label>((VisualElement)(object)m_vipLv, "lb_vip", (string)null);
		m_Label_Name = UQueryExtensions.Q<Label>(m_RootElement, "Label_Name", (string)null);
		m_Label_ID = UQueryExtensions.Q<Label>(m_RootElement, "Label_ID", (string)null);
		m_Label_Fight = UQueryExtensions.Q<Label>(m_RootElement, "Label_Fight", (string)null);
		m_Label_DungeonProgress = UQueryExtensions.Q<Label>(m_RootElement, "Label_DungeonProgress", (string)null);
		m_Label_DungeonProgress2 = UQueryExtensions.Q<Label>(m_RootElement, "Label_DungeonProgress2", (string)null);
		m_Label_LegionName = UQueryExtensions.Q<Label>(m_RootElement, "Label_LegionName", (string)null);
		m_Label_LegionLevel = UQueryExtensions.Q<Label>(m_RootElement, "Label_LegionLevel", (string)null);
		m_Label_LegionID = UQueryExtensions.Q<Label>(m_RootElement, "Label_LegionID", (string)null);
		m_lbLeftTime = UQueryExtensions.Q<Label>(m_RootElement, "Label_ServerTime", (string)null);
		m_lbUTC = UQueryExtensions.Q<Label>(m_RootElement, "lb_utc", (string)null);
		m_lbUTCTime = UQueryExtensions.Q<Label>(m_RootElement, "Label_UTCTime", (string)null);
		m_lbVersion = UQueryExtensions.Q<Label>(m_RootElement, "lb_version", (string)null);
		m_Button_close = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_Button_Head = UQueryExtensions.Q<Button>(m_RootElement, "Button_Head", (string)null);
		m_Button_Rename = UQueryExtensions.Q<Button>(m_RootElement, "Button_Rename", (string)null);
		m_Button_Code = UQueryExtensions.Q<Button>(m_RootElement, "Button_Code", (string)null);
		m_Button_Logout = UQueryExtensions.Q<Button>(m_RootElement, "Button_Logout", (string)null);
		m_btnCommunity = UQueryExtensions.Q<Button>(m_RootElement, "btn_community", (string)null);
		m_btnSet = UQueryExtensions.Q<Button>(m_RootElement, "btn_set", (string)null);
		m_btnRoleTip = UQueryExtensions.Q<Button>(m_RootElement, "btn_roleTip", (string)null);
		m_spriteProgress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_joined = UQueryExtensions.Q<VisualElement>(m_RootElement, "joined", (string)null);
		m_notJoined = UQueryExtensions.Q<Label>(m_RootElement, "lb_notJoined", (string)null);
		m_spriteRedDot = UQueryExtensions.Q<VisualElement>(m_RootElement, "spriteRedDot", (string)null);
		m_lbServer = UQueryExtensions.Q<Label>(m_RootElement, "lb_server", (string)null);
		m_btnCopy = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_notifyButtonRename = zbUtil.CreateNotifyTag((VisualElement)(object)m_Button_Rename, (Vector2?)new Vector2(10f, 30f), 1f, true);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_close, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_Head, (Action<ClickEvent>)OnHead, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_Rename, (Action)OnName, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_Code, (Action<ClickEvent>)OnCode, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_Button_Logout, (Action<ClickEvent>)OnLogOut, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRoleTip, (Action<ClickEvent>)OnRoleTip, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopy, (Action<ClickEvent>)OnCopyRoleID, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCommunity, (Action<ClickEvent>)OnCommunity, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSet, (Action<ClickEvent>)OnSet, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_vipLv, (Action<ClickEvent>)OnVipTequan, "", (TrickleDown)0);
		((VisualElement)m_Button_Code).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(zbChannel_Login.IsOpenTSCheck() ? 1 : 0));
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleBaseInfo, OnDataChangeRoleBaseInfo);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_RoleCommonInfo, OnDataChangeRoleBaseInfo);
	}

	public override void Refresh()
	{
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		base.Refresh();
		if (m_Label_Name != null)
		{
			((TextElement)m_Label_Name).text = zbSelf.Instance.GetRoleName();
		}
		if (m_Label_ID != null)
		{
			((TextElement)m_Label_ID).text = "ID:" + zbSelf.Instance.GetRoleId();
		}
		if (m_Label_Fight != null)
		{
			((TextElement)m_Label_Fight).text = zbSelf.Instance.GetFightValue().ToString();
		}
		if (m_lbServer != null)
		{
			((TextElement)m_lbServer).text = "Server: " + zbUtil.SetServerString(zbSelf.Instance.GetServerId());
		}
		((TextElement)m_lbVip).text = "VIP" + zbSelf.Instance.GetVipLevel();
		m_joined.visible = zbSelf.Instance.legionHandler.legionInfo != null;
		((VisualElement)m_notJoined).visible = zbSelf.Instance.legionHandler.legionInfo == null;
		if (zbServerData.sGameData.MainQuestInfo.MainLevel > 0)
		{
			((TextElement)m_Label_DungeonProgress).text = zbServerData.sGameData.MainQuestInfo.MainLevel.ToString();
		}
		else
		{
			((TextElement)m_Label_DungeonProgress).text = "0";
		}
		if (zbServerData.sGameData.EliteQuestInfo.EliteLevel > 0)
		{
			((TextElement)m_Label_DungeonProgress2).text = zbServerData.sGameData.EliteQuestInfo.EliteLevel.ToString();
		}
		else
		{
			((TextElement)m_Label_DungeonProgress2).text = "0";
		}
		bool flag = zbSelf.Instance.legionHandler.legionInfo != null;
		m_joined.visible = flag;
		((VisualElement)m_notJoined).visible = !flag;
		if (flag)
		{
			m_VisualElement_LegionIcon.style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById(zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Icon));
		}
		if (zbServerData.sGameData.LegionSelfInfo != null)
		{
			((TextElement)m_Label_LegionID).text = zbServerData.sGameData.LegionSelfInfo.LegionId.ToString();
		}
		else
		{
			((TextElement)m_Label_LegionID).text = "";
		}
		if (zbSelf.Instance.legionHandler.legionInfo != null)
		{
			((TextElement)m_Label_LegionName).text = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.LegionName;
			((TextElement)m_Label_LegionLevel).text = string.Format(zbUtil.Language("lang.10060"), zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level);
		}
		else
		{
			((TextElement)m_Label_LegionName).text = zbUtil.Language("lang.10047");
			((TextElement)m_Label_LegionLevel).text = "";
		}
		((TextElement)m_lbVersion).text = "ver:" + zbVersionMgr.GetCurVersion().Replace("v", "") + "(" + zbSdkHelper.GetResVersion() + ")";
		UpdateExperienceDisplay();
		UpdateHeadAndHeadFrameDisplay();
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		m_spriteRedDot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Head)));
		m_notifyButtonRename.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbServerData.sGameData.RoleCommonInfo.RoleNameChange != 0) ? 1 : 0));
	}

	private void OnCopyRoleID(ClickEvent evt)
	{
		zbUtil.CopyString(zbSelf.Instance.GetRoleId().ToString());
	}

	private void OnRoleTip(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)zbSelf.Instance.GetRoleId());
	}

	private void OnCommunity(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHub(1);
	}

	private void OnSet(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowSet();
	}

	private void OnLogOut(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.Game_LoginOut_Req();
	}

	private void OnCode(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowCode();
	}

	private void OnName()
	{
		zbUIMgr.Instance.ShowResetName();
	}

	private void OnHead(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHead();
	}

	private void OnVipTequan(ClickEvent evt)
	{
		zbSelf.Instance.vipHandler.openVipTqWnd();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnDataChangeRoleBaseInfo(object data)
	{
		Refresh();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		if (m_lbLeftTime != null)
		{
			((TextElement)m_lbLeftTime).text = zbSelf.Instance.timeHandler.GetRefreshTimeStr();
		}
		if (m_lbUTCTime != null)
		{
			((TextElement)m_lbUTCTime).text = zbSelf.Instance.timeHandler.GetServerTimeStr();
		}
	}

	private void UpdateExperienceDisplay()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		int level = zbSelf.Instance.GetLevel();
		int roleExp = zbServerData.sGameData.RoleCommonInfo.RoleExp;
		int num = 0;
		float num2 = 1f;
		if (level >= 200)
		{
			((TextElement)m_lbLevel).text = "MAX " + level;
			m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(99.9f, (LengthUnit)0));
			return;
		}
		zbPlayerUpgradeCsvRecord recordByKey = zbDataConfig.Instance.playerUpgradeCsvTable.GetRecordByKey(level + 1);
		if (recordByKey != null)
		{
			num = recordByKey.exp;
			num2 = Mathf.Min((float)roleExp / (float)num, 1f);
		}
		else
		{
			num = 1;
			num2 = 1f;
		}
		((TextElement)m_lbLevel).text = level.ToString();
		m_spriteProgress.style.width = StyleLength.op_Implicit(new Length(num2 * maxWidth, (LengthUnit)0));
	}

	private void UpdateHeadAndHeadFrameDisplay()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbSelf.Instance.GetHeadId());
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbSelf.Instance.GetHeadFrameId());
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
	}

	protected override void RefreshWhenAutoVisibleCaseNoScene()
	{
		base.RefreshWhenAutoVisibleCaseNoScene();
		RefreshNotify();
	}
}
