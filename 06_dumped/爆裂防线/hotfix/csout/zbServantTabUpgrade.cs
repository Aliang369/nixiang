using System;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantTabUpgrade
{
	private zbServantWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_iconSkill;

	private Label m_lbSkillDesc;

	private Label m_lbLv;

	private VisualElement m_mainAttr;

	private Label m_mainAttrKey;

	private Label m_mainAttrValLeft;

	private Label m_mainAttrValRight;

	private VisualElement[] m_subAttrs = (VisualElement[])(object)new VisualElement[5];

	private Label[] m_subAttrsKey = (Label[])(object)new Label[5];

	private Label[] m_subAttrsValLeft = (Label[])(object)new Label[5];

	private Label[] m_subAttrsValRight = (Label[])(object)new Label[5];

	private zbItem[] m_costItems = new zbItem[2];

	private Label[] m_lbCosts = (Label[])(object)new Label[2];

	private Button m_btnUpgrade;

	private VisualElement m_upgradeNotify;

	public void OnEnable(zbServantWnd wndParent, VisualElement root)
	{
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_iconSkill = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_skill", (string)null);
		m_lbSkillDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_skillDesc", (string)null);
		m_lbLv = UQueryExtensions.Q<Label>(m_RootElement, "lb_lv", (string)null);
		m_mainAttr = UQueryExtensions.Q<VisualElement>(m_RootElement, "mainAttr", (string)null);
		m_mainAttrKey = UQueryExtensions.Q<Label>(m_mainAttr, "lb_key", (string)null);
		m_mainAttrValLeft = UQueryExtensions.Q<Label>(m_mainAttr, "lb_val", (string)null);
		m_mainAttrValRight = UQueryExtensions.Q<Label>(m_mainAttr, "lb_nextVal", (string)null);
		for (int i = 0; i < 5; i++)
		{
			m_subAttrs[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + i, (string)null);
			m_subAttrsKey[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_key", (string)null);
			m_subAttrsValLeft[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_val", (string)null);
			m_subAttrsValRight[i] = UQueryExtensions.Q<Label>(m_subAttrs[i], "lb_nextVal", (string)null);
		}
		for (int j = 0; j < 2; j++)
		{
			m_costItems[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_costItems[j].OnEnable(m_wndParent);
			m_lbCosts[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
		}
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_upgradeNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(0f, 0f), 1f, true);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		if (m_wndParent.m_detail != null)
		{
			RefreshSkill();
			RefreshAttrs();
			RefreshCost();
			RefreshNotify();
		}
	}

	private void RefreshSkill()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		int skillId = m_wndParent.m_record.skillId;
		zbServantSkillCsvRecord recordByKey = zbDataConfig.Instance.servantSkillCsvTable.GetRecordByKey(skillId);
		m_iconSkill.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		((TextElement)m_lbSkillDesc).text = string.Format(zbUtil.Language(recordByKey.desc), recordByKey.baseVar + recordByKey.growthVar * (float)m_wndParent.m_detail.MainSkill.Level);
	}

	private void RefreshAttrs()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		zbServantQualityCsvRecord recordByKey = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_wndParent.m_detail.Quality);
		((TextElement)m_lbLv).text = $"{m_wndParent.m_detail.Level} / {recordByKey.lvLimit} ";
		zbServantAttrCsvRecord recordByKey2 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(m_wndParent.m_record.mainAttrId);
		Color val = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(m_wndParent.m_detail.Quality));
		((VisualElement)m_mainAttrKey).style.color = StyleColor.op_Implicit(val);
		((TextElement)m_mainAttrKey).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey2.attrType, recordByKey2.attr[0], recordByKey2.attr[1]);
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_wndParent.m_detail.Quality, recordByKey2);
		_ = m_wndParent.m_detail.Level;
		((TextElement)m_mainAttrValLeft).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_wndParent.m_record.mainAttrId, m_wndParent.m_detail.Quality, m_wndParent.m_detail.Level);
		bool flag = recordByKey.lvLimit == m_wndParent.m_detail.Level;
		bool flag2 = m_wndParent.m_detail.Quality == (EQuality)zbDataConfig.Instance.servantQualityCsvTable.GetRecords().records.Count;
		((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((flag & flag2) ? 1 : 0));
		zbSelf.Instance.servantHandler.GetAttrValByQuality(m_wndParent.m_detail.Quality, recordByKey2);
		_ = m_wndParent.m_detail.Level;
		if (flag & flag2)
		{
			((TextElement)m_mainAttrValRight).text = "Max";
		}
		else if (flag)
		{
			((TextElement)m_mainAttrValRight).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_wndParent.m_record.mainAttrId, m_wndParent.m_detail.Quality + 1, m_wndParent.m_detail.Level + 1);
		}
		else
		{
			((TextElement)m_mainAttrValRight).text = zbSelf.Instance.servantHandler.FormatAttrVal(m_wndParent.m_record.mainAttrId, m_wndParent.m_detail.Quality, m_wndParent.m_detail.Level + 1);
		}
		for (int i = 0; i < 5; i++)
		{
			bool flag3 = i < m_wndParent.m_detail.SubAttrList.Count;
			m_subAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag3));
			if (flag3)
			{
				RepeatedField<ServantAttrDetail> subAttrList = m_wndParent.m_detail.SubAttrList;
				zbServantAttrCsvRecord recordByKey3 = zbDataConfig.Instance.servantAttrCsvTable.GetRecordByKey(subAttrList[i].AttrId);
				Color val2 = zbUtil.ParseHexColor(zbSelf.Instance.servantHandler.GetColorByQuality(subAttrList[i].Quality));
				((VisualElement)m_subAttrsKey[i]).style.color = StyleColor.op_Implicit(val2);
				((TextElement)m_subAttrsKey[i]).text = zbAttrHelper.GetAttrName((EAttrType)recordByKey3.attrType, recordByKey3.attr[0], recordByKey3.attr[1]);
				zbSelf.Instance.servantHandler.GetAttrValByQuality(subAttrList[i].Quality, recordByKey3);
				_ = m_wndParent.m_detail.Level;
				((TextElement)m_subAttrsValLeft[i]).text = zbSelf.Instance.servantHandler.FormatAttrVal(subAttrList[i].AttrId, subAttrList[i].Quality, m_wndParent.m_detail.Level);
				if (flag & flag2)
				{
					((TextElement)m_subAttrsValRight[i]).text = "Max";
				}
				else if (flag)
				{
					((TextElement)m_subAttrsValRight[i]).text = zbSelf.Instance.servantHandler.FormatAttrVal(subAttrList[i].AttrId, subAttrList[i].Quality + 1, m_wndParent.m_detail.Level + 1);
				}
				else
				{
					((TextElement)m_subAttrsValRight[i]).text = zbSelf.Instance.servantHandler.FormatAttrVal(subAttrList[i].AttrId, subAttrList[i].Quality, m_wndParent.m_detail.Level + 1);
				}
			}
		}
	}

	private void RefreshCost()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		zbServantUpgradeCsvRecord recordByKey = zbDataConfig.Instance.servantUpgradeCsvTable.GetRecordByKey(m_wndParent.m_detail.Level);
		for (int i = 0; i < 2; i++)
		{
			bool flag = i < recordByKey.costList.Count();
			((VisualElement)m_lbCosts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			((VisualElement)m_costItems[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!flag));
			if (flag)
			{
				zbItemData data = new zbItemData((EResourceType)recordByKey.costList[i][0], recordByKey.costList[i][1], recordByKey.costList[i][2]);
				m_costItems[i].Refresh(data, isNameVis: false, isCountVis: false);
				((TextElement)m_lbCosts[i]).enableRichText = true;
				((TextElement)m_lbCosts[i]).text = zbUtil.GetNeedHaveRichString(zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)recordByKey.costList[i][0], recordByKey.costList[i][1]), recordByKey.costList[i][2]);
			}
		}
	}

	private void OnUpgrade()
	{
		if (m_wndParent.m_detail == null)
		{
			return;
		}
		bool flag = zbDataConfig.Instance.servantQualityCsvTable.GetRecordByKey((int)m_wndParent.m_detail.Quality).lvLimit == m_wndParent.m_detail.Level;
		bool flag2 = m_wndParent.m_detail.Quality == (EQuality)zbDataConfig.Instance.servantQualityCsvTable.GetRecords().records.Count;
		if (!zbSelf.Instance.servantHandler.CanServantUpgrade(m_wndParent.m_detail))
		{
			int upgradeNeedHeroLv = zbSelf.Instance.servantHandler.GetUpgradeNeedHeroLv(m_wndParent.m_detail);
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.11065"), upgradeNeedHeroLv));
		}
		else if (!(flag & flag2))
		{
			if (flag)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11041"));
			}
			else
			{
				zbWsMsgHandler.Instance.Servant_UpgradeLevel_Req(m_wndParent.m_detail.ServantId);
			}
		}
	}

	private void RefreshNotify()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.servantHandler.CanServantUpgrade(m_wndParent.m_detail)));
	}
}
