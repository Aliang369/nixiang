using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatDamageHelp : VisualElement
{
	public VisualElement m_root;

	private zbCombatWnd m_parentWnd;

	private VisualElement m_selfDmg;

	private VisualElement m_helpDmg;

	private VisualElement m_selfProgressValue;

	private VisualElement m_helpProgressValue;

	private Label m_selfDamagePro;

	private Label m_helpDamagePro;

	private Label m_selfDamVal;

	private Label m_helpDamVal;

	private Label m_selfName;

	private Label m_helpName;

	private VisualElement m_selfFrame;

	private VisualElement m_helpFrame;

	private VisualElement m_selfIcon;

	private VisualElement m_helpIcon;

	private VisualElement[] tc_selectSelfMods;

	private VisualElement[] tc_selectHelpMods;

	private int currIndex;

	private readonly Dictionary<int, Sprite> m_iconCache;

	private readonly string[] m_lastSelfNames;

	private readonly string[] m_lastHelpNames;

	private Button m_btnArrowDown;

	public zbCombatDamageHelp(zbCombatWnd parentWnd, VisualElement root)
	{
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		tc_selectSelfMods = (VisualElement[])(object)new VisualElement[5];
		tc_selectHelpMods = (VisualElement[])(object)new VisualElement[5];
		m_iconCache = new Dictionary<int, Sprite>();
		m_lastSelfNames = new string[5];
		m_lastHelpNames = new string[5];
		((VisualElement)this)._002Ector();
		m_root = root;
		m_parentWnd = parentWnd;
		m_selfDmg = UQueryExtensions.Q<VisualElement>(m_root, "selfDmg", (string)null);
		m_helpDmg = UQueryExtensions.Q<VisualElement>(m_root, "helpDmg", (string)null);
		m_selfProgressValue = UQueryExtensions.Q<VisualElement>(m_selfDmg, "progressValue", (string)null);
		m_selfDamagePro = UQueryExtensions.Q<Label>(m_selfDmg, "damagePro", (string)null);
		m_selfDamVal = UQueryExtensions.Q<Label>(m_selfDmg, "lb_damVal", (string)null);
		m_selfName = UQueryExtensions.Q<Label>(m_selfDmg, "skillName", (string)null);
		m_selfFrame = UQueryExtensions.Q<VisualElement>(m_selfDmg, "skillFrame", (string)null);
		m_selfIcon = UQueryExtensions.Q<VisualElement>(m_selfDmg, "skillIcon", (string)null);
		m_helpProgressValue = UQueryExtensions.Q<VisualElement>(m_helpDmg, "progressValue", (string)null);
		m_helpDamagePro = UQueryExtensions.Q<Label>(m_helpDmg, "damagePro", (string)null);
		m_helpDamVal = UQueryExtensions.Q<Label>(m_helpDmg, "lb_damVal", (string)null);
		m_helpName = UQueryExtensions.Q<Label>(m_helpDmg, "skillName", (string)null);
		m_helpFrame = UQueryExtensions.Q<VisualElement>(m_helpDmg, "skillFrame", (string)null);
		m_helpIcon = UQueryExtensions.Q<VisualElement>(m_helpDmg, "skillIcon", (string)null);
		m_selfProgressValue.style.width = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
		m_helpProgressValue.style.width = StyleLength.op_Implicit(new Length(0f, (LengthUnit)0));
		m_btnArrowDown = UQueryExtensions.Q<Button>(m_root, "button_data", (string)null);
		m_parentWnd.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnArrowDown, (Action)OnArrowDown, "", (TrickleDown)0);
		for (int i = 0; i < 5; i++)
		{
			tc_selectSelfMods[i] = UQueryExtensions.Q<VisualElement>(m_root, $"dmg{i}", (string)null);
			tc_selectHelpMods[i] = UQueryExtensions.Q<VisualElement>(m_root, $"dmg{i + 10}", (string)null);
			tc_selectSelfMods[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			tc_selectHelpMods[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public void Update()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!(m_root.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1)))
		{
			long damageSum = zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageSum;
			long damageSum2 = zbCombatSystem.Instance.combatData.helperTeam.combatStat.damageSum;
			float num = ((damageSum + damageSum2 == 0L) ? 0f : ((float)damageSum / (float)(damageSum + damageSum2)));
			float newWidthPx = num * 260f;
			zbUtil.SetWidthIfChanged(m_selfProgressValue, newWidthPx);
			zbUtil.SetLabelIfChanged(m_selfDamagePro, (num * 100f).ToString("F2") + "%");
			zbUtil.SetLabelIfChanged(m_selfDamVal, (damageSum > 0) ? zbUtil.ConvertToTBMK(damageSum) : null);
			num = ((damageSum + damageSum2 == 0L) ? 0f : ((float)damageSum2 / (float)(damageSum + damageSum2)));
			newWidthPx = num * 260f;
			zbUtil.SetWidthIfChanged(m_helpProgressValue, newWidthPx);
			zbUtil.SetLabelIfChanged(m_helpDamagePro, (num * 100f).ToString("F2") + "%");
			zbUtil.SetLabelIfChanged(m_helpDamVal, (damageSum2 > 0) ? zbUtil.ConvertToTBMK(damageSum2) : null);
			UpdatePanel();
		}
	}

	public void UpdatePanel()
	{
		if (currIndex != 0)
		{
			RefreshDetail(tc_selectSelfMods, zbCombatSystem.Instance.combatData.masterTeam, m_lastSelfNames);
			RefreshDetail(tc_selectHelpMods, zbCombatSystem.Instance.combatData.helperTeam, m_lastHelpNames);
		}
	}

	public void RefreshDetail(VisualElement[] selectMods, zbCombatTeam team, string[] lastNameCache)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		long damageSum = team.combatStat.damageSum;
		List<zbHero> heroList = team.heroList;
		List<zbHero> list = heroList.OrderByDescending(delegate(zbHero hero)
		{
			float num5 = team.combatStat.GetHeroDamageStat(hero.GetHeroTid());
			return ((damageSum == 0L) ? 0f : (num5 / (float)damageSum)) * 260f;
		}).ToList();
		for (int num = 0; num < 5; num++)
		{
			bool flag = num < heroList.Count;
			selectMods[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(list[num].skillDetail.SkillId);
				int skillId = list[num].skillDetail.SkillId;
				if (!m_iconCache.TryGetValue(skillId, out var value))
				{
					value = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
					m_iconCache[skillId] = value;
				}
				UQueryExtensions.Q<VisualElement>(selectMods[num], "skillFrame", (string)null).style.backgroundImage = new StyleBackground(value);
				string name = list[num].skillTypeRecord.name;
				if (lastNameCache[num] != name)
				{
					lastNameCache[num] = name;
					((TextElement)UQueryExtensions.Q<Label>(selectMods[num], "skillName", (string)null)).text = name;
				}
				float num2 = team.combatStat.GetHeroDamageStat(list[num].GetHeroTid());
				float num3 = ((damageSum == 0L) ? 0f : (num2 / (float)damageSum));
				float num4 = num3 * 260f;
				UQueryExtensions.Q<VisualElement>(selectMods[num], "progressValue", (string)null).style.width = StyleLength.op_Implicit(new Length(num4, (LengthUnit)0));
				zbUtil.SetLabelIfChanged(UQueryExtensions.Q<Label>(selectMods[num], "damagePro", (string)null), (num3 * 100f).ToString("F2") + "%");
				zbUtil.SetLabelIfChanged(UQueryExtensions.Q<Label>(selectMods[num], "lb_damVal", (string)null), (num2 > 0f) ? zbUtil.ConvertToTBMK((long)num2) : null);
			}
		}
	}

	public void OnArrowDown()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		currIndex = 1 - currIndex;
		if (currIndex == 0)
		{
			for (int i = 0; i < 5; i++)
			{
				tc_selectSelfMods[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				tc_selectHelpMods[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
		}
	}

	public void Show()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.HeadId);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.HeadFrameId);
		m_selfIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		m_selfFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		playerHeadTex = zbResourceHelper.GetPlayerHeadTex(zbCombatSystem.Instance.combatData.helperTeam.battleTeamInfo.RoleBaseInfo.HeadId);
		playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(zbCombatSystem.Instance.combatData.helperTeam.battleTeamInfo.RoleBaseInfo.HeadFrameId);
		m_helpIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex));
		m_helpFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex));
		((TextElement)m_selfName).text = zbCombatSystem.Instance.combatData.masterTeam.battleTeamInfo.RoleBaseInfo.RoleName;
		((TextElement)m_helpName).text = zbCombatSystem.Instance.combatData.helperTeam.battleTeamInfo.RoleBaseInfo.RoleName;
	}

	public void Close()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
