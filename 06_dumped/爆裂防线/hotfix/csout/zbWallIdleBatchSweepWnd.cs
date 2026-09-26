using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbWallIdleBatchSweepWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Label m_lbNum;

	private int m_numSel = 1;

	private int m_numMin = 1;

	private int m_numMax = 100;

	private int m_numTen = 10;

	private VisualElement[] m_awards = (VisualElement[])(object)new VisualElement[2];

	private zbItem[] m_items = new zbItem[2];

	private Label[] m_lbCounts = (Label[])(object)new Label[2];

	private Label m_lbTiliCost;

	private Button m_btnSweep;

	private zbIdleAwardCsvRecord m_idleAwardRecord;

	private int m_costTili;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		for (int i = 0; i < 2; i++)
		{
			m_awards[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "award" + i, (string)null);
			m_items[i] = UQueryExtensions.Q<zbItem>(m_awards[i], "item", (string)null);
			m_items[i].OnEnable(this);
			m_lbCounts[i] = UQueryExtensions.Q<Label>(m_awards[i], "lb_count", (string)null);
		}
		m_lbTiliCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_btnSweep = UQueryExtensions.Q<Button>(m_RootElement, "btn_sweep", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSweep, (Action<ClickEvent>)OnSweep, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Money, OnEventUpdate);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_IdleInfo, OnEventUpdate);
		InitCostTili();
		AddDark();
		CreateMoneyPanel(new EMoneyType[2]
		{
			EMoneyType.Diamond,
			EMoneyType.Tili
		});
	}

	public override void Refresh()
	{
		m_idleAwardRecord = zbDataConfig.Instance.idleAwardCsvTable.GetRecordByKey(zbServerData.sGameData.MainQuestInfo.MainLevel);
		if (m_idleAwardRecord != null)
		{
			m_items[0].Refresh(new zbItemData(EResourceType.Money, 1L, m_idleAwardRecord.gold), isNameVis: false, isCountVis: false, isTouchVis: false);
			m_items[1].Refresh(new zbItemData(EResourceType.Box, 1001001L, m_idleAwardRecord.randCount), isNameVis: false, isCountVis: false, isTouchVis: false);
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.Money, 4);
		int num = (int)((m_costTili > 0) ? (resourceCount / m_costTili) : 0);
		if (num < 1)
		{
			num = 1;
		}
		m_numMax = Math.Min(100, num);
		if (m_numSel > m_numMax)
		{
			m_numSel = m_numMax;
		}
		if (m_numSel < m_numMin)
		{
			m_numSel = m_numMin;
		}
		RefreshNum();
	}

	private void RefreshNum()
	{
		if (m_idleAwardRecord == null)
		{
			m_idleAwardRecord = zbDataConfig.Instance.idleAwardCsvTable.GetRecordByKey(zbServerData.sGameData.MainQuestInfo.MainLevel);
		}
		if (m_idleAwardRecord != null)
		{
			((TextElement)m_lbCounts[0]).text = (m_idleAwardRecord.gold * m_numSel).ToString() ?? "";
			((TextElement)m_lbCounts[1]).text = (m_idleAwardRecord.randCount * m_numSel).ToString() ?? "";
			((TextElement)m_lbNum).text = m_numSel.ToString();
			((TextElement)m_lbTiliCost).text = "x" + m_numSel * m_costTili;
		}
	}

	private void OnMin()
	{
		int num = m_numSel - m_numTen;
		if (num < m_numMin)
		{
			m_numSel = m_numMin;
		}
		else
		{
			m_numSel = num;
		}
		RefreshNum();
	}

	private void OnSubtract()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel--;
			RefreshNum();
		}
	}

	private void OnAdd()
	{
		if (m_numSel < m_numMax)
		{
			m_numSel++;
			RefreshNum();
		}
	}

	private void OnMax()
	{
		int num = m_numSel + m_numTen;
		if (num > m_numMax)
		{
			m_numSel = m_numMax;
		}
		else
		{
			m_numSel = num;
		}
		RefreshNum();
	}

	private void OnSweep(ClickEvent evt)
	{
		int num = m_numSel * m_costTili;
		if (!zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, num))
		{
			zbUIMgr.Instance.ShowTip(EResourceType.Money, 4L, num);
			return;
		}
		zbWsMsgHandler.Instance.Idle_FastGain_Req(m_numSel);
		OnClose();
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		Refresh();
	}

	private void InitCostTili()
	{
		JObject record = zbDataConfig.Instance.idleBaseJsonTable.GetRecord();
		JToken val = ((record != null) ? record["fastGain"] : null);
		if (val != null)
		{
			m_costTili = Extensions.Value<int>((IEnumerable<JToken>)val[(object)"costTili"]);
		}
	}
}
