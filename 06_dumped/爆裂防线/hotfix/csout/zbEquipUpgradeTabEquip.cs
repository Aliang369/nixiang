using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEquipUpgradeTabEquip
{
	private zbEquipUpgradeWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevelCur;

	private Label m_lbLevelNext;

	private VisualElement m_arrow;

	private VisualElement m_nodeSubAttr;

	private VisualElement[] m_nodeSubAttrs = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbSubAttrCurs = (Label[])(object)new Label[6];

	private VisualElement[] m_subAttrArrows = (VisualElement[])(object)new VisualElement[6];

	private Label[] m_lbSubAttrNexts = (Label[])(object)new Label[6];

	private VisualElement m_costList;

	private Label[] m_costLabels = (Label[])(object)new Label[1];

	private zbItem[] m_items = new zbItem[1];

	private Button m_btnEquipUpgrade;

	private zbItemData m_needItemData;

	public void OnEnable(zbEquipUpgradeWnd wndParent, VisualElement root)
	{
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLevelCur = UQueryExtensions.Q<Label>(m_RootElement, "lb_cur", (string)null);
		m_lbLevelNext = UQueryExtensions.Q<Label>(m_RootElement, "lb_next", (string)null);
		m_arrow = UQueryExtensions.Q<VisualElement>(m_RootElement, "arrow", (string)null);
		m_nodeSubAttr = (VisualElement)(object)UQueryExtensions.Q<Label>(m_RootElement, "lb_title_sub", (string)null);
		for (int i = 0; i < 6; i++)
		{
			m_nodeSubAttrs[i] = UQueryExtensions.Q<VisualElement>(m_nodeSubAttr, "sub" + i, (string)null);
			m_lbSubAttrCurs[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrs[i], "lb_cur", (string)null);
			m_subAttrArrows[i] = UQueryExtensions.Q<VisualElement>(m_nodeSubAttrs[i], "arrow", (string)null);
			m_lbSubAttrNexts[i] = UQueryExtensions.Q<Label>(m_nodeSubAttrs[i], "lb_next", (string)null);
		}
		zbUtil.CreateRowMoveAction(m_arrow);
		for (int j = 0; j < 6; j++)
		{
			zbUtil.CreateRowMoveAction(m_subAttrArrows[j]);
		}
		m_costList = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int k = 0; k < 1; k++)
		{
			m_costLabels[k] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + k, (string)null);
			m_items[k] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + k, (string)null);
			m_items[k].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[k]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnEquipUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEquipUpgrade, (Action<ClickEvent>)delegate
		{
			OnLevelUpgrade();
		}, "", (TrickleDown)0);
	}

	public void Refresh()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		m_needItemData = null;
		EquipSlotDetail equipSlotInfo = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_wndParent.m_curEquipType);
		if (equipSlotInfo.Equip == null)
		{
			m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		m_RootElement.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		((TextElement)m_lbLevelCur).text = equipSlotInfo.Equip.SubLevel.ToString();
		zbEquipUpgradeSubCsvRecord equipUpgradeSubRecord = GetEquipUpgradeSubRecord(equipSlotInfo.Equip.Quality, equipSlotInfo.Equip.SubLevel + 1);
		if (equipUpgradeSubRecord != null && equipUpgradeSubRecord.equipQuality <= (int)equipSlotInfo.Equip.Quality)
		{
			int[][] array = new int[1][] { new int[3] { 1, 12, equipUpgradeSubRecord.exp } };
			zbSelf.Instance.homelandHandler.UpdateCostUI(array, m_costLabels, m_items);
			m_needItemData = zbSelf.Instance.dataHandler.GetCostListLackItem(array);
			((TextElement)m_lbLevelNext).text = (equipSlotInfo.Equip.SubLevel + 1).ToString();
			m_costList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnEquipUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((TextElement)m_lbLevelNext).text = "Max";
			m_costList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnEquipUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		for (int i = 0; i < 6; i++)
		{
			((VisualElement)m_lbSubAttrCurs[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_subAttrArrows[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbSubAttrNexts[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		int num = 0;
		foreach (EquipSubAttrDetail subAttr in equipSlotInfo.Equip.SubAttrList)
		{
			if (num < m_nodeSubAttrs.Length)
			{
				((VisualElement)m_lbSubAttrCurs[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_subAttrArrows[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				((VisualElement)m_lbSubAttrNexts[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				string text = "";
				string text2 = "Max";
				if (subAttr.ShowPct == EBoolState.False)
				{
					text = (subAttr.Value * (float)equipSlotInfo.Equip.SubLevel).ToString("F2");
					if (equipUpgradeSubRecord != null && equipUpgradeSubRecord.equipQuality <= (int)equipSlotInfo.Equip.Quality)
					{
						text2 = (subAttr.Value * (float)(equipSlotInfo.Equip.SubLevel + 1)).ToString("F2");
					}
				}
				else
				{
					text = (subAttr.Value * (float)equipSlotInfo.Equip.SubLevel * 100f).ToString("F2") + "%";
					if (equipUpgradeSubRecord != null && equipUpgradeSubRecord.equipQuality <= (int)equipSlotInfo.Equip.Quality)
					{
						text2 = (subAttr.Value * (float)(equipSlotInfo.Equip.SubLevel + 1) * 100f).ToString("F2") + "%";
					}
				}
				((TextElement)m_lbSubAttrCurs[num]).text = zbAttrHelper.GetAttrName(subAttr.AttrType, subAttr.FuncId, subAttr.AttrId) + ": " + text;
				((TextElement)m_lbSubAttrNexts[num]).text = text2;
			}
			num++;
		}
	}

	private zbEquipUpgradeSubCsvRecord GetEquipUpgradeSubRecord(EQuality quality, int level)
	{
		zbEquipUpgradeSubCsvRecord[] records = zbDataConfig.Instance.equipUpgradeSubCsvTable.GetRecords().records;
		foreach (zbEquipUpgradeSubCsvRecord zbEquipUpgradeSubCsvRecord2 in records)
		{
			if (zbEquipUpgradeSubCsvRecord2.id == (int)quality && zbEquipUpgradeSubCsvRecord2.lv == level)
			{
				return zbEquipUpgradeSubCsvRecord2;
			}
		}
		return null;
	}

	public void OnLevelUpgrade()
	{
		if (m_needItemData != null)
		{
			zbUIMgr.Instance.ShowTip(m_needItemData.type, m_needItemData.id, m_needItemData.count);
		}
		else
		{
			zbWsMsgHandler.Instance.EquipSlot_EquipUpgrade_Req(m_wndParent.m_curEquipType);
		}
	}
}
