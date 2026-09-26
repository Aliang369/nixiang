using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipModWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Button m_btnTouchTest;

	private Label m_lbDesc;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(string desc, Vector2 pos)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbDesc).text = desc;
		Vector2 val = pos;
		val.x = Mathf.Clamp(pos.x - 260f, 20f, 180f);
		val.y = ((pos.y < 840f) ? (pos.y + 50f) : (pos.y - 200f));
		m_bg.style.left = StyleLength.op_Implicit(val.x);
		m_bg.style.top = StyleLength.op_Implicit(val.y);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
