using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroTabUpgrade
{
	private zbHeroWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevel;

	private VisualElement[] m_nodeAttrs = (VisualElement[])(object)new VisualElement[4];

	private Label[] m_lbAttrKeys = (Label[])(object)new Label[4];

	private Label[] m_lbAttrValues = (Label[])(object)new Label[4];

	private Button[] m_btnAttrs = (Button[])(object)new Button[4];

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private Button m_btnUpgrade;

	private Label m_lbLimit;

	private Label m_lbMax;

	private VisualElement m_notify;

	private int[][] m_upgradeCostList;

	private bool m_isUpgradeReq;

	public void OnEnable(zbHeroWnd wndParent, VisualElement root)
	{
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
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
			m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAttrs[curIndex], (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnAttrTip(evt, curIndex);
			}, "", (TrickleDown)0);
		}
		for (int num = 0; num < 3; num++)
		{
			m_costLabels[num] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + num, (string)null);
			m_items[num] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + num, (string)null);
			m_items[num].OnEnable(m_wndParent);
			((VisualElement)m_costLabels[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_upgrade", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_notify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-3f, -5f), 1f, true);
	}

	public void Refresh()
	{
		if (m_wndParent.m_warRoomDetail != null && m_wndParent.m_roomDetail != null)
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
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbLevel).text = "Lv." + m_wndParent.m_roomDetail.Level;
		for (int i = 0; i < 4; i++)
		{
			m_nodeAttrs[i].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		HeroAwakenDetail heroAwakenDetail = zbSelf.Instance.heroHandler.GetHeroAwakenDetail(m_wndParent.m_warRoomDetail.SkillType);
		int num = 0;
		foreach (WarRoomAttrDetail item in ((IEnumerable<WarRoomAttrDetail>)m_wndParent.m_warRoomDetail.AttrList).OrderBy((WarRoomAttrDetail a) => a.Type).ToList())
		{
			if (m_nodeAttrs[num] != null)
			{
				m_nodeAttrs[num].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				zbSkillAttrCsvRecord recordByKey = zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey((int)item.Type);
				((TextElement)m_lbAttrKeys[num]).text = recordByKey.name ?? zbUtil.Language("lang.10119");
				string text = ((item.Type == EAttrSkillType.Damage) ? ((int)item.Value).ToString() : zbAttrHelper.FormatAttrValue(item.Value, recordByKey.showPercent > 0));
				if (heroAwakenDetail != null && heroAwakenDetail.AttrDetail.Type == item.Type && heroAwakenDetail.AttrDetail.Value > 0f)
				{
					text += $"(<color=#2DF421>+{heroAwakenDetail.AttrDetail.Value}</color>)";
				}
				((TextElement)m_lbAttrValues[num]).text = text;
			}
			num++;
		}
	}

	private void RefreshCostList()
	{
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		int num = ((zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecordByKey(m_wndParent.m_roomDetail.Level + 1) != null) ? (m_wndParent.m_roomDetail.Level + 1) : m_wndParent.m_roomDetail.Level);
		zbRoomUpgradeWarCsvRecord recordByKey = zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecordByKey(num);
		bool num2 = zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecords().records.Count == m_wndParent.m_roomDetail.Level;
		if (num > m_wndParent.m_roomDetail.Level)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbHomeRoomTypeCsvRecord recordByKey2 = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(m_wndParent.m_roomDetail.TypeId);
			m_upgradeCostList = zbSelf.Instance.homelandHandler.AppendExtraCostItem(recordByKey.costList, recordByKey2.itemId, recordByKey.costItemNum);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() < recordByKey.playerLv) ? 1 : 0));
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((zbSelf.Instance.GetLevel() >= recordByKey.playerLv) ? 1 : 0));
			((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10816"), recordByKey.playerLv);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_upgradeCostList = new int[0][];
		}
		if (num2)
		{
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		zbSelf.Instance.homelandHandler.UpdateCostUI(m_upgradeCostList, m_costLabels, m_items);
		m_notify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroUpgradeNotify(m_wndParent.m_record.id)));
	}

	private void OnUpgrade()
	{
		if (m_wndParent.m_roomDetail == null)
		{
			return;
		}
		ConditionCheckResult conditionCheckResult = zbSelf.Instance.homelandHandler.GetRoomUpgradeConditions(m_wndParent.m_roomDetail).FirstOrDefault((ConditionCheckResult c) => !c.IsMet && !c.IsMaxLevel);
		if (conditionCheckResult != null)
		{
			if (conditionCheckResult.NeedItem != null)
			{
				zbUIMgr.Instance.ShowTip(conditionCheckResult.NeedItem.type, conditionCheckResult.NeedItem.id, conditionCheckResult.NeedItem.count);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(conditionCheckResult.Description);
			}
		}
		else
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomUpgrade_Req(m_wndParent.m_roomDetail.RoomId);
			m_isUpgradeReq = true;
		}
	}

	private void OnAttrTip(ClickEvent evt, int index)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		if (((EventBase)evt).target is VisualElement)
		{
			float x = ((TextElement)m_lbAttrValues[index]).MeasureTextSize(((TextElement)m_lbAttrValues[index]).text, 0f, (MeasureMode)0, 0f, (MeasureMode)0).x;
			Rect worldBound = ((VisualElement)m_lbAttrValues[index]).worldBound;
			float num = ((Rect)(ref worldBound)).xMax - x;
			worldBound = ((VisualElement)m_lbAttrValues[index]).worldBound;
			Vector2 pos = default(Vector2);
			((Vector2)(ref pos))._002Ector(num, ((Rect)(ref worldBound)).center.y);
			List<WarRoomAttrDetail> list = ((IEnumerable<WarRoomAttrDetail>)m_wndParent.m_warRoomDetail.AttrList).OrderBy((WarRoomAttrDetail a) => a.Type).ToList();
			zbUIMgr.Instance.ShowTipAttr(m_wndParent.m_warRoomDetail.SkillType, list[index], pos);
		}
	}
}
