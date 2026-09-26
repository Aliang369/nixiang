using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class zbLegionWarZonePanel
{
	private zbBaseWnd m_wndParent;

	private VisualElement m_RootElement;

	public Button btn_clickItem;

	public VisualElement m_pullDown_detail;

	private Label m_lbZoneNumber;

	private ScrollView m_scrollView;

	private List<zbLegionWarZoneItem> m_items = new List<zbLegionWarZoneItem>();

	public int currSettleZone;

	public void OnEnable(zbBaseWnd wndParent, VisualElement root)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		m_RootElement = root;
		m_pullDown_detail = UQueryExtensions.Q<VisualElement>(m_RootElement, "pullDown_detail", (string)null);
		m_pullDown_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		btn_clickItem = UQueryExtensions.Q<Button>(m_RootElement, "btn_clickItem", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)btn_clickItem, (Action)OnFollow, "", (TrickleDown)0);
		m_lbZoneNumber = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_scrollView = UQueryExtensions.Q<ScrollView>(m_RootElement, "scrollView", (string)null);
	}

	public void Refresh(int param = 0)
	{
		if (param == 0)
		{
			currSettleZone = zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionZone;
			((TextElement)m_lbZoneNumber).text = string.Format(zbUtil.Language("lang.10899"), zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionZone);
		}
		else
		{
			currSettleZone = zbSelf.Instance.legionWarHandler.currSettleZone;
			((TextElement)m_lbZoneNumber).text = string.Format(zbUtil.Language("lang.10899"), zbSelf.Instance.legionWarHandler.currSettleZone);
		}
		RefreshScrollView();
	}

	private void RefreshScrollView()
	{
		if (zbSelf.Instance.legionWarHandler.maxZone != m_items.Count)
		{
			m_items.Clear();
			((VisualElement)m_scrollView).Clear();
			for (int i = 0; i < zbSelf.Instance.legionWarHandler.maxZone; i++)
			{
				zbLegionWarZoneItem zbLegionWarZoneItem2 = new zbLegionWarZoneItem(m_wndParent, this);
				m_items.Add(zbLegionWarZoneItem2);
				((VisualElement)m_scrollView).Add((VisualElement)(object)zbLegionWarZoneItem2);
			}
		}
		for (int j = 0; j < m_items.Count; j++)
		{
			m_items[j].Refresh(j);
		}
	}

	private void OnFollow()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (m_pullDown_detail.style.display != StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0))
		{
			m_pullDown_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			m_pullDown_detail.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		RefreshScrollView();
	}
}
