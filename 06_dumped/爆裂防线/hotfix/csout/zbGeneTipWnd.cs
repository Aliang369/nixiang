using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbGeneTipWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_tip;

	private VisualElement m_entry;

	private Label m_lbAttr;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_tip = UQueryExtensions.Q<VisualElement>(m_RootElement, "tip", (string)null);
		m_entry = UQueryExtensions.Q<VisualElement>(m_RootElement, "entry", (string)null);
		m_lbAttr = UQueryExtensions.Q<Label>(m_RootElement, "lb_attr", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(List<Dictionary<string, object>> geneInfo, Vector2 pos)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Expected O, but got Unknown
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		m_entry.Clear();
		m_tip.style.right = StyleLength.op_Implicit(pos.x);
		m_tip.style.top = StyleLength.op_Implicit(pos.y);
		bool flag = geneInfo.Count > 0;
		((TextElement)m_lbAttr).text = zbUtil.Language(flag ? "lang.10914" : "lang.10915");
		Dictionary<string, float> dictionary = new Dictionary<string, float>();
		foreach (Dictionary<string, object> item in geneInfo)
		{
			string key = (string)item["attr"];
			float num = Convert.ToSingle(item["val"]);
			if (!dictionary.ContainsKey(key))
			{
				dictionary[key] = num;
			}
			else
			{
				dictionary[key] += num;
			}
		}
		foreach (KeyValuePair<string, float> item2 in dictionary)
		{
			string attr = item2.Key;
			float value = item2.Value;
			Convert.ToInt32(geneInfo.Find((Dictionary<string, object> g) => (string)g["attr"] == attr)["showPercent"]);
			Label val = new Label();
			((TextElement)val).text = attr + zbAttrHelper.FormatAttrValue(value, isPercentageDisplay: false, isDisplayAdd: true);
			((VisualElement)val).style.fontSize = StyleLength.op_Implicit(18f);
			((VisualElement)val).style.unityFontStyleAndWeight = StyleEnum<FontStyle>.op_Implicit((FontStyle)1);
			zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)val);
			m_entry.Add((VisualElement)(object)val);
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
