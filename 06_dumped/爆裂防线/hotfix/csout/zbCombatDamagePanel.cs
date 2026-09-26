using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbCombatDamagePanel : VisualElement
{
	public VisualElement m_root;

	private zbCombatWnd m_parentWnd;

	private TemplateContainer[] tc_selectMods;

	private VisualElement[] m_slotIcon;

	private VisualElement[] m_slotProgress;

	private Label[] m_slotName;

	private Label[] m_slotDamagePro;

	private Label[] m_slotDamVal;

	private int[] m_slotHeroTid;

	public zbCombatDamagePanel(zbCombatWnd parentWnd, VisualElement root)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		tc_selectMods = (TemplateContainer[])(object)new TemplateContainer[5];
		m_slotIcon = (VisualElement[])(object)new VisualElement[5];
		m_slotProgress = (VisualElement[])(object)new VisualElement[5];
		m_slotName = (Label[])(object)new Label[5];
		m_slotDamagePro = (Label[])(object)new Label[5];
		m_slotDamVal = (Label[])(object)new Label[5];
		m_slotHeroTid = new int[5];
		((VisualElement)this)._002Ector();
		m_root = root;
		m_parentWnd = parentWnd;
		for (int i = 0; i < 5; i++)
		{
			tc_selectMods[i] = UQueryExtensions.Q<TemplateContainer>(m_root, $"dmg{i}", (string)null);
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMods[i], "skillName", (string)null)).style.fontSize = StyleLength.op_Implicit(22f);
			m_slotIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tc_selectMods[i], "skillIcon", (string)null);
			m_slotProgress[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tc_selectMods[i], "progressValue", (string)null);
			m_slotName[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMods[i], "skillName", (string)null);
			m_slotDamagePro[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMods[i], "damagePro", (string)null);
			m_slotDamVal[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMods[i], "lb_damVal", (string)null);
			m_slotHeroTid[i] = -1;
		}
	}

	public void Update()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if (m_root.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1))
		{
			return;
		}
		long damageSum = zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageSum;
		List<zbHero> heroList = zbCombatSystem.Instance.combatData.masterTeam.heroList;
		List<zbHero> list = heroList.OrderByDescending(delegate(zbHero hero)
		{
			float num4 = zbCombatSystem.Instance.combatData.masterTeam.combatStat.GetHeroDamageStat(hero.GetHeroTid());
			return ((damageSum == 0L) ? 0f : (num4 / (float)damageSum)) * 356f;
		}).ToList();
		for (int num = 0; num < 5; num++)
		{
			bool flag = num < heroList.Count;
			((VisualElement)tc_selectMods[num]).visible = flag;
			if (flag)
			{
				int heroTid = list[num].GetHeroTid();
				if (m_slotHeroTid[num] != heroTid)
				{
					m_slotHeroTid[num] = heroTid;
					zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(list[num].skillDetail.SkillId);
					Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
					m_slotIcon[num].style.backgroundImage = new StyleBackground(val);
					m_slotIcon[num].transform.scale = Vector3.one * 0.9f;
					((TextElement)m_slotName[num]).text = list[num].skillTypeRecord.name;
				}
				float num2 = zbCombatSystem.Instance.combatData.masterTeam.combatStat.GetHeroDamageStat(heroTid);
				float num3 = ((damageSum == 0L) ? 0f : (num2 / (float)damageSum));
				float newWidthPx = num3 * 356f;
				zbUtil.SetWidthIfChanged(m_slotProgress[num], newWidthPx);
				zbUtil.SetLabelIfChanged(m_slotDamagePro[num], (num3 * 100f).ToString("F2") + "%");
				zbUtil.SetLabelIfChanged(m_slotDamVal[num], (num2 > 0f) ? zbUtil.ConvertToTBMK((long)num2) : null);
			}
		}
	}

	public void Show()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
	}

	public void Close()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		m_root.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}
}
