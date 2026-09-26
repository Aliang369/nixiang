using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSkillSelected
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbPower;

	private VisualElement[] m_nodeDetail = (VisualElement[])(object)new VisualElement[5];

	private TemplateContainer[] tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private zbPieProgress[] m_skillOutProgress = new zbPieProgress[5];

	private zbPieProgress[] m_skillInProgress = new zbPieProgress[5];

	private VisualElement[] m_skillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillLevel = (Label[])(object)new Label[5];

	private List<SkillDetail> heroList = new List<SkillDetail>();

	private Button m_btnModList;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(root, "headFrame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(root, "head", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(root, "lb_name", (string)null);
		m_lbPower = UQueryExtensions.Q<Label>(root, "lb_power", (string)null);
		for (int i = 0; i < 5; i++)
		{
			tb_skillMod[i] = UQueryExtensions.Q<TemplateContainer>(root, "skill" + (i + 1), (string)null);
			m_nodeDetail[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "nodeDetail", (string)null);
			m_skillOutProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "outProgress", (string)null);
			m_skillOutProgress[i].progress = 100f;
			m_skillInProgress[i] = UQueryExtensions.Q<zbPieProgress>((VisualElement)(object)tb_skillMod[i], "inProgress", (string)null);
			m_skillInProgress[i].progress = 100f;
			m_skillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tb_skillMod[i], "skillIcon", (string)null);
			m_lbSkillLevel[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tb_skillMod[i], "lb_skill", (string)null);
		}
		m_btnModList = UQueryExtensions.Q<Button>(root, "btn_modList", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnModList, (Action)OnModList, "", (TrickleDown)0);
		wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSubmit, OnEventUpdate);
	}

	public void Refresh(RoleBaseInfo roleBaseInfo)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (roleBaseInfo != null)
		{
			string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(roleBaseInfo.HeadFrameId);
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
			string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(roleBaseInfo.HeadId);
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
			((TextElement)m_lbName).text = roleBaseInfo.RoleName;
			((TextElement)m_lbPower).text = zbUtil.ConvertToTBMK(roleBaseInfo.TeamAttack);
		}
	}

	public void refreshSkill()
	{
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		if (zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
		{
			return;
		}
		heroList.Clear();
		HashSet<int> hashSet = new HashSet<int>();
		List<zbModCsvRecord> list = new List<zbModCsvRecord>();
		foreach (int mod in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod);
			list.Add(recordByKey);
		}
		foreach (int select in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			zbModCsvRecord recordByKey2 = zbDataConfig.Instance.modCsvTable.GetRecordByKey(select);
			list.Add(recordByKey2);
		}
		foreach (int mod2 in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.ModList)
		{
			foreach (SkillDetail skill in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill.SkillType + 20010 == (ESkillType)mod2 && hashSet.Add((int)skill.SkillType))
				{
					SkillDetail skillDetail = new SkillDetail();
					skillDetail.HeroTid = skill.HeroTid;
					skillDetail.SkillType = skill.SkillType;
					skillDetail.SkillId = skill.SkillId;
					heroList.Add(skillDetail);
				}
			}
		}
		foreach (int select2 in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SelectList)
		{
			foreach (SkillDetail skill2 in zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo.SkillList)
			{
				if (skill2.SkillType + 20010 == (ESkillType)select2 && hashSet.Add((int)skill2.SkillType))
				{
					SkillDetail skillDetail2 = new SkillDetail();
					skillDetail2.HeroTid = skill2.HeroTid;
					skillDetail2.SkillType = skill2.SkillType;
					skillDetail2.SkillId = skill2.SkillId;
					heroList.Add(skillDetail2);
				}
			}
		}
		for (int i = 0; i < 5; i++)
		{
			m_nodeDetail[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (SkillDetail hero in heroList)
		{
			if (num >= 5)
			{
				break;
			}
			zbSkillCsvRecord recordByKey3 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(hero.SkillId);
			if (recordByKey3 != null)
			{
				((VisualElement)tb_skillMod[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_nodeDetail[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey3.icon);
				m_skillIcon[num].style.backgroundImage = StyleBackground.op_Implicit(val);
				m_skillIcon[num].transform.scale = Vector3.one * 0.45f;
				ESkillType type = hero.SkillType;
				int num2 = list.Count((zbModCsvRecord mod) => mod.skillLvUp == 1 && Enumerable.Contains(mod.skillType, (int)type));
				((TextElement)m_lbSkillLevel[num]).text = num2.ToString();
				num++;
			}
		}
	}

	private void OnModList()
	{
		if (zbSelf.Instance.guardHandler.pvpTipsInfo.PvpModInfo == null)
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

	private void OnEventUpdate(object data)
	{
		refreshSkill();
	}
}
