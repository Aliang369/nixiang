using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbJiziProbabilityWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement[] m_items = (VisualElement[])(object)new VisualElement[5];

	private zbItem[] m_props = new zbItem[5];

	private Label m_lbMain;

	private Dictionary<string, float>[] m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbMain = UQueryExtensions.Q<Label>(m_RootElement, "lb_main", (string)null);
		((TextElement)m_lbMain).text = zbUtil.Language("lang.10945");
		for (int i = 0; i < 5; i++)
		{
			m_items[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "item" + i, (string)null);
			m_props[i] = UQueryExtensions.Q<zbItem>(m_items[i], (string)null, (string)null);
			m_props[i].OnEnable(this);
		}
		AddDark();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(Dictionary<string, float>[] data)
	{
		m_data = data;
		int num = 0;
		for (int i = 0; i < m_data.Count(); i++)
		{
			num += (int)m_data[i]["weight"];
		}
		for (int j = 0; j < m_data.Count(); j++)
		{
			Dictionary<string, float> dictionary = m_data[j];
			zbItemData data2 = new zbItemData(EResourceType.Prop, (int)dictionary["id"], 1L);
			m_props[j].Refresh(data2, isNameVis: false, isCountVis: false);
			((TextElement)UQueryExtensions.Q<Label>(m_items[j], "lb_mainProb", (string)null)).text = dictionary["weight"] / (float)num * 100f + "%";
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
