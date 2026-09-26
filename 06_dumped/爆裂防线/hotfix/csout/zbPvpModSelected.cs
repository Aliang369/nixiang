using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpModSelected
{
	private zbBaseWnd m_wndParent;

	private VisualElement[] m_nodeDetail;

	private TemplateContainer[] tb_skillMod;

	private zbPieProgress[] m_skillOutProgress;

	private zbPieProgress[] m_skillInProgress;

	private VisualElement[] m_skillIcon;

	private Label[] m_lbSkillLevel;

	private List<SkillDetail> heroList;

	private Button m_btnModList;

	private PvpModInfo pvpModInfo;

	public zbPvpModSelected(VisualElement root, zbBaseWnd wndParent)
	{
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		m_nodeDetail = (VisualElement[])(object)new VisualElement[5];
		tb_skillMod = (TemplateContainer[])(object)new TemplateContainer[5];
		m_skillOutProgress = new zbPieProgress[5];
		m_skillInProgress = new zbPieProgress[5];
		m_skillIcon = (VisualElement[])(object)new VisualElement[5];
		m_lbSkillLevel = (Label[])(object)new Label[5];
		heroList = new List<SkillDetail>();
		base._002Ector();
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
			((VisualElement)m_lbSkillLevel[i]).style.fontSize = StyleLength.op_Implicit(14f);
		}
		m_btnModList = UQueryExtensions.Q<Button>(root, "btn_modList", (string)null);
		wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnModList, (Action)OnModList, "", (TrickleDown)0);
		zbFontReplacer.Instance.ReplaceFontInUI(root);
	}

	public void Refresh(PvpModInfo _pvpModInfo)
	{
		pvpModInfo = _pvpModInfo;
		refreshSkill();
	}

	public void refreshSkill()
	{
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		heroList.Clear();
		HashSet<int> hashSet = new HashSet<int>();
		List<zbModCsvRecord> list = new List<zbModCsvRecord>();
		foreach (int mod in pvpModInfo.ModList)
		{
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(mod);
			if (recordByKey != null)
			{
				list.Add(recordByKey);
			}
		}
		foreach (int select in pvpModInfo.SelectList)
		{
			zbModCsvRecord recordByKey2 = zbDataConfig.Instance.modCsvTable.GetRecordByKey(select);
			if (recordByKey2 != null)
			{
				list.Add(recordByKey2);
			}
		}
		foreach (int mod2 in pvpModInfo.ModList)
		{
			foreach (SkillDetail skill in pvpModInfo.SkillList)
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
		foreach (int select2 in pvpModInfo.SelectList)
		{
			foreach (SkillDetail skill2 in pvpModInfo.SkillList)
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
		if (heroList.Count() <= 0)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int mod in pvpModInfo.ModList)
		{
			list.Add(mod);
		}
		foreach (int select in pvpModInfo.SelectList)
		{
			list.Add(select);
		}
		zbUIMgr.Instance.ShowSkillFightModWnd(0, heroList.ToArray(), list, isResumeFight: false);
	}
}
