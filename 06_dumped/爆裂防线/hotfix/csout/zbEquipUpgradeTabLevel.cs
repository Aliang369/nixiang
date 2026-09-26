using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine.UIElements;

public class zbEquipUpgradeTabLevel
{
	private zbEquipUpgradeWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevelCur;

	private Label m_lbLevelNext;

	private Label m_lbLevelMax;

	private VisualElement m_arrow;

	private Label m_lbMainAttrCur;

	private Label m_lbMainAttrNext;

	private Label m_lbMainAttrMax;

	private VisualElement m_mainAttrArrow;

	private VisualElement m_costList;

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnLevelUpgrade;

	private Button m_btnLevelUpgradeAll;

	private VisualElement m_notify;

	private zbItemData m_needItemData;

	public void OnEnable(zbEquipUpgradeWnd wndParent, VisualElement root)
	{
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		VisualElement val = UQueryExtensions.Q<VisualElement>(m_RootElement, "level", (string)null);
		m_lbLevelCur = UQueryExtensions.Q<Label>(val, "lb_cur", (string)null);
		m_lbLevelNext = UQueryExtensions.Q<Label>(val, "lb_next", (string)null);
		m_lbLevelMax = UQueryExtensions.Q<Label>(val, "lb_max", (string)null);
		m_arrow = UQueryExtensions.Q<VisualElement>(val, "arrow", (string)null);
		zbUtil.CreateRowMoveAction(m_arrow);
		Label val2 = UQueryExtensions.Q<Label>(m_RootElement, "lb_title_main", (string)null);
		m_lbMainAttrCur = UQueryExtensions.Q<Label>((VisualElement)(object)val2, "lb_cur", (string)null);
		m_lbMainAttrNext = UQueryExtensions.Q<Label>((VisualElement)(object)val2, "lb_next", (string)null);
		m_lbMainAttrMax = UQueryExtensions.Q<Label>((VisualElement)(object)val2, "lb_max", (string)null);
		m_mainAttrArrow = UQueryExtensions.Q<VisualElement>((VisualElement)(object)val2, "arrow", (string)null);
		zbUtil.CreateRowMoveAction(m_mainAttrArrow);
		m_costList = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_costLabels[i] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + i, (string)null);
			m_items[i].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnLevelUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnLevelUpgradeAll = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgradeAll", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLevelUpgrade, (Action<ClickEvent>)delegate
		{
			OnLevelUpgrade();
		}, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLevelUpgradeAll, (Action<ClickEvent>)delegate
		{
			OnLevelUpgradeAll();
		}, "", (TrickleDown)0);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLevelUpgradeAll);
	}

	public void Refresh()
	{
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		EquipSlotDetail equipSlotInfo = zbSelf.Instance.equipHandler.GetEquipSlotInfo(m_wndParent.m_curEquipType);
		((TextElement)m_lbLevelCur).text = "Lv." + equipSlotInfo.Level;
		((TextElement)m_lbLevelMax).text = "Lv." + equipSlotInfo.Level + "<color=#008415>(Max)</color>";
		if (equipSlotInfo.Equip != null)
		{
			((TextElement)m_lbMainAttrCur).text = zbUtil.Language("lang.10039") + zbSelf.Instance.equipHandler.GetEquipMainAttrValue(equipSlotInfo.Equip.EquipTid, equipSlotInfo.Equip.Quality);
			((TextElement)m_lbMainAttrMax).text = zbUtil.Language("lang.10039") + zbSelf.Instance.equipHandler.GetEquipMainAttrValue(equipSlotInfo.Equip.EquipTid, equipSlotInfo.Equip.Quality);
			((TextElement)m_lbMainAttrNext).text = zbUtil.Language("lang.10039") + zbSelf.Instance.equipHandler.GetEquipMainAttrValue(equipSlotInfo.Equip.EquipTid, equipSlotInfo.Equip.Quality, isNextLevel: true);
			m_mainAttrArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((TextElement)m_lbMainAttrCur).text = "";
			((TextElement)m_lbMainAttrNext).text = "";
			m_mainAttrArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbEquipSlotUpgradeCsvRecord equipSlotUpgradeRecord = GetEquipSlotUpgradeRecord(equipSlotInfo.EquipType, equipSlotInfo.Level + 1);
		if (equipSlotUpgradeRecord != null)
		{
			zbSelf.Instance.homelandHandler.UpdateCostUI(equipSlotUpgradeRecord.costList, m_costLabels, m_items);
			m_needItemData = zbSelf.Instance.dataHandler.GetCostListLackItem(equipSlotUpgradeRecord.costList);
			((TextElement)m_lbLevelNext).text = "Lv." + (equipSlotInfo.Level + 1);
			((VisualElement)m_btnLevelUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnLevelUpgradeAll).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_arrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_mainAttrArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbLevelMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbMainAttrMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_costList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((TextElement)m_lbLevelCur).text = "";
			((TextElement)m_lbMainAttrCur).text = "";
			((TextElement)m_lbLevelNext).text = "";
			((TextElement)m_lbMainAttrNext).text = "";
			((VisualElement)m_btnLevelUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnLevelUpgradeAll).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_arrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_mainAttrArrow.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLevelMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbMainAttrMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_costList.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.equipHandler.CanEquipSlotUpgrade(equipSlotInfo.EquipType)));
	}

	private zbEquipSlotUpgradeCsvRecord GetEquipSlotUpgradeRecord(EEquipType equipType, int level)
	{
		zbEquipSlotUpgradeCsvRecord[] records = zbDataConfig.Instance.equipSlotUpgradeCsvTable.GetRecords().records;
		foreach (zbEquipSlotUpgradeCsvRecord zbEquipSlotUpgradeCsvRecord2 in records)
		{
			if (zbEquipSlotUpgradeCsvRecord2.type == (int)equipType && zbEquipSlotUpgradeCsvRecord2.lv == level)
			{
				return zbEquipSlotUpgradeCsvRecord2;
			}
		}
		return null;
	}

	private void OnLevelUpgrade()
	{
		if (m_needItemData != null)
		{
			zbUIMgr.Instance.ShowTip(m_needItemData.type, m_needItemData.id, m_needItemData.count);
		}
		else
		{
			zbWsMsgHandler.Instance.EquipSlot_LevelUpgrade_Req(m_wndParent.m_curEquipType);
		}
	}

	private void OnLevelUpgradeAll()
	{
		if (zbSelf.Instance.equipHandler.CanEquipSlotUpgrade(EEquipType.Neuron) || zbSelf.Instance.equipHandler.CanEquipSlotUpgrade(EEquipType.Mnsr) || zbSelf.Instance.equipHandler.CanEquipSlotUpgrade(EEquipType.ExoSkeleton))
		{
			zbWsMsgHandler.Instance.EquipSlot_LevelUpgradeAll_Req();
		}
		else if (m_needItemData != null)
		{
			zbUIMgr.Instance.ShowTip(m_needItemData.type, m_needItemData.id, m_needItemData.count);
		}
	}
}
