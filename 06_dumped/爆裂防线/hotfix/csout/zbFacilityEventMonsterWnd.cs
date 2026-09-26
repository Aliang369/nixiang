using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbFacilityEventMonsterWnd : zbBaseWnd
{
	[NonSerialized]
	private zbPlayerAvatarTarget renderTarget;

	private VisualElement foldIcon;

	private Label m_lbPlayerName;

	private Button m_btnPlayer;

	private Image m_descImg;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private Button[] m_btnSkill = (Button[])(object)new Button[5];

	private List<SkillDetail> heroList = new List<SkillDetail>();

	private zbItem m_item;

	private VisualElement m_spriteCost;

	private Label m_lbCost;

	private Button m_btnFight;

	private Button m_btnCancel;

	private Button m_btnClose;

	private FacilityRoomDetail m_roomDetail;

	private RoleBaseInfo m_roleBaseInfo;

	private BattleInfo m_battleInfo;

	protected override void OnEnable()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		renderTarget = ((Component)GameObject.Find("Renders").transform.Find("playerAvatarTarget")).GetComponent<zbPlayerAvatarTarget>();
		if (m_descImg == null)
		{
			m_descImg = new Image();
			((VisualElement)m_descImg).style.left = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.top = StyleLength.op_Implicit(0f);
			((VisualElement)m_descImg).style.width = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.height = StyleLength.op_Implicit(240f);
			((VisualElement)m_descImg).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		}
		foldIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "foldIcon", (string)null);
		foldIcon.Add((VisualElement)(object)m_descImg);
		m_lbPlayerName = UQueryExtensions.Q<Label>(m_RootElement, "lb_playername", (string)null);
		m_btnPlayer = UQueryExtensions.Q<Button>(m_RootElement, "btn_player", (string)null);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(m_RootElement, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
			m_btnSkill[i] = UQueryExtensions.Q<Button>((VisualElement)(object)tb_skillMod[i], "btn_skill", (string)null);
			int curIndex = i + 1;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSkill[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnSkillClick(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_spriteCost = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg_cost", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnFight = UQueryExtensions.Q<Button>(m_RootElement, "btn_fight", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnPlayer, (Action<ClickEvent>)OnPlayer, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFight, (Action)OnFight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_PvpTipsInfo, delegate
		{
			RefreshPvpTipsInfo();
		});
		AddDark();
	}

	public void RefreshCustom(FacilityRoomDetail roomDetail, RoleBaseInfo roleBaseInfo, BattleInfo battleInfo)
	{
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		m_roomDetail = roomDetail;
		m_roleBaseInfo = roleBaseInfo;
		m_battleInfo = battleInfo;
		renderTarget.StartRender(m_descImg);
		renderTarget.RefreshAvatar(roleBaseInfo.AvatarId, roleBaseInfo.GunId);
		((TextElement)m_lbPlayerName).text = roleBaseInfo.RoleName;
		m_item.Refresh(new zbItemData(roomDetail.RoomAward.Type, roomDetail.RoomAward.Tid, roomDetail.RoomAward.Count));
		if (m_roomDetail.RoomState == EFacilityRoomState.Ready)
		{
			m_spriteCost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbCost).text = zbSelf.Instance.facilityHandler.tiliCost.ToString();
		}
		else
		{
			m_spriteCost.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.guardHandler.pvpTipsInfo = null;
		RefreshPvpTipsInfo();
		zbWsMsgHandler.Instance.Role_PvpTipsInfo_Req((int)roleBaseInfo.RoleId);
	}

	private void OnPlayer(ClickEvent evt)
	{
		if (m_roleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
		{
			zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_roleBaseInfo.RoleId);
		}
	}

	private void OnFight()
	{
		if (m_battleInfo != null)
		{
			zbSelf.Instance.facilityHandler.curRoomId = m_roomDetail.RoomId;
			zbSelf.Instance.combatHandler.StartShowCombat(m_battleInfo);
			OnClose();
		}
		else if (m_roomDetail.RoomState == EFacilityRoomState.Ready && !zbSelf.Instance.moneyHandler.HasEnoughMoney(4, zbSelf.Instance.facilityHandler.tiliCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, zbSelf.Instance.facilityHandler.tiliCost);
		}
		else
		{
			zbSelf.Instance.facilityHandler.curRoomId = m_roomDetail.RoomId;
			zbWsMsgHandler.Instance.Facility_EventTrigger_Req(zbSelf.Instance.facilityHandler.facilityMapInfo.BaseInfo.FacilityId, m_roomDetail.RoomId, needExploreAnim: false, openCombatWnd: true);
			OnClose();
		}
	}

	private void OnClose()
	{
		Hide();
	}

	public void RefreshPvpTipsInfo()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < 5; i++)
		{
			m_nodeDetail[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		heroList.Clear();
		HashSet<int> hashSet = new HashSet<int>();
		if (zbSelf.Instance.guardHandler.pvpTipsInfo == null || zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
		{
			return;
		}
		List<zbModCsvRecord> source = (from r in (IEnumerable<int>)zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList
			select zbDataConfig.Instance.modCsvTable.GetRecordByKey(r) into m
			where m != null
			select m).ToList();
		foreach (int mod in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			foreach (SkillDetail skill in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)mod && hashSet.Add((int)skill.SkillType))
				{
					SkillDetail skillDetail = new SkillDetail();
					skillDetail.HeroTid = skill.HeroTid;
					skillDetail.SkillType = skill.SkillType;
					skillDetail.SkillId = skill.SkillId;
					heroList.Add(skillDetail);
				}
			}
		}
		foreach (int select in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			foreach (SkillDetail skill2 in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill2.SkillType + 20010 == (ESkillType)select && hashSet.Add((int)skill2.SkillType))
				{
					SkillDetail skillDetail2 = new SkillDetail();
					skillDetail2.HeroTid = skill2.HeroTid;
					skillDetail2.SkillType = skill2.SkillType;
					skillDetail2.SkillId = skill2.SkillId;
					heroList.Add(skillDetail2);
				}
			}
		}
		int num = 0;
		foreach (SkillDetail hero in heroList)
		{
			if (num >= 5)
			{
				break;
			}
			zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(hero.SkillId);
			if (recordByKey != null)
			{
				((VisualElement)tb_skillMod[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
				m_skillIcon[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				m_skillIcon[num].transform.scale = Vector3.one * 0.45f;
				ESkillType type = hero.SkillType;
				int num2 = source.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
				((TextElement)m_lbSkillLevel[num]).text = num2.ToString();
				num++;
			}
		}
	}

	public void OnSkillClick(ClickEvent evt, int skillIndex)
	{
		if (zbSelf.Instance.guardHandler.pvpTipsInfo == null || zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int mod in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			list.Add(mod);
		}
		foreach (int select in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			list.Add(select);
		}
		zbUIMgr.Instance.ShowSkillFightModWnd(0, heroList.ToArray(), list, isResumeFight: false);
	}
}
