using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class zbPvpBalanceDamageSub
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_rootElement;

	private TemplateContainer[] tc_selectMod = (TemplateContainer[])(object)new TemplateContainer[5];

	private VisualElement[] m_Progress = (VisualElement[])(object)new VisualElement[5];

	private VisualElement[] m_SkillIcon = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_lbSkillName = (Label[])(object)new Label[5];

	private Label[] m_lbDamagePro = (Label[])(object)new Label[5];

	private Label[] m_lbDamVal = (Label[])(object)new Label[5];

	private VisualElement m_icon;

	private Label m_lbName;

	private VisualElement m_headIcon;

	private VisualElement m_headFrame;

	private Label m_lbLevel;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_rootElement = root;
		for (int i = 0; i < 5; i++)
		{
			string text = "damage_" + (i + 1);
			tc_selectMod[i] = UQueryExtensions.Q<TemplateContainer>(m_rootElement, text, (string)null);
			m_lbSkillName[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMod[i], "skillName", (string)null);
			m_lbDamagePro[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMod[i], "damagePro", (string)null);
			m_SkillIcon[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tc_selectMod[i], "skillIcon", (string)null);
			m_Progress[i] = UQueryExtensions.Q<VisualElement>((VisualElement)(object)tc_selectMod[i], "progressValue", (string)null);
			m_lbDamVal[i] = UQueryExtensions.Q<Label>((VisualElement)(object)tc_selectMod[i], "lb_damVal", (string)null);
		}
		m_icon = UQueryExtensions.Q<VisualElement>(m_rootElement, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_rootElement, "lb_name", (string)null);
		m_headFrame = UQueryExtensions.Q<VisualElement>(m_rootElement, "headFrame", (string)null);
		m_headIcon = UQueryExtensions.Q<VisualElement>(m_rootElement, "head", (string)null);
		m_lbLevel = UQueryExtensions.Q<Label>(m_rootElement, "lb_level", (string)null);
	}

	public void Refresh(zbCombatData combatData)
	{
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		long damageSum = combatData.masterTeam.combatStat.damageSum;
		List<zbHero> heroList = combatData.masterTeam.heroList;
		List<zbHero> list = heroList.OrderByDescending(delegate(zbHero hero)
		{
			float num5 = combatData.masterTeam.combatStat.GetHeroDamageStat(hero.GetHeroTid());
			return ((damageSum == 0L) ? 0f : (num5 / (float)damageSum)) * 356f;
		}).ToList();
		for (int num = 0; num < 5; num++)
		{
			bool flag = num < heroList.Count;
			((VisualElement)tc_selectMod[num]).visible = flag;
			if (flag)
			{
				zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(list[num].skillDetail.SkillId);
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon);
				m_SkillIcon[num].style.backgroundImage = new StyleBackground(val);
				m_SkillIcon[num].transform.scale = Vector3.one * 0.9f;
				((TextElement)m_lbSkillName[num]).text = list[num].skillTypeRecord.name;
				float num2 = combatData.masterTeam.combatStat.GetHeroDamageStat(list[num].GetHeroTid());
				float num3 = ((damageSum == 0L) ? 0f : (num2 / (float)damageSum));
				float num4 = num3 * 356f;
				m_Progress[num].style.width = StyleLength.op_Implicit(new Length(num4, (LengthUnit)0));
				((TextElement)m_lbDamagePro[num]).text = (num3 * 100f).ToString("F2") + "%";
				((TextElement)m_lbDamVal[num]).text = ((num2 > 0f) ? zbUtil.ConvertToTBMK((long)num2) : null);
			}
		}
		UpdateHeadAndHeadFrameDisplay(combatData);
	}

	private void UpdateHeadAndHeadFrameDisplay(zbCombatData combatData)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		string playerHeadTex = zbResourceHelper.GetPlayerHeadTex(combatData.battleSideTeamInfo.MasterTeamInfo.RoleBaseInfo.HeadId);
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadTex);
		m_headIcon.style.backgroundImage = new StyleBackground(val);
		string playerHeadFrameTex = zbResourceHelper.GetPlayerHeadFrameTex(combatData.battleSideTeamInfo.MasterTeamInfo.RoleBaseInfo.HeadFrameId);
		Sprite val2 = zbAssetLoader.Instance.LoadResouce<Sprite>(playerHeadFrameTex);
		m_headFrame.style.backgroundImage = new StyleBackground(val2);
		((TextElement)m_lbName).text = combatData.battleSideTeamInfo.MasterTeamInfo.RoleBaseInfo.RoleName;
		((TextElement)m_lbLevel).text = combatData.battleSideTeamInfo.MasterTeamInfo.RoleBaseInfo.Level.ToString();
	}
}
