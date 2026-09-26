using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTermDescTipWnd : zbBaseWnd
{
	private Label m_lbTitle;

	private Label m_lbDesc;

	private Button m_btnTouch;

	private VisualElement m_bg;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
	}

	public void RefreshCustom(zbLangTermCsvRecord record, Vector2 pos)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		m_bg.MarkDirtyRepaint();
		m_bg.style.left = StyleLength.op_Implicit(260f);
		float height = zbUIMgr.Instance.GetRootElement().resolvedStyle.height;
		m_bg.style.bottom = StyleLength.op_Implicit(height - pos.y + ((height == 1480f) ? 100f : 0f));
		m_bg.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		RefreshRecord(record);
	}

	private void RefreshRecord(zbLangTermCsvRecord record)
	{
		((TextElement)m_lbTitle).text = "";
		((TextElement)m_lbDesc).text = "";
		string stringTable = zbLanguage.Instance.GetStringTable(record.name);
		((TextElement)m_lbTitle).text = stringTable;
		if (!string.IsNullOrEmpty(record.desc))
		{
			((TextElement)m_lbDesc).text = zbUtil.Language(record.desc);
		}
	}

	private void OnClose()
	{
		Hide();
	}
}
