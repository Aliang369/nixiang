using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbBoxComposeWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbItem m_item;

	private Label m_lbHave;

	private Label m_lbNum;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Label m_lbBatchLimit;

	private Button m_btnCompose;

	private Button m_btnClose;

	private long m_numSel = 1L;

	private long m_numMin = 1L;

	private long m_numMax = 100L;

	private long m_numStep = 10L;

	private zbItemData m_data;

	private zbBoxCsvRecord m_record;

	protected override void Awake()
	{
		base.Awake();
		m_item = new zbItem();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_item = UQueryExtensions.Q<zbItem>(m_RootElement, "item", (string)null);
		m_item.OnEnable(this);
		m_lbHave = UQueryExtensions.Q<Label>(m_RootElement, "lb_have", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_lbBatchLimit = UQueryExtensions.Q<Label>(m_RootElement, "lb_batchLimit", (string)null);
		m_btnCompose = UQueryExtensions.Q<Button>(m_RootElement, "btn_compose", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCompose, (Action)OnCompose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_DataChange_Box, OnEventUpdate);
	}

	public void RefreshCustom(zbItemData data)
	{
		m_data = data;
		m_numSel = 1L;
		m_record = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)m_data.id);
		if (m_record != null)
		{
			((TextElement)m_lbHave).text = m_data.count + "/" + m_record.boxCost;
			m_numMax = Mathf.FloorToInt((float)m_data.count / (float)m_record.boxCost);
			m_numMax = Math.Min(m_numMax, m_record.batchLimit);
			m_numSel = m_numMax;
			m_numStep = m_record.batchStep;
			((TextElement)m_lbBatchLimit).text = string.Format(zbUtil.Language("lang.11036"), m_record.batchLimit);
		}
		else
		{
			((TextElement)m_lbBatchLimit).text = "";
		}
		m_item.Refresh(m_data, isNameVis: true);
		RefreshNum();
	}

	private void RefreshNum()
	{
		((TextElement)m_lbNum).text = m_numSel.ToString();
	}

	private void OnMin()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel = (long)Mathf.Max((float)m_numMin, (float)(m_numSel - m_numStep));
			if (m_numSel > m_numStep && m_numSel % m_numStep != 0L)
			{
				m_numSel = Mathf.CeilToInt((float)(m_numSel / m_numStep)) * m_numStep;
			}
			RefreshNum();
		}
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
		if (m_numSel < m_numMax)
		{
			m_numSel = (long)Mathf.Min((float)m_numMax, (float)(m_numSel + m_numStep));
			if (m_numSel > m_numStep && m_numSel % m_numStep != 0L && m_numSel / m_numStep != m_numMax / m_numStep)
			{
				m_numSel = m_numSel / m_numStep * m_numStep;
			}
			RefreshNum();
		}
	}

	private void OnCompose()
	{
		zbWsMsgHandler.Instance.Bag_ComposeBoxOpen_Req((int)m_data.id, (int)m_numSel);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnEventUpdate(object data)
	{
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(m_data.type, (int)m_data.id);
		if (resourceCount > m_record.boxCost)
		{
			zbItemData data2 = new zbItemData(m_data.type, m_data.id, resourceCount);
			RefreshCustom(data2);
		}
		else
		{
			Hide();
		}
	}
}
