using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWallSubjectRelicTipWnd : zbBaseWnd
{
	private Button m_btnTouchTest;

	private Button m_btnClose;

	private VisualElement m_spriteFrame;

	private VisualElement m_spriteIcon;

	private Label m_lbName;

	private Label m_lbActived;

	private Label m_lbDesc;

	private Label m_lbTip;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_spriteFrame = UQueryExtensions.Q<VisualElement>(m_RootElement, "frame", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbActived = UQueryExtensions.Q<Label>(m_RootElement, "lb_actived", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_lbTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_tip", (string)null);
		AddDark();
	}

	public void RefreshCustom(zbWallSubjectRelicCsvRecord record, bool isActive)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (record != null)
		{
			m_spriteFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/frame/frame_" + (record.classId + 3) + ".png"));
			m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(record.icon));
			((TextElement)m_lbName).text = record.name;
			zbModCsvRecord recordByKey = zbDataConfig.Instance.modCsvTable.GetRecordByKey(record.mod);
			if (recordByKey != null)
			{
				((TextElement)m_lbDesc).text = recordByKey.desc;
			}
			((VisualElement)m_lbActived).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!isActive));
		}
	}

	public void OnClose(ClickEvent evt)
	{
		Hide();
	}
}
