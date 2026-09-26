using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbRoomDetailWnd : zbBaseWnd
{
	private HomelandRoomDetail m_data;

	private List<ConditionCheckResult> m_conditions;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private VisualElement m_costList;

	private VisualElement m_subBg;

	private Label m_level;

	private Label m_name;

	private Label m_lbRoomDesc;

	private Label m_lbUpStatusDesc;

	private Button m_btnTouch;

	public Button m_btnClose;

	public Button m_btnUpgrade;

	private Button m_btnLeft;

	private Button m_btnRight;

	private Button m_delRoom;

	private readonly VisualElement[] m_nodes = (VisualElement[])(object)new VisualElement[2];

	private readonly Label[] m_keys = (Label[])(object)new Label[2];

	private readonly Label[] m_values = (Label[])(object)new Label[2];

	private Label[] m_costLabels = (Label[])(object)new Label[3];

	private zbItem[] m_items = new zbItem[3];

	private zbHomeRoomTypeCsvRecord m_homeRoomTypeRecord;

	private List<VisualElement> m_indicators = new List<VisualElement>();

	private StyleBackground OnBg;

	private StyleBackground OffBg;

	private VisualElement m_indicatorRoot;

	private List<HomelandRoomDetail> m_effectRooms = new List<HomelandRoomDetail>();

	private int m_curIndex;

	protected override void OnEnable()
	{
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>(m_RootElement, "roomType", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "roomIcon", (string)null);
		m_level = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_name = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbRoomDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_roomDesc", (string)null);
		m_btnUpgrade = UQueryExtensions.Q<Button>(m_RootElement, "btn_ok", (string)null);
		m_lbUpStatusDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_upStatusDesc", (string)null);
		m_subBg = UQueryExtensions.Q<VisualElement>(m_RootElement, "sub_bg", (string)null);
		m_btnLeft = UQueryExtensions.Q<Button>(m_RootElement, "btn_left", (string)null);
		m_btnRight = UQueryExtensions.Q<Button>(m_RootElement, "btn_right", (string)null);
		m_indicatorRoot = UQueryExtensions.Q<VisualElement>(m_RootElement, "indicator", (string)null);
		m_delRoom = UQueryExtensions.Q<Button>(m_RootElement, "btn_delRoom", (string)null);
		for (int i = 0; i < m_nodes.Length; i++)
		{
			m_nodes[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "node" + i, (string)null);
			m_keys[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_key", (string)null);
			m_values[i] = UQueryExtensions.Q<Label>(m_nodes[i], "lb_value", (string)null);
		}
		m_costList = UQueryExtensions.Q<VisualElement>(m_RootElement, "costList", (string)null);
		for (int j = 0; j < 3; j++)
		{
			m_costLabels[j] = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost" + j, (string)null);
			m_items[j] = UQueryExtensions.Q<zbItem>(m_RootElement, "item" + j, (string)null);
			m_items[j].OnEnable(this);
			((VisualElement)m_costLabels[j]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		OnBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/guide/guideIndicator_on.png"));
		OffBg = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/main/guide/guideIndicator_off.png"));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClickClose, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnHomelandInfoUpdated);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Item, OnHomelandInfoUpdated);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnHomelandInfoUpdated);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Aux_ExchangeMoney, OnHomelandInfoUpdated);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnUpgrade, (Action<ClickEvent>)OnClickUpgrade, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLeft, (Action)OnLeft, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnRight, (Action)OnRight, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_delRoom, (Action<ClickEvent>)OnClickDelRoom, "", (TrickleDown)0);
		AddDark();
	}

	private void OnHomelandInfoUpdated(object obj)
	{
		if (m_data != null)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(m_data.RoomId);
			if (homelandRoomDetail == null)
			{
				Hide();
				return;
			}
			m_data = homelandRoomDetail;
			RefreshCustom(m_data);
		}
	}

	public void RefreshCustom(HomelandRoomDetail data)
	{
		if (data != null)
		{
			m_data = data;
			RefreshRoomView();
		}
	}

	private void RefreshRoomView()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		if (m_data == null)
		{
			return;
		}
		m_conditions = zbSelf.Instance.homelandHandler.GetRoomUpgradeConditions(m_data);
		zbSelf.Instance.homelandHandler.ShowUpgradeConditions(m_conditions, m_lbUpStatusDesc);
		UpdateUpgradeButtonVisibilityAndText(m_conditions);
		m_homeRoomTypeRecord = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(m_data.TypeId);
		if (m_homeRoomTypeRecord == null)
		{
			return;
		}
		((TextElement)m_name).text = m_homeRoomTypeRecord.name;
		((TextElement)m_lbRoomDesc).text = m_homeRoomTypeRecord.desc;
		((TextElement)m_level).text = "Lv." + m_data.Level;
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(m_homeRoomTypeRecord.iconType));
		m_subBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (m_homeRoomTypeRecord.type == 3)
		{
			if (m_data.Level == 0)
			{
				m_subBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
				ShowNode(show0: false, show1: false);
			}
			else
			{
				ShowNode(show0: true, show1: true);
			}
			RebuildEffectRoomsAndIndex();
			RefreshAttributes();
			RefreshIndicators();
			UpdateIndicator(m_curIndex);
		}
		else if (m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.SpecimenId)
		{
			m_subBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			ShowNode(show0: true, show1: false);
		}
		else if (m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId)
		{
			m_subBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			ShowNode(show0: true, show1: false);
		}
		else if (m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId)
		{
			RefreshCommRadar();
		}
		RefreshCostUI();
		m_spriteColor.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_homeRoomTypeRecord.type != 3));
		((VisualElement)m_btnLeft).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_homeRoomTypeRecord.type != 3));
		((VisualElement)m_btnRight).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_homeRoomTypeRecord.type != 3));
		m_indicatorRoot.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_homeRoomTypeRecord.type != 3));
		((VisualElement)m_delRoom).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)((m_homeRoomTypeRecord.destroy != 1 || m_data.Level < 1 || zbSelf.Instance.homelandHandler.GetFloorState(-2) != EActiveState.Done) ? 1 : 0));
	}

	private void RefreshIndicators()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		m_indicators.Clear();
		m_indicatorRoot.Clear();
		for (int i = 0; i < m_effectRooms.Count; i++)
		{
			VisualElement val = new VisualElement();
			val.style.backgroundImage = OffBg;
			val.style.width = StyleLength.op_Implicit(16f);
			val.style.height = StyleLength.op_Implicit(16f);
			val.style.marginLeft = StyleLength.op_Implicit(5f);
			val.style.marginRight = StyleLength.op_Implicit(5f);
			val.style.position = StyleEnum<Position>.op_Implicit((Position)0);
			m_indicatorRoot.Add(val);
			m_indicators.Add(val);
		}
	}

	private void UpdateIndicator(int activeIndex)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (m_indicators != null && m_indicators.Count != 0)
		{
			for (int i = 0; i < m_indicators.Count; i++)
			{
				m_indicators[i].style.backgroundImage = ((i == activeIndex) ? OnBg : OffBg);
			}
		}
	}

	private void RebuildEffectRoomsAndIndex()
	{
		m_effectRooms.Clear();
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
			if (recordByKey != null && recordByKey.type == 3)
			{
				m_effectRooms.Add(room);
			}
		}
		m_effectRooms.Sort((HomelandRoomDetail a, HomelandRoomDetail b) => (a.Y != b.Y) ? b.Y.CompareTo(a.Y) : a.X.CompareTo(b.X));
		int num = m_effectRooms.FindIndex((HomelandRoomDetail rr) => rr.RoomId == m_data.RoomId);
		if (num >= 0)
		{
			m_curIndex = num;
		}
		else
		{
			m_curIndex = 0;
		}
	}

	private void RefreshCommRadar()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (m_data.Level == 0)
		{
			m_subBg.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			return;
		}
		zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord2 = zbDataConfig.Instance.roomUpgradeUtilityCsvTable.GetRecords().records.FirstOrDefault((zbRoomUpgradeUtilityCsvRecord r) => r.id == m_data.TypeId && r.lv == m_data.Level);
		((TextElement)m_keys[0]).text = zbUtil.Language("lang.10404");
		((TextElement)m_values[0]).text = ((zbRoomUpgradeUtilityCsvRecord2 != null) ? zbRoomUpgradeUtilityCsvRecord2.typeValue[0].ToString() : "0");
		ShowNode(show0: true, show1: false);
	}

	private void RefreshAttributes()
	{
		((TextElement)m_keys[0]).text = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)m_homeRoomTypeRecord.roomAttr[0][0]).name;
		((TextElement)m_keys[1]).text = zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)m_homeRoomTypeRecord.roomAttr[1][0]).name;
		float value = (float)m_data.Level * m_homeRoomTypeRecord.roomAttr[0][1];
		float value2 = (float)m_data.Level * m_homeRoomTypeRecord.roomAttr[1][1];
		((TextElement)m_values[0]).text = zbAttrHelper.FormatAttrValue(value, zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)m_homeRoomTypeRecord.roomAttr[0][0]).showPercent > 0);
		((TextElement)m_values[1]).text = zbAttrHelper.FormatAttrValue(value2, zbDataConfig.Instance.teamAttrCsvTable.GetRecordByKey((int)m_homeRoomTypeRecord.roomAttr[1][0]).showPercent > 0);
	}

	private void UpdateUpgradeButtonVisibilityAndText(List<ConditionCheckResult> conditions)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (conditions.Any((ConditionCheckResult c) => !c.IsMaxLevel) || m_data.Level == 0)
		{
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnUpgrade).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		if (m_data.Level == 0)
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10349");
		}
		else if (m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId)
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10762");
		}
		else
		{
			((TextElement)m_btnUpgrade).text = zbUtil.Language("lang.10350");
		}
	}

	private void OnClickUpgrade(ClickEvent evt)
	{
		ConditionCheckResult conditionCheckResult = m_conditions.FirstOrDefault((ConditionCheckResult c) => !c.IsMet && !c.IsMaxLevel);
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
		else if (m_data.Level == 0)
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomRepair_Req(m_data.RoomId);
			Hide();
		}
		else
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomUpgrade_Req(m_data.RoomId);
		}
	}

	private void OnClickClose(ClickEvent evt)
	{
		Hide();
	}

	private void RefreshCostUI()
	{
		if (m_data == null)
		{
			return;
		}
		if (m_homeRoomTypeRecord.type == 3)
		{
			if (m_data.Level == 0)
			{
				zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records.FirstOrDefault((zbRoomUpgradeEffectCsvRecord r) => r.lv == 1);
				if (zbRoomUpgradeEffectCsvRecord2 != null)
				{
					int[][] costLists = zbSelf.Instance.homelandHandler.AppendExtraCostItem(zbRoomUpgradeEffectCsvRecord2.costList, m_homeRoomTypeRecord.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum);
					zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, m_costLabels, m_items);
				}
				else
				{
					HideAllCostLabels();
				}
			}
			else
			{
				RefreshAttributesCost();
			}
		}
		else if (m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId || m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId || m_data.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.SpecimenId)
		{
			RefreshCommRadarCost();
		}
	}

	private void RefreshAttributesCost()
	{
		zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records.FirstOrDefault((zbRoomUpgradeEffectCsvRecord r) => r.id == m_homeRoomTypeRecord.length && r.lv == m_data.Level + 1);
		if (zbRoomUpgradeEffectCsvRecord2 != null)
		{
			int[][] costLists = zbSelf.Instance.homelandHandler.AppendExtraCostItem(zbRoomUpgradeEffectCsvRecord2.costList, m_homeRoomTypeRecord.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum);
			zbSelf.Instance.homelandHandler.UpdateCostUI(costLists, m_costLabels, m_items);
		}
		else
		{
			HideAllCostLabels();
		}
	}

	private void RefreshCommRadarCost()
	{
		zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord2 = zbDataConfig.Instance.roomUpgradeUtilityCsvTable.GetRecords().records.FirstOrDefault((zbRoomUpgradeUtilityCsvRecord r) => r.id == m_data.TypeId && r.lv == m_data.Level + 1);
		if (zbRoomUpgradeUtilityCsvRecord2 != null)
		{
			zbSelf.Instance.homelandHandler.UpdateCostUI(zbRoomUpgradeUtilityCsvRecord2.costList, m_costLabels, m_items);
		}
		else
		{
			HideAllCostLabels();
		}
	}

	private void HideAllCostLabels()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_costLabels.Length; i++)
		{
			((VisualElement)m_costLabels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	private void ShowNode(bool show0, bool show1)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		m_nodes[0].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!show0));
		m_nodes[1].style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!show1));
	}

	private void OnRight()
	{
		SwitchRoom(1);
	}

	private void OnLeft()
	{
		SwitchRoom(-1);
	}

	private void SwitchRoom(int direction)
	{
		if (m_effectRooms == null || m_effectRooms.Count == 0 || m_data == null)
		{
			return;
		}
		int count = m_effectRooms.Count;
		int num = m_effectRooms.FindIndex((HomelandRoomDetail r) => r.RoomId == m_data.RoomId);
		if (num < 0)
		{
			num = m_curIndex;
		}
		int num2 = (num + direction + count) % count;
		HomelandRoomDetail homelandRoomDetail = m_effectRooms[num2];
		HomelandRoomDetail homelandRoomDetail2 = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(homelandRoomDetail.RoomId);
		if (homelandRoomDetail2 != null)
		{
			m_curIndex = num2;
			RefreshCustom(homelandRoomDetail2);
			UpdateIndicator(m_curIndex);
			return;
		}
		RebuildEffectRoomsAndIndex();
		if (m_effectRooms.Count > 0)
		{
			HomelandRoomDetail homelandRoomDetail3 = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(m_effectRooms[0].RoomId);
			if (homelandRoomDetail3 != null)
			{
				m_curIndex = 0;
				RefreshCustom(homelandRoomDetail3);
				UpdateIndicator(m_curIndex);
			}
		}
		else
		{
			Hide();
		}
	}

	private void OnClickDelRoom(ClickEvent evt)
	{
		string title = zbUtil.Language("lang.10143");
		string message = string.Format(zbUtil.Language("lang.10144"), m_homeRoomTypeRecord.name);
		zbUIMgr.Instance.ShowMessageBox(title, message, OnClickDelRoomConfirm);
	}

	private void OnClickDelRoomConfirm(bool isConfirm)
	{
		if (isConfirm)
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomDestroy_Req(m_data.RoomId);
		}
	}
}
