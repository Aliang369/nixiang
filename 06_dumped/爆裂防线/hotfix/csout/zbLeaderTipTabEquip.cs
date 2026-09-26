using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbLeaderTipTabEquip
{
	private zbLeaderTipWnd m_wndParent;

	private VisualElement m_RootElement;

	private VisualElement m_baseInfo;

	private VisualElement m_defenseInfo;

	private VisualElement m_attackInfo;

	private VisualElement m_dmgInfo;

	private zbTeamAttrCsvTableData m_records;

	private Dictionary<EAttrTeamType, float> m_teamInfo;

	private Dictionary<EAttrDefenseType, float> m_defenseInfoData;

	public void OnEnable(zbLeaderTipWnd wndParent, VisualElement root)
	{
		m_wndParent = wndParent;
		m_RootElement = root;
		m_baseInfo = UQueryExtensions.Q<VisualElement>(root, "baseInfo", (string)null);
		m_defenseInfo = UQueryExtensions.Q<VisualElement>(root, "defenseInfo", (string)null);
		m_attackInfo = UQueryExtensions.Q<VisualElement>(root, "attackInfo", (string)null);
		m_dmgInfo = UQueryExtensions.Q<VisualElement>(root, "dmgInfo", (string)null);
	}

	public void Refresh()
	{
		m_teamInfo = ((IEnumerable<BattleTeamAttrDetail>)zbServerData.sGameData.RoleTeamInfo.AttrList).ToDictionary((BattleTeamAttrDetail item) => item.Type, (BattleTeamAttrDetail item) => item.Value);
		m_defenseInfoData = ((IEnumerable<BattleDefenseAttrDetail>)zbServerData.sGameData.DefenseInfo.DefenseList[0].AttrList).ToDictionary((BattleDefenseAttrDetail item) => item.Type, (BattleDefenseAttrDetail item) => item.Value);
		m_records = zbDataConfig.Instance.teamAttrCsvTable.GetRecords();
		InitBaseInfo();
		InitDefenseInfo();
		InitAttackInfo();
		InitDmgInfo();
	}

	private string GetRecordNameByType(EAttrTeamType type)
	{
		foreach (zbTeamAttrCsvRecord value in m_records.records.Values)
		{
			if (value.id == (int)type)
			{
				return value.name;
			}
		}
		return string.Empty;
	}

	private void InitBaseInfo()
	{
		Label obj = UQueryExtensions.Q<Label>(m_baseInfo, "lb_attr0", (string)null);
		Label val = UQueryExtensions.Q<Label>(m_baseInfo, "lb_attr1", (string)null);
		((TextElement)obj).text = zbUtil.Language(GetRecordNameByType(EAttrTeamType.CritRate));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)obj, "lb_val", (string)null)).text = FormatAttrValue(m_teamInfo[EAttrTeamType.CritRate], 1);
		((TextElement)val).text = zbUtil.Language(GetRecordNameByType(EAttrTeamType.CritDamage));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_val", (string)null)).text = FormatAttrValue(m_teamInfo[EAttrTeamType.CritDamage], 2);
	}

	private void InitDefenseInfo()
	{
		SetDefenseAttr(m_defenseInfo, "lb_attr0", EAttrDefenseType.DefenseHp, 11);
		SetDefenseAttr(m_defenseInfo, "lb_attr1", EAttrDefenseType.ShieldHp, 12);
		SetDefenseAttr(m_defenseInfo, "lb_attr2", EAttrDefenseType.DamageDecr, 13);
		SetDefenseAttr(m_defenseInfo, "lb_attr3", EAttrDefenseType.DamageImmunity, 14);
		SetDefenseAttr(m_defenseInfo, "lb_attr4", EAttrDefenseType.EnergyRegen, 15);
	}

	private void SetDefenseAttr(VisualElement parent, string attrName, EAttrDefenseType type, int valueId)
	{
		Label obj = UQueryExtensions.Q<Label>(parent, attrName, (string)null);
		((TextElement)obj).text = zbUtil.Language(GetRecordNameByType((EAttrTeamType)type));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)obj, "lb_val", (string)null)).text = FormatAttrValue(m_defenseInfoData[type], valueId);
	}

	private void InitAttackInfo()
	{
		SetAttr(m_attackInfo, "lb_attr0", EAttrTeamType.Attack, 21);
		SetAttr(m_attackInfo, "lb_attr1", EAttrTeamType.ElecAttack, 22);
		SetAttr(m_attackInfo, "lb_attr2", EAttrTeamType.FireAttack, 23);
		SetAttr(m_attackInfo, "lb_attr3", EAttrTeamType.IceAttack, 24);
		SetAttr(m_attackInfo, "lb_attr4", EAttrTeamType.PhyAttack, 25);
		SetAttr(m_attackInfo, "lb_attr5", EAttrTeamType.EnergyAttack, 26);
		SetAttr(m_attackInfo, "lb_attr6", EAttrTeamType.WindAttack, 27);
	}

	private void InitDmgInfo()
	{
		SetAttr(m_dmgInfo, "lb_attr0", EAttrTeamType.DamagePct, 31);
		SetAttr(m_dmgInfo, "lb_attr1", EAttrTeamType.ElecDamagePct, 32);
		SetAttr(m_dmgInfo, "lb_attr2", EAttrTeamType.FireDamagePct, 33);
		SetAttr(m_dmgInfo, "lb_attr3", EAttrTeamType.IceDamagePct, 34);
		SetAttr(m_dmgInfo, "lb_attr4", EAttrTeamType.PhyDamagePct, 35);
		SetAttr(m_dmgInfo, "lb_attr5", EAttrTeamType.EnergyDamagePct, 36);
		SetAttr(m_dmgInfo, "lb_attr6", EAttrTeamType.WindDamagePct, 37);
	}

	private void SetAttr(VisualElement parent, string attrName, EAttrTeamType type, int valueId)
	{
		Label obj = UQueryExtensions.Q<Label>(parent, attrName, (string)null);
		((TextElement)obj).text = zbUtil.Language(GetRecordNameByType(type));
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)obj, "lb_val", (string)null)).text = FormatAttrValue(m_teamInfo[type], valueId);
	}

	private string FormatAttrValue(float value, int valueId)
	{
		bool isPercentageDisplay = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey(valueId).showPercent > 0;
		return zbAttrHelper.FormatAttrValue(value, isPercentageDisplay);
	}
}
