using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbEquipSuitWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_nodeCur;

	private zbItem[] m_equipCurs = new zbItem[3];

	private Label m_lbSuitCur;

	private Label m_lbKeyCur;

	private Label m_lbValueCur;

	private VisualElement m_nodeNext;

	private zbItem[] m_equipNexts = new zbItem[3];

	private Label m_lbSuitNext;

	private Label m_lbKeyNext;

	private Label m_lbValueNext;

	private Label m_lbMax;

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_nodeCur = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_cur", (string)null);
		m_nodeNext = UQueryExtensions.Q<VisualElement>(m_RootElement, "node_next", (string)null);
		m_lbMax = UQueryExtensions.Q<Label>(m_RootElement, "lb_max", (string)null);
		for (int i = 0; i < 3; i++)
		{
			m_equipCurs[i] = UQueryExtensions.Q<zbItem>(m_nodeCur, $"equip{i}", (string)null);
			m_equipCurs[i]?.OnEnable(this);
			m_equipNexts[i] = UQueryExtensions.Q<zbItem>(m_nodeNext, $"equip{i}", (string)null);
			m_equipNexts[i]?.OnEnable(this);
		}
		m_lbSuitCur = UQueryExtensions.Q<Label>(m_nodeCur, "lb_suit", (string)null);
		m_lbKeyCur = UQueryExtensions.Q<Label>(m_nodeCur, "lb_key", (string)null);
		m_lbValueCur = UQueryExtensions.Q<Label>(m_nodeCur, "lb_value", (string)null);
		m_lbSuitNext = UQueryExtensions.Q<Label>(m_nodeNext, "lb_suit", (string)null);
		m_lbKeyNext = UQueryExtensions.Q<Label>(m_nodeNext, "lb_key", (string)null);
		m_lbValueNext = UQueryExtensions.Q<Label>(m_nodeNext, "lb_value", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public override void Refresh()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbKeyCur).text = zbUtil.Language("language.teamAttr.name.31");
		((TextElement)m_lbKeyNext).text = zbUtil.Language("language.teamAttr.name.31");
		EQuality equipSuitQuality = zbSelf.Instance.equipHandler.GetEquipSuitQuality();
		if (equipSuitQuality >= EQuality.Orange)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeNext.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeNext.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshNextQuality((int)(equipSuitQuality + 1));
		}
		RefreshCurQuality((int)equipSuitQuality);
	}

	private void RefreshCurQuality(int quality)
	{
		for (int i = 0; i < 3; i++)
		{
			zbEquipCsvRecord recordByTypeAndQuality = GetRecordByTypeAndQuality(i + 1, quality);
			if (recordByTypeAndQuality != null)
			{
				m_equipCurs[i].Refresh(new zbItemData(EResourceType.Equip, recordByTypeAndQuality.id, 1L), isNameVis: false, isCountVis: false);
			}
		}
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(quality);
		if (recordByKey != null)
		{
			string text = string.Format(zbUtil.Language("lang.10765"), recordByKey.name);
			string text2 = zbUtil.ConvertToHexColor(recordByKey.color);
			((TextElement)m_lbSuitCur).text = "<color=" + text2 + ">" + text + "</color>";
		}
		zbEquipQualityCsvRecord recordByKey2 = zbDataConfig.Instance.equipQualityCsvTable.GetRecordByKey(quality);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbValueCur).text = recordByKey2.damagePct * 100f + "%";
		}
	}

	private void RefreshNextQuality(int quality)
	{
		for (int i = 0; i < 3; i++)
		{
			zbEquipCsvRecord recordByTypeAndQuality = GetRecordByTypeAndQuality(i + 1, quality);
			if (recordByTypeAndQuality != null)
			{
				m_equipNexts[i].Refresh(new zbItemData(EResourceType.Equip, recordByTypeAndQuality.id, 1L), isNameVis: false, isCountVis: false);
			}
		}
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(quality);
		if (recordByKey != null)
		{
			string text = string.Format(zbUtil.Language("lang.10765"), recordByKey.name);
			string text2 = zbUtil.ConvertToHexColor(recordByKey.color);
			((TextElement)m_lbSuitNext).text = "<color=" + text2 + ">" + text + "</color>";
		}
		zbEquipQualityCsvRecord recordByKey2 = zbDataConfig.Instance.equipQualityCsvTable.GetRecordByKey(quality);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbValueNext).text = recordByKey2.damagePct * 100f + "%";
		}
	}

	private zbEquipCsvRecord GetRecordByTypeAndQuality(int type, int quality)
	{
		foreach (zbEquipCsvRecord value in zbDataConfig.Instance.equipCsvTable.GetRecords().records.Values)
		{
			if (value.type == type && value.quality == quality)
			{
				return value;
			}
		}
		return null;
	}

	private void OnClose()
	{
		Hide();
	}
}
