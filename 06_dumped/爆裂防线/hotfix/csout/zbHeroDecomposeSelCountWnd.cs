using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHeroDecomposeSelCountWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private zbItem m_item;

	private VisualElement m_spriteMoney;

	private Label m_lbPrice;

	private Label m_lbNum;

	private Button m_btnMin;

	private Button m_btnSubtract;

	private Button m_btnAdd;

	private Button m_btnMax;

	private Button m_btnConfirm;

	private Button m_btnClose;

	private long m_numSel = 1L;

	private long m_numMin = 1L;

	private long m_numMax = 100L;

	private ResourceDetail m_data;

	private Action<ResourceDetail, long> m_callback;

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
		m_spriteMoney = UQueryExtensions.Q<VisualElement>(m_RootElement, "sprite_money", (string)null);
		m_lbPrice = UQueryExtensions.Q<Label>(m_RootElement, "lb_price", (string)null);
		m_lbNum = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnMin = UQueryExtensions.Q<Button>(m_RootElement, "btn_min", (string)null);
		m_btnSubtract = UQueryExtensions.Q<Button>(m_RootElement, "btn_subtract", (string)null);
		m_btnAdd = UQueryExtensions.Q<Button>(m_RootElement, "btn_add", (string)null);
		m_btnMax = UQueryExtensions.Q<Button>(m_RootElement, "btn_max", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMin, (Action)OnMin, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSubtract, (Action)OnSubtract, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAdd, (Action)OnAdd, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnMax, (Action)OnMax, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(ResourceDetail data, Action<ResourceDetail, long> callback)
	{
		m_data = data;
		m_callback = callback;
		m_numSel = 1L;
		m_numMax = m_data.Count;
		m_item.Refresh(new zbItemData(EResourceType.HeroFrag, m_data.Tid, m_data.Count), isNameVis: true, isCountVis: false);
		OnMax();
	}

	private void RefreshNum()
	{
		((TextElement)m_lbNum).text = m_numSel.ToString();
	}

	private void OnMin()
	{
		if (m_numSel > m_numMin)
		{
			m_numSel = (long)Mathf.Max((float)m_numMin, (float)(m_numSel - 10));
			if (m_numSel > 10 && m_numSel % 10 != 0L)
			{
				m_numSel = Mathf.CeilToInt((float)m_numSel / 10f) * 10;
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
			m_numSel = (long)Mathf.Min((float)m_numMax, (float)(m_numSel + 10));
			if (m_numSel > 10 && m_numSel % 10 != 0L && m_numSel / 10 != m_numMax / 10)
			{
				m_numSel = m_numSel / 10 * 10;
			}
			RefreshNum();
		}
	}

	private void OnConfirm()
	{
		m_callback?.Invoke(m_data, m_numSel);
		Hide();
	}

	private void OnClose()
	{
		m_callback?.Invoke(m_data, 0L);
		Hide();
	}
}
