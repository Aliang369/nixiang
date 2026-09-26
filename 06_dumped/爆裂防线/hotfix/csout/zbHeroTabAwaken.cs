using System;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroTabAwaken
{
	private zbHeroWnd m_wndParent;

	private VisualElement m_RootElement;

	private Label m_lbLevel;

	private Label m_lbAttr;

	private VisualElement m_progress;

	private Label m_lbProgress;

	private VisualElement m_nodeUpgrade;

	private Label m_costUpgrade;

	private zbItem m_itemUpgrade = new zbItem();

	public Button m_btnUpgrade;

	private Label m_costUpgradeFast;

	private zbItem m_itemUpgradeFast = new zbItem();

	private Button m_btnUpgradeFast;

	private VisualElement m_nodeAdvance;

	private Label m_costAdvance;

	private zbItem m_itemAdvance = new zbItem();

	private Button m_btnAdvance;

	private Label m_lbLimit;

	private Label m_lbMax;

	private VisualElement m_notifyUpgrade;

	private VisualElement m_notifyUpgradeFast;

	private VisualElement m_notifyAdvance;

	private HeroAwakenDetail m_awakenDetail;

	private zbHeroAwakenCsvRecord m_nextRecord;

	private int m_costUpgradeId = zbDataConfig.Instance.itemCsvTable.GetRecords().records.Values.FirstOrDefault((zbItemCsvRecord r) => r.type == 8).id;

	private int m_costAdvanceId = zbDataConfig.Instance.itemCsvTable.GetRecords().records.Values.FirstOrDefault((zbItemCsvRecord r) => r.type == 9).id;

	private int m_costUpgradeFastCount;

	private bool m_isUpgradeReq;

	public void OnEnable(zbHeroWnd wndParent, VisualElement root)
	{
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_lbLevel = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbProgress = UQueryExtensions.Q<Label>(m_RootElement, "lb_progress", (string)null);
		m_nodeUpgrade = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_upgrade", (string)null);
		m_costUpgrade = UQueryExtensions.Q<Label>(m_nodeUpgrade, "lb_cost", (string)null);
		m_itemUpgrade = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_costUpgrade, "item", (string)null);
		m_itemUpgrade.OnEnable(m_wndParent);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_nodeUpgrade, "btn_upgrade", (string)null);
		m_costUpgradeFast = UQueryExtensions.Q<Label>(m_nodeUpgrade, "lb_costFast", (string)null);
		m_itemUpgradeFast = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_costUpgradeFast, "item", (string)null);
		m_itemUpgradeFast.OnEnable(m_wndParent);
		m_btnUpgradeFast = UQueryExtensions.Q<Button>(m_nodeUpgrade, "btn_upgradeFast", (string)null);
		m_nodeAdvance = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_advance", (string)null);
		m_costAdvance = UQueryExtensions.Q<Label>(m_nodeAdvance, "lb_cost", (string)null);
		m_itemAdvance = UQueryExtensions.Q<zbItem>((VisualElement)(object)m_costAdvance, "item", (string)null);
		m_itemAdvance.OnEnable(m_wndParent);
		m_btnAdvance = UQueryExtensions.Q<Button>(m_nodeAdvance, "btn_advance", (string)null);
		m_lbLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_limit", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action)OnUpgrade, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgradeFast, (Action)OnUpgradeFast, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdvance, (Action)OnAdvance, "", (TrickleDown)0);
		m_notifyUpgrade = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgrade, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_notifyUpgradeFast = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnUpgradeFast, (Vector2?)new Vector2(-3f, -5f), 1f, true);
		m_notifyAdvance = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnAdvance, (Vector2?)new Vector2(-3f, -5f), 1f, true);
	}

	public void Refresh()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_nodeAdvance.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		m_awakenDetail = zbSelf.Instance.heroHandler.GetHeroAwakenDetail(m_wndParent.m_warRoomDetail.SkillType);
		if (m_awakenDetail == null)
		{
			m_nextRecord = zbSelf.Instance.heroHandler.GetHeroAwakenRecord(m_wndParent.m_warRoomDetail.SkillType, 1);
		}
		else
		{
			m_nextRecord = zbSelf.Instance.heroHandler.GetHeroAwakenRecord(m_wndParent.m_warRoomDetail.SkillType, m_awakenDetail.Level + 1);
		}
		if (m_awakenDetail == null || (m_awakenDetail != null && m_awakenDetail.AttrDetail.Value < m_awakenDetail.AttrDetail.CurrentMax))
		{
			m_nodeUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshUpgrade();
		}
		else if (m_nextRecord == null)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else if (m_wndParent.m_roomDetail.Level < m_nextRecord.warRoomLevel)
		{
			((VisualElement)m_lbLimit).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)m_lbLimit).text = string.Format(zbUtil.Language("lang.10757"), m_nextRecord.warRoomLevel);
		}
		else
		{
			m_nodeAdvance.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshAdvance();
		}
		RefreshDetail();
		m_notifyUpgrade.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_wndParent.m_heroDetail.HeroTid)));
		if (m_nextRecord != null)
		{
			m_notifyUpgradeFast.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_costUpgradeId, m_costUpgradeFastCount * m_nextRecord.upgradeCost)));
		}
		else
		{
			m_notifyUpgradeFast.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		m_notifyAdvance.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.heroHandler.HasHeroAwakenNotify(m_wndParent.m_heroDetail.HeroTid)));
		if (m_isUpgradeReq)
		{
			m_wndParent.ActionChuchangAnim();
			m_isUpgradeReq = false;
		}
	}

	private void RefreshDetail()
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = 100f;
		if (m_awakenDetail != null)
		{
			((TextElement)m_lbLevel).text = zbUtil.Language("lang.10758") + m_awakenDetail.Level;
			foreach (WarRoomAttrDetail attr in m_wndParent.m_warRoomDetail.AttrList)
			{
				if (m_awakenDetail.AttrDetail.Type == attr.Type)
				{
					((TextElement)m_lbAttr).text = zbDataConfig.Instance.skillAttrCsvTable.GetRecordByKey((int)m_awakenDetail.AttrDetail.Type).name + $": <color=#2DF421>{attr.Value}</color>";
					break;
				}
			}
			num = m_awakenDetail.AttrDetail.Value;
			num2 = m_awakenDetail.AttrDetail.CurrentMax;
		}
		float num3 = num / num2;
		m_progress.style.width = StyleLength.op_Implicit(new Length(num3 * 490f, (LengthUnit)0));
		if (m_nextRecord == null)
		{
			((TextElement)m_lbProgress).text = "Max";
		}
		else if (m_nodeAdvance.style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0) || ((VisualElement)m_lbLimit).style.display == StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			((TextElement)m_lbProgress).text = "Max " + $"<color=#2DF421>+{m_awakenDetail.AttrDetail.NextMax}</color>";
		}
		else
		{
			((TextElement)m_lbProgress).text = "+" + num + "/" + num2;
		}
	}

	private void RefreshUpgrade()
	{
		if (m_nextRecord == null)
		{
			return;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, m_costUpgradeId);
		m_itemUpgrade.Refresh(new zbItemData(EResourceType.Item, m_costUpgradeId, m_nextRecord.upgradeCost), isNameVis: false, isCountVis: false);
		((TextElement)m_costUpgrade).text = zbUtil.GetNeedHaveRichString(resourceCount, m_nextRecord.upgradeCost);
		if (m_nextRecord.upgradeCost > resourceCount)
		{
			m_itemUpgrade.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, m_nextRecord.upgradeCost);
			});
		}
		else
		{
			m_itemUpgrade.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, 0L);
			});
		}
		m_itemUpgradeFast.Refresh(new zbItemData(EResourceType.Item, m_costUpgradeId, 1L), isNameVis: false, isCountVis: false);
		m_costUpgradeFastCount = (int)Math.Ceiling((m_awakenDetail.AttrDetail.CurrentMax - m_awakenDetail.AttrDetail.Value) / m_awakenDetail.AttrDetail.Growth);
		((TextElement)m_costUpgradeFast).text = zbUtil.GetNeedHaveRichString(resourceCount, m_costUpgradeFastCount * m_nextRecord.upgradeCost);
		if (m_costUpgradeFastCount * m_nextRecord.upgradeCost > resourceCount)
		{
			m_itemUpgradeFast.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, m_costUpgradeFastCount * m_nextRecord.upgradeCost);
			});
		}
		else
		{
			m_itemUpgradeFast.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, 0L);
			});
		}
	}

	private void RefreshAdvance()
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, m_costAdvanceId);
		m_itemAdvance.Refresh(new zbItemData(EResourceType.Item, m_costAdvanceId, m_nextRecord.advanceCost), isNameVis: false, isCountVis: false);
		((TextElement)m_costAdvance).text = zbUtil.GetNeedHaveRichString(resourceCount, m_nextRecord.advanceCost);
		if (m_nextRecord.advanceCost > resourceCount)
		{
			m_itemAdvance.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costAdvanceId, m_nextRecord.advanceCost);
			});
		}
		else
		{
			m_itemAdvance.SetTouchCallback(delegate
			{
				zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costAdvanceId, 0L);
			});
		}
	}

	private void OnUpgrade()
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_costUpgradeId, m_nextRecord.upgradeCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, m_nextRecord.upgradeCost);
			return;
		}
		zbWsMsgHandler.Instance.HeroAwaken_Upgrade_Req(m_wndParent.m_warRoomDetail.SkillType, 1, EBoolState.False);
		m_isUpgradeReq = true;
	}

	private void OnUpgradeFast()
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_costUpgradeId, m_nextRecord.upgradeCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costUpgradeId, m_costUpgradeFastCount * m_nextRecord.upgradeCost);
			return;
		}
		int count = Math.Min((int)(zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Item, m_costUpgradeId) / m_nextRecord.upgradeCost), m_costUpgradeFastCount);
		zbWsMsgHandler.Instance.HeroAwaken_Upgrade_Req(m_wndParent.m_warRoomDetail.SkillType, count, EBoolState.False);
		m_isUpgradeReq = true;
	}

	private void OnAdvance()
	{
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, m_costAdvanceId, m_nextRecord.advanceCost))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Item, m_costAdvanceId, m_nextRecord.advanceCost);
			return;
		}
		zbWsMsgHandler.Instance.HeroAwaken_Advance_Req(m_wndParent.m_warRoomDetail.SkillType);
		m_isUpgradeReq = true;
	}
}
