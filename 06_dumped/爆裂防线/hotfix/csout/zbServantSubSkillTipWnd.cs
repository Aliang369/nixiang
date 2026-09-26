using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbServantSubSkillTipWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Button m_btnTouchTest;

	private Label m_lbTip;

	private VisualElement m_wrapper;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_bg, "lb_tip", (string)null);
		m_wrapper = UQueryExtensions.Q<VisualElement>(m_RootElement, "wrapper", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(string desc, Vector2 pos)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		((TextElement)m_lbTip).text = desc;
		m_bg.MarkDirtyRepaint();
		Rect layout = m_bg.layout;
		float num = ((Rect)(ref layout)).width;
		if (float.IsNaN(num) || num <= 0f)
		{
			num = 320f;
		}
		if (pos.x > 360f)
		{
			m_bg.style.left = StyleLength.op_Implicit(pos.x - num);
			m_wrapper.style.scale = StyleScale.op_Implicit(new Scale(new Vector3(-1f, 1f, 1f)));
			((VisualElement)m_lbTip).style.scale = StyleScale.op_Implicit(new Scale(new Vector3(-1f, 1f, 1f)));
		}
		else
		{
			m_bg.style.left = StyleLength.op_Implicit(pos.x);
			m_wrapper.style.transformOrigin = StyleTransformOrigin.op_Implicit(new TransformOrigin(Length.Percent(50f), Length.Percent(50f)));
			m_wrapper.style.scale = StyleScale.op_Implicit(new Scale(new Vector3(1f, 1f, 1f)));
			((VisualElement)m_lbTip).style.transformOrigin = StyleTransformOrigin.op_Implicit(new TransformOrigin(Length.Percent(50f), Length.Percent(50f)));
			((VisualElement)m_lbTip).style.scale = StyleScale.op_Implicit(new Scale(new Vector3(1f, 1f, 1f)));
		}
		m_bg.style.top = StyleLength.op_Implicit(pos.y);
		m_bg.style.position = StyleEnum<Position>.op_Implicit((Position)1);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
