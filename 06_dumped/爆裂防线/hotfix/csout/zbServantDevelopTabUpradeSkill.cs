using System;
using System.Linq;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantDevelopTabUpradeSkill
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_iconCurSkill;

	private Label m_lbCurSkillLv;

	private VisualElement m_iconNextSkill;

	private Label m_lbNextSkillLv;

	private Label m_lbSkillDesc;

	private Label m_lbLvLimitDesc;

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private zbItem[] m_costItems = new zbItem[2];

	private Button m_btnUpgrade;

	private VisualElement m_upgradeNotify;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_iconCurSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_curSkill", (string)null);
		m_lbCurSkillLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_curSkill", (string)null);
		m_iconNextSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_nextSkill", (string)null);
		m_lbNextSkillLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_nextSkill", (string)null);
		m_lbLvLimitDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvlimitDesc", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_skillDesc", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_upgradeNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(0f, 0f), 1f, true);
		for (int i = 0; i < 2; i++)
		{
			m_lbCosts[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_costItems[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_costItems[i].OnEnable(m_wndParent);
		}
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			RefreshSkill();
			RefreshCost();
			RefreshNotify();
		}
	}

	private void RefreshSkill()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		int skillId = m_wndParent.m_record.skillId;
		zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(skillId);
		m_iconCurSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		m_iconNextSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		((TextElement)m_lbCurSkillLv).text = "Lv." + m_wndParent.m_detail.MainSkill.Level;
		((TextElement)m_lbNextSkillLv).text = "Lv." + (m_wndParent.m_detail.MainSkill.Level + 1);
		((TextElement)m_lbSkillDesc).text = string.Format(zbUtil.Language(recordByKey.desc), recordByKey.baseVar + recordByKey.growthVar * (float)m_wndParent.m_detail.MainSkill.Level);
	}

	private void RefreshCost()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		zbServantSkillUpgradeCsvRecord recordByKey = zbDataConfig.Instance.servantSkillUpgradeCsvTable.GetRecordByKey(m_wndParent.m_detail.MainSkill.Level);
		bool flag = zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_wndParent.m_detail);
		((VisualElement)m_lbLvLimitDesc).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(flag ? 1 : 0));
		if (!flag && !zbSelf.Instance.servantHandler.IsMainSkillLevelMax(m_wndParent.m_detail.MainSkill.Level))
		{
			zbServantSkillUpgradeCsvRecord recordByKey2 = zbDataConfig.Instance.servantSkillUpgradeCsvTable.GetRecordByKey(m_wndParent.m_detail.MainSkill.Level + 1);
			((TextElement)m_lbLvLimitDesc).text = string.Format(zbUtil.Language("lang.11068"), recordByKey2.lvLimit);
		}
		for (int i = 0; i < 2; i++)
		{
			bool flag2 = i < recordByKey.costList.Count();
			((VisualElement)m_lbCosts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			((VisualElement)m_costItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag2));
			if (flag2)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.costList[i][0], recordByKey.costList[i][1], recordByKey.costList[i][2]);
				m_costItems[i].Refresh(data, isNameVis: false, isCountVis: false);
				((TextElement)m_lbCosts[i]).enableRichText = true;
				((TextElement)m_lbCosts[i]).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)recordByKey.costList[i][0], recordByKey.costList[i][1]), recordByKey.costList[i][2]);
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_wndParent.m_detail)));
	}

	private void OnUpgrade()
	{
		if (m_wndParent.m_detail == null)
		{
			return;
		}
		if (zbSelf.Instance.servantHandler.CanServantUpgradeMainSkill(m_wndParent.m_detail))
		{
			zbWsMsgHandler.Instance.Servant_UpgradeSkill_Req(m_wndParent.m_detail.ServantId);
			return;
		}
		zbServantSkillUpgradeCsvRecord recordByKey = zbDataConfig.Instance.servantSkillUpgradeCsvTable.GetRecordByKey(m_wndParent.m_detail.MainSkill.Level + 1);
		if (recordByKey != null)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11068"), recordByKey.lvLimit));
		}
	}
}
