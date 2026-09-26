using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallTabUpgrade
{
	private zbWallWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevel;

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbAttrKeys = (Label[])(object)new Label[4];

	private Label[] m_lbAttrValues = (Label[])(object)new Label[4];

	private Label[] m_costLabels = (Label[])(object)new Label[2];

	private zbItem[] m_items = new zbItem[2];

	private Button m_btnUpgrade;

	private Label m_lbLimit;

	private Label m_lbMax;

	private VisualElement m_notify;

	private zbWallUpgradeCsvRecord m_upgradeRecord;

	public void OnEnable(zbWallWnd wndParent, VisualElement root)
	{
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		for (int i = 0; i < 4; i++)
		{
			m_nodeAttrs[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "attr" + i, (string)null);
			m_lbAttrKeys[i] = UQueryExtensions.Q<Label>(m_nodeAttrs[i], "lb_key", (string)null);
			m_lbAttrValues[i] = UQueryExtensions.Q<Label>(m_nodeAttrs[i], "lb_value", (string)null);
		}
		for (int j = 0; j < 2; j++)
		{
			m_costLabels[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
			m_items[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_items[j].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-3f, -5f), 1f, true);
	}

	public void Refresh()
	{
		m_wndParent.RefreshDetail(zbServerData.sGameData.WallInfo.WallId);
		RefreshAttrs();
		RefreshCostList();
	}

	private void RefreshAttrs()
	{
		((TextElement)m_lbLevel).text = "Lv." + zbServerData.sGameData.WallInfo.Level;
		EAttrDefenseType[] array = new EAttrDefenseType[4]
		{
			EAttrDefenseType.DefenseHp,
			EAttrDefenseType.ShieldHp,
			EAttrDefenseType.DamageDecr,
			EAttrDefenseType.DamageImmunity
		};
		for (int i = 0; i < array.Length; i++)
		{
			((TextElement)m_lbAttrKeys[i]).text = GetAttrName(array[i]);
			((TextElement)m_lbAttrValues[i]).text = Math.Floor(GetAttr(array[i])).ToString();
		}
		static float GetAttr(EAttrDefenseType type)
		{
			return ((IEnumerable<BattleDefenseAttrDetail>)zbServerData.sGameData.DefenseInfo.DefenseList[0].AttrList).FirstOrDefault((BattleDefenseAttrDetail a) => a.Type == type)?.Value ?? 0f;
		}
		static string GetAttrName(EAttrDefenseType type)
		{
			string result = "";
			zbTeamAttrCsvRecord recordByKey = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)type);
			if (recordByKey != null)
			{
				result = zbUtil.Language(recordByKey.name);
			}
			return result;
		}
	}

	private void RefreshCostList()
	{
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		m_upgradeRecord = zbDataConfig.Instance.wallUpgradeCsvTable.GetRecordByKey(zbServerData.sGameData.WallInfo.Level + 1);
		if (m_upgradeRecord != null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() < m_upgradeRecord.playerLv) ? 1 : 0));
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() >= m_upgradeRecord.playerLv) ? 1 : 0));
			((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10816"), m_upgradeRecord.playerLv);
			zbSelf.Instance.homelandHandler.UpdateCostUI(m_upgradeRecord.costList, m_costLabels, m_items);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_costLabels[0]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_costLabels[1]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.wallHandler.HasUpgradeNotify()));
	}

	private void OnUpgrade()
	{
		if (m_upgradeRecord == null)
		{
			return;
		}
		if (zbSelf.Instance.GetLevel() < m_upgradeRecord.playerLv)
		{
			zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10816"), m_upgradeRecord.playerLv));
			return;
		}
		int[][] costList = m_upgradeRecord.costList;
		foreach (int[] array in costList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
			{
				zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
				return;
			}
		}
		zbWsMsgHandler.Instance.Wall_UpgradeLevel_Req();
	}
}
