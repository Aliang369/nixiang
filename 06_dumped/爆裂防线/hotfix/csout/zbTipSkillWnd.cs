using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbTipSkillWnd : zbBaseWnd
{
	private VisualElement m_bg;

	private Button m_btnTouchTest;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbDesc;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_bg = UQueryExtensions.Q<VisualElement>(m_RootElement, "bg", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnTouchTest, "", (TrickleDown)0);
	}

	public void RefreshCustom(int skillId, Vector2 pos)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		zbSkillCsvRecord recordByKey = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(skillId);
		if (recordByKey != null)
		{
			Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(recordByKey.icon);
			if ((Object)(object)val != (Object)null)
			{
				m_spriteIcon.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
				m_spriteIcon.MarkDirtyRepaint();
			}
			((TextElement)m_lbName).text = recordByKey.name;
			((TextElement)m_lbDesc).text = recordByKey.desc;
		}
		Vector2 val2 = pos;
		val2.x = Mathf.Clamp(pos.x - 160f, 20f, 340f);
		val2.y = ((pos.y < 840f) ? (pos.y + 50f) : (pos.y - 280f));
		m_bg.style.left = StyleLength.op_Implicit(val2.x);
		m_bg.style.top = StyleLength.op_Implicit(val2.y);
	}

	public void OnTouchTest(ClickEvent evt)
	{
		Hide();
	}
}
