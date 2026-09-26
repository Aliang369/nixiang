using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPlayerInfoWnd : zbBaseWnd
{
	private zbPlayerAvatar m_spineAvatar;

	private zbPlayerInfoItem[] m_equipSlots = new zbPlayerInfoItem[3];

	private Button m_close;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private Label m_lbName;

	private Label m_lbLevel;

	private Label m_lbId;

	private Label m_lbServer;

	private Button m_btnCopy;

	private Label m_Label_Fight;

	private Label m_Label_DungeonProgress;

	private VisualElement m_VisualElement_Head;

	private VisualElement m_VisualElement_LegionIcon;

	private Label m_Label_LegionName;

	private Label m_lbVip;

	private Button m_vipLv;

	private Button m_btnWall;

	private Button m_btnMecha;

	private Button m_btnFootHold;

	private VisualElement m_leftIcons;

	private Button m_btnAdd;

	private Button m_btnShowLegion;

	private Button m_btnCheckLegion;

	private Button m_btnQiechuo;

	private RoleTipsInfo m_roleTipsInfo;

	private Button m_btnGun;

	private Label m_lbGunDesc;

	private Button[] m_suits = (Button[])(object)new Button[3];

	private Button[] m_highSuits = (Button[])(object)new Button[2];

	private Label[] m_lbSuitsDesc = (Label[])(object)new Label[3];

	private Label[] m_lbHighSuitsDesc = (Label[])(object)new Label[2];

	private VisualElement m_btnMask;

	protected override void Awake()
	{
		base.Awake();
		for (int i = 0; i < 3; i++)
		{
			m_equipSlots[i] = new zbPlayerInfoItem();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_spineAvatar = ((Component)m_GameObjectSceneRoot.transform.Find("playerAvatar")).GetComponent<zbPlayerAvatar>();
		for (int i = 0; i < 3; i++)
		{
			VisualElement root = UQueryExtensions.Q<VisualElement>(m_RootElement, "equipSlot" + i, (string)null);
			m_equipSlots[i].OnEnable(this, root);
			m_suits[i] = UQueryExtensions.Q<Button>(m_RootElement, "btn_suit" + i, (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_suits[i], (Action)delegate
			{
				OnMask(tempIndex + 1);
			}, "", (TrickleDown)0);
			m_lbSuitsDesc[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_descSuit" + i, (string)null);
		}
		for (int num = 0; num < 2; num++)
		{
			m_highSuits[num] = UQueryExtensions.Q<Button>(m_RootElement, "btn_highSuit" + num, (string)null);
			m_lbHighSuitsDesc[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_descHighSuit" + num, (string)null);
			int tempIndex2 = num;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_highSuits[num], (Action)delegate
			{
				OnMask(tempIndex2 + 1 + 3);
			}, "", (TrickleDown)0);
		}
		m_close = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "head", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "headFrame", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbId = UQueryExtensions.Q<Label>(m_RootElement, "lb_id", (string)null);
		m_lbServer = UQueryExtensions.Q<Label>(m_RootElement, "lb_server", (string)null);
		m_btnCopy = UQueryExtensions.Q<Button>(m_RootElement, "btn_copy", (string)null);
		m_leftIcons = UQueryExtensions.Q<VisualElement>(m_RootElement, "leftIcons", (string)null);
		m_Label_Fight = UQueryExtensions.Q<Label>(m_RootElement, "Label_Fight", (string)null);
		m_Label_DungeonProgress = UQueryExtensions.Q<Label>(m_RootElement, "Label_DungeonProgress", (string)null);
		m_vipLv = UQueryExtensions.Q<Button>(m_RootElement, "btn_viptq", (string)null);
		m_lbVip = UQueryExtensions.Q<Label>((VisualElement)(object)m_vipLv, "lb_vip", (string)null);
		m_VisualElement_Head = UQueryExtensions.Q<VisualElement>(m_RootElement, "VisualElement_Head", (string)null);
		m_VisualElement_LegionIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "VisualElement_LegionIcon", (string)null);
		m_Label_LegionName = UQueryExtensions.Q<Label>(m_RootElement, "Label_LegionName", (string)null);
		m_btnGun = UQueryExtensions.Q<Button>(m_RootElement, "btn_gun", (string)null);
		m_lbGunDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_gunDesc", (string)null);
		m_btnMask = UQueryExtensions.Q<VisualElement>(m_RootElement, "btn_mask", (string)null);
		m_btnWall = UQueryExtensions.Q<Button>(m_RootElement, "btn_wall", (string)null);
		m_btnMecha = UQueryExtensions.Q<Button>(m_RootElement, "btn_mecha", (string)null);
		m_btnFootHold = UQueryExtensions.Q<Button>(m_RootElement, "btn_footHold", (string)null);
		m_btnCheckLegion = UQueryExtensions.Q<Button>(m_RootElement, "btn_checkLegion", (string)null);
		m_btnQiechuo = UQueryExtensions.Q<Button>(m_RootElement, "btn_qiechuo", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnShowLegion = UQueryExtensions.Q<Button>(m_RootElement, "btn_showLegion", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_close, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnShowLegion, (Action)OnDetail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAddOrDel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCopy, (Action<ClickEvent>)OnCopyRoleID, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnGun, (Action)delegate
		{
			OnMask();
		}, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_btnMask, (Action)OnCloseMask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnWall, (Action)OnDetail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMecha, (Action)OnDetail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFootHold, (Action)OnDetail, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCheckLegion, (Action)OnCheckLegion, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnQiechuo, (Action)OnQiechuo, "", (TrickleDown)0);
	}

	public void RefreshCustom(RoleTipsInfo _roleTipsInfo)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (_roleTipsInfo != null)
		{
			m_roleTipsInfo = _roleTipsInfo;
			bool flag = m_roleTipsInfo.RoleBaseInfo.GunId > 2 && m_roleTipsInfo.RoleSuitInfo.SuitList.Count == 3 && ((IEnumerable<PlayerSuitDetail>)m_roleTipsInfo.RoleSuitInfo.SuitList).All((PlayerSuitDetail u) => u.SuitId > 1) && m_roleTipsInfo.RoleSuitInfo.SuperSuitList.Count == 2 && ((IEnumerable<PlayerSuitDetail>)m_roleTipsInfo.RoleSuitInfo.SuperSuitList).All((PlayerSuitDetail s) => s.SuitId > 1);
			m_leftIcons.style.justifyContent = StyleEnum<Justify>.op_Implicit((Justify)(flag ? 2 : 0));
			RefreshView();
			UpdateHeadAndHeadFrameDisplay();
			InitGunAndSuit();
			InitBottomInfo();
			OnCloseMask();
		}
	}

	private void InitBottomInfo()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		if (m_roleTipsInfo != null && m_roleTipsInfo.RoleWallInfo != null)
		{
			zbWallCsvRecord recordByKey = zbDataConfig.Instance.wallCsvTable.GetRecordByKey((m_roleTipsInfo.RoleWallInfo.WallId == 0) ? 1 : m_roleTipsInfo.RoleWallInfo.WallId);
			((VisualElement)m_btnWall).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.banner));
			zbFootholdCsvRecord recordByKey2 = zbDataConfig.Instance.footholdCsvTable.GetRecordByKey((m_roleTipsInfo.RoleBaseInfo.FootholdId == 0) ? 1 : m_roleTipsInfo.RoleBaseInfo.FootholdId);
			((VisualElement)m_btnFootHold).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.banner));
			zbMechaCsvRecord recordByKey3 = zbDataConfig.Instance.mechaCsvTable.GetRecordByKey(m_roleTipsInfo.RoleBaseInfo.MechaId);
			bool flag = recordByKey3 != null;
			((VisualElement)m_btnMecha).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(flag ? recordByKey3.banner : "ui/image/mecha/mecha_no.jpg"));
		}
	}

	private void InitGunAndSuit()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerGunCsvRecord recordByKey = zbDataConfig.Instance.playerGunCsvTable.GetRecordByKey(m_roleTipsInfo.RoleBaseInfo.GunId);
		((VisualElement)m_btnGun).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_roleTipsInfo.RoleBaseInfo.GunId <= 2));
		if (recordByKey != null)
		{
			Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.skillIcon);
			((VisualElement)m_btnGun).style.backgroundImage = new StyleBackground(val);
			((TextElement)m_lbGunDesc).text = zbUtil.Language(recordByKey.desc);
		}
		RepeatedField<PlayerSuitDetail> suitList = m_roleTipsInfo.RoleSuitInfo.SuitList;
		RepeatedField<PlayerSuitDetail> superSuitList = m_roleTipsInfo.RoleSuitInfo.SuperSuitList;
		for (int i = 0; i < 3; i++)
		{
			if (suitList == null || i >= suitList.Count || suitList[i].SuitId <= 1)
			{
				((VisualElement)m_suits[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				continue;
			}
			zbPlayerSuitCsvRecord recordByKey2 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(suitList[i].SuitId);
			bool flag = recordByKey2 != null && recordByKey2.grade < 2;
			((VisualElement)m_suits[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (!flag)
			{
				continue;
			}
			Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.skillIcon);
			((VisualElement)m_suits[i]).style.backgroundImage = new StyleBackground(val2);
			int suitId = suitList[i].SuitId;
			int num = 0;
			foreach (PlayerSuitDetail item in suitList)
			{
				if (item.SuitId == suitId)
				{
					num = item.Star;
					break;
				}
			}
			if (num > 0)
			{
				((TextElement)m_lbSuitsDesc[i]).text = zbUtil.Language(zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId, num));
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (j >= superSuitList.Count)
			{
				((VisualElement)m_highSuits[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				continue;
			}
			zbPlayerSuitCsvRecord recordByKey3 = zbDataConfig.Instance.playerSuitCsvTable.GetRecordByKey(superSuitList[j].SuitId);
			if (recordByKey3 == null)
			{
				continue;
			}
			bool flag2 = j < superSuitList.Count && superSuitList[j].SuitId > 1 && recordByKey3.grade >= 2;
			((VisualElement)m_highSuits[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (!flag2)
			{
				continue;
			}
			Sprite val3 = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey3.skillIcon);
			((VisualElement)m_highSuits[j]).style.backgroundImage = new StyleBackground(val3);
			int suitId2 = superSuitList[j].SuitId;
			int num2 = 0;
			foreach (PlayerSuitDetail item2 in superSuitList)
			{
				if (item2.SuitId == suitId2)
				{
					num2 = item2.Star;
				}
			}
			if (suitId2 > 0 && num2 > 0)
			{
				((TextElement)m_lbHighSuitsDesc[j]).text = zbUtil.Language(zbSelf.Instance.suitGunHandler.GetSkillDesc(suitId2, num2));
			}
		}
	}

	public bool IsSelf()
	{
		if (m_roleTipsInfo != null && m_roleTipsInfo.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId())
		{
			return true;
		}
		return false;
	}

	private void OnMask(int index = 0)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		m_btnMask.SetEnabled(true);
		m_btnMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		m_btnMask.pickingMode = (PickingMode)0;
		((VisualElement)m_lbGunDesc).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((index != 0) ? 1 : 0));
		((VisualElement)m_lbSuitsDesc[0]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 1));
		((VisualElement)m_lbSuitsDesc[1]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 2));
		((VisualElement)m_lbSuitsDesc[2]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 3));
		((VisualElement)m_lbHighSuitsDesc[0]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 4));
		((VisualElement)m_lbHighSuitsDesc[1]).parent.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(index != 5));
	}

	private void OnCloseMask()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		m_btnMask.SetEnabled(false);
		m_btnMask.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_btnMask.pickingMode = (PickingMode)1;
	}

	private void RefreshView()
	{
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		if (m_roleTipsInfo != null)
		{
			((Component)m_spineAvatar).gameObject.SetActive(m_roleTipsInfo != null);
			m_spineAvatar.RefreshCustom(m_roleTipsInfo.RoleBaseInfo.AvatarId, m_roleTipsInfo.RoleBaseInfo.GunId);
			zbSelf.Instance.suitGunHandler.SetSuperSuitEnterAnimation(m_spineAvatar);
			((TextElement)m_lbVip).text = "VIP" + m_roleTipsInfo.RoleBaseInfo.VipLevel;
		}
		for (int i = 0; i < 3; i++)
		{
			EquipSlotInfo equipSlotInfo = new EquipSlotInfo();
			equipSlotInfo.EquipSlotList.Add(m_roleTipsInfo.EquipSlotInfo.EquipSlotList[i]);
			GemPlanDetail gemPlanDetail = new GemPlanDetail();
			gemPlanDetail.GemPlanSlotList.AddRange((IEnumerable<GemPlanSlotDetail>)m_roleTipsInfo.EquipSlotInfo.GemSlotList);
			equipSlotInfo.GemPlanList.Add(gemPlanDetail);
			m_equipSlots[i].Refresh(equipSlotInfo, (EEquipType)(i + 1));
		}
		((TextElement)m_lbName).text = m_roleTipsInfo.RoleBaseInfo.RoleName;
		((TextElement)m_lbLevel).text = m_roleTipsInfo.RoleBaseInfo.Level.ToString();
		((TextElement)m_lbId).text = m_roleTipsInfo.RoleBaseInfo.RoleId.ToString();
		((TextElement)m_Label_Fight).text = m_roleTipsInfo.RoleBaseInfo.TeamAttack.ToString();
		if (m_roleTipsInfo.RoleAuxInfo.MainLevel > 0)
		{
			((TextElement)m_Label_DungeonProgress).text = zbResourceHelper.GetMainLevelName(m_roleTipsInfo.RoleAuxInfo.MainLevel);
		}
		else
		{
			((TextElement)m_Label_DungeonProgress).text = "0";
		}
		((TextElement)m_lbServer).text = "Server: " + zbUtil.SetServerString(m_roleTipsInfo.RoleBaseInfo.ServerId);
		bool flag = m_roleTipsInfo.LegionFlagInfo != null && m_roleTipsInfo.LegionFlagInfo.LegionId > 0;
		((VisualElement)m_Label_LegionName).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		((VisualElement)m_btnCheckLegion).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
		if (flag)
		{
			((TextElement)m_Label_LegionName).text = string.Format(zbUtil.Language("lang.11337"), m_roleTipsInfo.LegionFlagInfo.LegionName);
		}
		((TextElement)m_btnAdd).text = (zbSelf.Instance.friendHandler.IsMyFriend((int)m_roleTipsInfo.RoleBaseInfo.RoleId) ? zbUtil.Language("lang.10373") : zbUtil.Language("lang.10374"));
		((VisualElement)m_btnAdd).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_roleTipsInfo.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId()) ? 1 : 0));
		int num = 999;
		zbFunctionUnlockCsvRecord recordByKey = zbDataConfig.Instance.functionUnlockCsvTable.GetRecordByKey(45);
		if (recordByKey != null)
		{
			num = recordByKey.unlockVal;
		}
		((VisualElement)m_btnQiechuo).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena) || m_roleTipsInfo.RoleBaseInfo.RoleId == zbSelf.Instance.GetRoleId() || num >= m_roleTipsInfo.RoleAuxInfo.PeerageId) ? 1 : 0));
	}

	private void OnCheckLegion()
	{
		if (m_roleTipsInfo.LegionFlagInfo != null && m_roleTipsInfo.LegionFlagInfo.LegionId > 0)
		{
			zbWsMsgHandler.Instance.Legion_TipsInfo_Req(m_roleTipsInfo.LegionFlagInfo.LegionId);
		}
	}

	private void OnQiechuo()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena) && m_roleTipsInfo.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
		{
			zbWsMsgHandler.Instance.Role_FightStart_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
		}
	}

	private void UpdateHeadAndHeadFrameDisplay()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(m_roleTipsInfo.RoleBaseInfo.HeadId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(m_roleTipsInfo.RoleBaseInfo.HeadFrameId);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
	}

	private void OnCopyRoleID(ClickEvent evt)
	{
		zbUtil.CopyString(m_roleTipsInfo.RoleBaseInfo.RoleId.ToString());
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnAddOrDel()
	{
		if (zbSelf.Instance.friendHandler.IsMyFriend((int)m_roleTipsInfo.RoleBaseInfo.RoleId))
		{
			zbWsMsgHandler.Instance.Friends_Delete_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
			OnClose();
		}
		else if (m_roleTipsInfo.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
		{
			zbWsMsgHandler.Instance.Friends_Apply_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
			zbSelf.Instance.friendHandler.ShowFriendAlreadyAppliedTip();
		}
	}

	private void OnDetail()
	{
		zbWsMsgHandler.Instance.Role_TipsDetail_Req((int)m_roleTipsInfo.RoleBaseInfo.RoleId);
	}
}
