using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbGemSuitWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private VisualElement m_nodeCur;

	private zbItem[] m_gemCurs = new zbItem[6];

	private Label m_lbNone;

	private Label m_lbSuitCur;

	private Label m_lbKeyCur;

	private Label m_lbValueCur;

	private VisualElement m_nodeNext;

	private zbItem[] m_gemNexts = new zbItem[6];

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
		for (int i = 0; i < 6; i++)
		{
			m_gemCurs[i] = UQueryExtensions.Q<zbItem>(m_nodeCur, $"gem{i}", (string)null);
			m_gemCurs[i]?.OnEnable(this);
			m_gemNexts[i] = UQueryExtensions.Q<zbItem>(m_nodeNext, $"gem{i}", (string)null);
			m_gemNexts[i]?.OnEnable(this);
		}
		m_lbNone = UQueryExtensions.Q<Label>(m_nodeCur, "lb_none", (string)null);
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
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbKeyCur).text = zbUtil.Language("language.teamAttr.name.2");
		((TextElement)m_lbKeyNext).text = zbUtil.Language("language.teamAttr.name.2");
		if (!zbSelf.Instance.equipHandler.HasGemSuit())
		{
			((VisualElement)m_lbNone).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbSuitCur).text = "";
			((TextElement)m_lbValueCur).text = "0%";
			for (int i = 0; i < 6; i++)
			{
				((VisualElement)m_gemCurs[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			}
			RefreshNextQuality(1);
			return;
		}
		((VisualElement)m_lbNone).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		EQuality gemSuitQuality = zbSelf.Instance.equipHandler.GetGemSuitQuality();
		if (gemSuitQuality >= EQuality.Orange)
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_nodeNext.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_lbMax).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_nodeNext.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			RefreshNextQuality((int)(gemSuitQuality + 1));
		}
		RefreshCurQuality((int)gemSuitQuality);
	}

	private void RefreshCurQuality(int quality)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		zbBoxCsvRecord boxRecord = GetBoxRecordByQuality(quality);
		for (int i = 0; i < 6; i++)
		{
			zbGemCsvRecord recordByTypeAndQuality = GetRecordByTypeAndQuality(i + 1, quality);
			if (recordByTypeAndQuality != null)
			{
				((VisualElement)m_gemCurs[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
				m_gemCurs[i].Refresh(new zbItemData(EResourceType.Gem, recordByTypeAndQuality.id, 1L), isNameVis: false, isCountVis: false);
				m_gemCurs[i].SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip(EResourceType.Box, boxRecord.id, 0L);
				});
			}
		}
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(quality);
		if (recordByKey != null)
		{
			string text = string.Format(zbUtil.Language("lang.10765"), recordByKey.name);
			string text2 = zbUtil.ConvertToHexColor(recordByKey.color);
			((TextElement)m_lbSuitCur).text = "<color=" + text2 + ">" + text + "</color>";
		}
		zbGemQualityCsvRecord recordByKey2 = zbDataConfig.Instance.gemQualityCsvTable.GetRecordByKey(quality);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbValueCur).text = recordByKey2.critDamage * 100f + "%";
		}
	}

	private void RefreshNextQuality(int quality)
	{
		zbBoxCsvRecord boxRecord = GetBoxRecordByQuality(quality);
		for (int i = 0; i < 6; i++)
		{
			zbGemCsvRecord recordByTypeAndQuality = GetRecordByTypeAndQuality(i + 1, quality);
			if (recordByTypeAndQuality != null)
			{
				m_gemNexts[i].Refresh(new zbItemData(EResourceType.Gem, recordByTypeAndQuality.id, 1L), isNameVis: false, isCountVis: false);
				m_gemNexts[i].SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip(EResourceType.Box, boxRecord.id, 0L);
				});
			}
		}
		zbQualityCsvRecord recordByKey = zbDataConfig.Instance.qualityCsvTable.GetRecordByKey(quality);
		if (recordByKey != null)
		{
			string text = string.Format(zbUtil.Language("lang.10765"), recordByKey.name);
			string text2 = zbUtil.ConvertToHexColor(recordByKey.color);
			((TextElement)m_lbSuitNext).text = "<color=" + text2 + ">" + text + "</color>";
		}
		zbGemQualityCsvRecord recordByKey2 = zbDataConfig.Instance.gemQualityCsvTable.GetRecordByKey(quality);
		if (recordByKey2 != null)
		{
			((TextElement)m_lbValueNext).text = recordByKey2.critDamage * 100f + "%";
		}
	}

	private zbGemCsvRecord GetRecordByTypeAndQuality(int type, int quality)
	{
		foreach (zbGemCsvRecord value in zbDataConfig.Instance.gemCsvTable.GetRecords().records.Values)
		{
			if (value.type == type && value.quality == quality)
			{
				return value;
			}
		}
		return null;
	}

	private zbBoxCsvRecord GetBoxRecordByQuality(int quality)
	{
		foreach (zbBoxCsvRecord value in zbDataConfig.Instance.boxCsvTable.GetRecords().records.Values)
		{
			if (value.type == 5 && value.boxShow == 0 && value.quality == quality)
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
