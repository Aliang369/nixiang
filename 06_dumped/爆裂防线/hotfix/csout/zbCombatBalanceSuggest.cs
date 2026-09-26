using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbCombatBalanceSuggest
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_root;

	private VisualElement m_playerInfo;

	private VisualElement[] m_skills = (VisualElement[])(object)new VisualElement[5];

	private Button[] m_skillButtons = (Button[])(object)new Button[5];

	private Button m_btnHead;

	private Button m_btnDetail;

	private PlayerSuggestDetail m_detail;

	private SkillDetail[] m_heroList;

	private List<int> m_modList;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_root = root;
		m_playerInfo = UQueryExtensions.Q<VisualElement>(m_root, "playerInfo", (string)null);
		m_btnHead = UQueryExtensions.Q<Button>(m_root, "btn_head", (string)null);
		m_btnDetail = UQueryExtensions.Q<Button>(m_root, "btn_detail", (string)null);
		for (int i = 0; i < 5; i++)
		{
			int tempIndex = i;
			m_skills[i] = UQueryExtensions.Q<VisualElement>(m_root, "skill" + i, (string)null);
			m_skillButtons[i] = UQueryExtensions.Q<Button>(m_skills[i], "btn_touch", (string)null);
			wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_skillButtons[i], (Action<ClickEvent>)delegate
			{
				OnSkillButton(tempIndex);
			}, "", (TrickleDown)0);
		}
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnHead, (Action)OnHead, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetail, (Action)OnDetail, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		m_detail = ((IEnumerable<PlayerSuggestDetail>)zbSelf.Instance.playerSuggestHandler.playerSuggestAck.SuggestList).ToArray()[0];
		InitPlayerInfo();
		InitSkills();
	}

	private void InitPlayerInfo()
	{
		zbUtil.InitPlayerInfo(UQueryExtensions.Q<VisualElement>(m_playerInfo, "header", (string)null), frame: UQueryExtensions.Q<VisualElement>(m_playerInfo, "frame", (string)null), lb_level: UQueryExtensions.Q<Label>(m_playerInfo, "lb_level", (string)null), HeadId: m_detail.TipsInfo.RoleBaseInfo.HeadId, HeadFrameId: m_detail.TipsInfo.RoleBaseInfo.HeadFrameId, Level: m_detail.TipsInfo.RoleBaseInfo.Level);
	}

	private int GetModCountBySkillType(ESkillType type)
	{
		int num = 0;
		foreach (int mod in m_modList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod);
			if (recordByKey.skillLvUp == 1 && Enumerable.Contains(recordByKey.skillType, (int)type))
			{
				num++;
			}
		}
		return num;
	}

	private void InitSkills()
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		List<BattleResultHeroDetail> list = ((IEnumerable<BattleResultHeroDetail>)m_detail.BattleResult.AttackerTeamInfo.MasterTeamInfo.HeroList).Where((BattleResultHeroDetail h) => h.HeroTid != 0).ToList();
		RepeatedField<BattleResultModDetail> modList = m_detail.BattleResult.AttackerTeamInfo.MasterTeamInfo.ModList;
		m_heroList = list.ToList().Select(delegate(BattleResultHeroDetail s)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(s.HeroTid);
			return new SkillDetail
			{
				HeroTid = s.HeroTid,
				SkillType = (ESkillType)recordByKey.type,
				SkillId = recordByKey.skillId
			};
		}).ToArray();
		m_modList = ((IEnumerable<BattleResultModDetail>)modList).Select((BattleResultModDetail r) => r.ModId).ToList();
		for (int num = 0; num < 5; num++)
		{
			bool flag = num < list.Count;
			m_skills[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				BattleResultHeroDetail battleResultHeroDetail = list[num];
				UQueryExtensions.Q<VisualElement>(m_skills[num], "icon_mod", (string)null).style.backgroundImage = new StyleBackground(zbSelf.Instance.heroHandler.GetSkillIconByHeroTid(battleResultHeroDetail.HeroTid));
				((TextElement)UQueryExtensions.Q<Label>(m_skills[num], "lb_modLevel", (string)null)).text = "Lv." + GetModCountBySkillType((ESkillType)zbSelf.Instance.heroHandler.GetHeroRecord(battleResultHeroDetail.HeroTid).type);
			}
		}
	}

	private void OnSkillButton(int index)
	{
		zbUIMgr.Instance.ShowSkillFightModWnd(index, m_heroList, m_modList, isResumeFight: false);
	}

	private void OnHead()
	{
		zbWsMsgHandler.Instance.Role_TipsInfo_Req((int)m_detail.TipsInfo.RoleBaseInfo.RoleId);
	}

	public void SetDisplay(bool show)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!show));
	}

	private List<ModSkillInfo> GetAllModBySkillType()
	{
		List<ModSkillInfo> list = new List<ModSkillInfo>();
		foreach (int mod in m_modList)
		{
			int num = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod).skillType[0];
			int heroId = 0;
			int skillId = 0;
			SkillDetail[] heroList = m_heroList;
			foreach (SkillDetail skillDetail in heroList)
			{
				if (num == (int)skillDetail.SkillType)
				{
					heroId = skillDetail.HeroTid;
					skillId = skillDetail.SkillId;
				}
			}
			list.Add(new ModSkillInfo
			{
				ModId = mod,
				SkillType = (ESkillType)num,
				HeroId = heroId,
				SkillId = skillId
			});
		}
		return list;
	}

	private void OnDetail()
	{
		zbWsMsgHandler.Instance.Aux_PlayerSuggest_Req(EBattleFuncType.MainQuest, zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId);
	}
}
