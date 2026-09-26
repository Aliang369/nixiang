using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipAttrWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Button m_btnTouchTest;

	private Label m_lbTip;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_bg, "lb_tip", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(ESkillType skillType, WarRoomAttrDetail attrDetail, Vector2 screenPos)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		string text = zbUtil.Language("lang.10480") + "\n" + string.Format("{0}: {1}\n", zbUtil.Language("lang.10403"), attrDetail.FromInfo.SkillBase) + string.Format("{0}: {1}\n", zbUtil.Language("lang.10401"), attrDetail.FromInfo.EquipSlot) + string.Format("{0}: {1}\n", zbUtil.Language("lang.10402"), attrDetail.FromInfo.Hero) + string.Format("{0}: {1}\n", zbUtil.Language("lang.10350"), attrDetail.FromInfo.WarRoom);
		if (skillType == ESkillType.Qiang)
		{
			text += string.Format("{0}: {1}", zbUtil.Language("lang.10684"), attrDetail.FromInfo.Gun);
		}
		((TextElement)m_lbTip).text = text;
		Vector2 designPos = ConvertToDesignPos(screenPos);
		((CallbackEventHandler)m_bg).RegisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnGeometryChanged, (TrickleDown)0);
		void OnGeometryChanged(GeometryChangedEvent evt)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			((CallbackEventHandler)m_bg).UnregisterCallback<GeometryChangedEvent>((EventCallback<GeometryChangedEvent>)OnGeometryChanged, (TrickleDown)0);
			float width = m_bg.resolvedStyle.width;
			m_bg.style.left = StyleLength.op_Implicit(designPos.x - width - 25f);
			m_bg.style.top = StyleLength.op_Implicit(designPos.y + 65f);
		}
	}

	private Vector2 ConvertToDesignPos(Vector2 panelPos)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		float num = 720f;
		float num2 = 1480f;
		Rect worldBound = m_bg.panel.visualTree.worldBound;
		Vector2 size = ((Rect)(ref worldBound)).size;
		float num3 = size.x / num;
		float num4 = size.y / num2;
		float num5 = Mathf.Min(num3, num4);
		float num6 = (size.x - num * num5) * 0.5f;
		float num7 = (size.y - num2 * num5) * 0.5f;
		float num8 = (panelPos.x - num6) / num5;
		float num9 = (panelPos.y - num7) / num5;
		return new Vector2(num8, num9);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
