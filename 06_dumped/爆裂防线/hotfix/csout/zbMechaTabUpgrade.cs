using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMechaTabUpgrade
{
	private zbMechaWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevel;

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbAttrKeys = (Label[])(object)new Label[4];

	private Label[] m_lbAttrValues = (Label[])(object)new Label[4];

	private Button[] m_btnAttrs = (Button[])(object)new Button[4];

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnUpgrade;

	private Button m_btnFastUpgrade;

	private Label m_lbLimit;

	private Label m_lbMax;

	private VisualElement m_notify;

	private VisualElement m_notifyFast;

	private zbMechaUpgradeCsvRecord m_upgradeRecord;

	private int[][] m_upgradeCostList;

	private bool m_isUpgradeReq;

	public void OnEnable(zbMechaWnd wndParent, VisualElement root)
	{
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_nodeAttrs[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + i, (string)null);
			m_lbAttrKeys[i] = UQueryExtensions.Q<Label>(m_nodeAttrs[i], "lb_key", (string)null);
			m_lbAttrValues[i] = UQueryExtensions.Q<Label>(m_nodeAttrs[i], "lb_value", (string)null);
			m_btnAttrs[i] = UQueryExtensions.Q<Button>(m_nodeAttrs[i], "btn_attr", (string)null);
			int curIndex = i;
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttrs[i], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnAttrs(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 2; num++)
		{
			m_costLabels[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + num, (string)null);
			m_items[num] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + num, (string)null);
			m_items[num].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_btnFastUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_fastUpgrade", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFastUpgrade, (Action)OnFastUpgradeCalc, "", (TrickleDown)0);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_notifyFast = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnFastUpgrade, (Vector2?)new Vector2(7f, 3f), 1f, true);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_MechaFastUpgradeLevelCalc_Ack, OnFastUpgradeCalcAck);
	}

	public void Refresh()
	{
		if (m_wndParent.m_mechaDetail != null)
		{
			RefreshAttrs();
			RefreshCostList();
			if (m_isUpgradeReq)
			{
				m_wndParent.ActionChuchangAnim();
				m_isUpgradeReq = false;
			}
		}
	}

	private void RefreshAttrs()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLevel).text = "Lv." + m_wndParent.m_mechaDetail.Level;
		for (int i = 1; i < 4; i++)
		{
			m_nodeAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		((TextElement)m_lbAttrValues[0]).text = zbSelf.Instance.mechaHandler.GetMechaTypeString(m_wndParent.m_record.id);
		int num = 1;
		foreach (BattleMechaAttrDetail attr in m_wndParent.m_mechaDetail.AttrList)
		{
			if (m_nodeAttrs[num] != null)
			{
				m_nodeAttrs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				if (attr.Type == EAttrSkillType.Damage)
				{
					((TextElement)m_lbAttrKeys[num]).text = zbUtil.Language("lang.10797");
					((TextElement)m_lbAttrValues[num]).text = attr.Value.ToString();
				}
				else if (attr.Type == EAttrSkillType.Duration)
				{
					((TextElement)m_lbAttrKeys[num]).text = zbUtil.Language("lang.10796");
					((TextElement)m_lbAttrValues[num]).text = attr.Value + "s";
				}
			}
			num++;
		}
	}

	private void RefreshCostList()
	{
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeRecord = zbDataConfig.Instance.mechaUpgradeCsvTable.GetRecords()?.records.FirstOrDefault((zbMechaUpgradeCsvRecord r) => r.quality == m_wndParent.m_record.quality && r.lv == m_wndParent.m_mechaDetail.Level + 1);
		if (m_upgradeRecord != null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() < m_upgradeRecord.playerLv) ? 1 : 0));
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() >= m_upgradeRecord.playerLv) ? 1 : 0));
			((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10853"), m_upgradeRecord.playerLv);
			m_upgradeCostList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(m_upgradeRecord.costList, m_wndParent.m_record.upgradeCost[1], m_upgradeRecord.costNum, m_wndParent.m_record.upgradeCost[0]);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_upgradeCostList = new int[0][];
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_upgradeCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaUpgradeNotify(m_wndParent.m_record.id)));
		m_notifyFast.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.HasMechaUpgradeNotify(m_wndParent.m_record.id)));
		((VisualElement)m_btnFastUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.mechaHandler.CanUpgradeMultiLv(m_wndParent.m_record.id)));
	}

	private void OnAttrs(ClickEvent evt, int index)
	{
	}

	private void OnUpgrade()
	{
		if (zbSelf.Instance.funHandler.IsLockAndTip(EFuncType.Mecha) || m_wndParent.m_mechaDetail == null)
		{
			return;
		}
		int[][] upgradeCostList = m_upgradeCostList;
		foreach (int[] array in upgradeCostList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.Mecha_UpgradeLevel_Req(m_wndParent.m_record.id);
		m_isUpgradeReq = true;
	}

	private void OnFastUpgradeCalc()
	{
		if (m_wndParent.m_mechaDetail == null)
		{
			return;
		}
		int[][] upgradeCostList = m_upgradeCostList;
		foreach (int[] array in upgradeCostList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.Mecha_FastUpgradeLevelCalc_Req(m_wndParent.m_record.id);
	}

	private void OnFastUpgradeCalcAck(object data)
	{
		if (!(data is MechaFastUpgradeLevelCalcAck mechaFastUpgradeLevelCalcAck))
		{
			return;
		}
		string text = "是否消耗";
		foreach (AwardDetail award in mechaFastUpgradeLevelCalcAck.AwardList)
		{
			stResorceIconInfo resorceIconPath = zbResourceHelper.GetResorceIconPath(award.Type, award.Tid);
			text = text + resorceIconPath.strName + "x" + award.Count + ",";
		}
		text = text + "将机甲升到" + mechaFastUpgradeLevelCalcAck.ToLevel + "级?";
		zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10947"), text, delegate(bool isOK)
		{
			if (isOK)
			{
				zbWsMsgHandler.Instance.Mecha_FastUpgradeLevel_Req(m_wndParent.m_record.id);
				m_isUpgradeReq = true;
			}
		}, zbUtil.Language("lang.10206"), zbUtil.Language("lang.10287"));
	}
}
