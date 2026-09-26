using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbMessageBox : zbBaseWnd
{
	private Button m_btnTouch;

	private Label m_lbTitle;

	private Label m_lbMessage;

	private Button m_btnOk;

	private Button m_btnCancel;

	private Button m_btnClose;

	private Action<bool> m_callback;

	private VisualElement m_spriteColor;

	private VisualElement m_spriteIcon;

	private Label m_lbLeve;

	private HomelandRoomDetail m_data;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbMessage = UQueryExtensions.Q<Label>(m_RootElement, "lb_message", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_spriteColor = UQueryExtensions.Q<VisualElement>(m_RootElement, "roomType", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbLeve = UQueryExtensions.Q<Label>(m_RootElement, "lb_level", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(string title, string message, Action<bool> callback, string lbBtnOk = "1", string lbCancel = "0", bool isTouchVis = true, bool showRoomIcon = false, HomelandRoomDetail roomData = null, bool isCloseVis = true)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_btnTouch).visible = isTouchVis;
		((VisualElement)m_btnClose).visible = isCloseVis;
		if (isCloseVis)
		{
			((VisualElement)m_btnCancel).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_red.png"));
			((VisualElement)m_btnOk).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
		}
		else
		{
			((VisualElement)m_btnCancel).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_orange.png"));
			((VisualElement)m_btnOk).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/button/btn_250_80_red.png"));
		}
		if (lbBtnOk.Equals("1"))
		{
			lbBtnOk = zbUtil.Language("lang.10206");
		}
		if (lbCancel.Equals("0"))
		{
			lbCancel = zbUtil.Language("lang.10287");
		}
		((TextElement)m_lbTitle).text = title;
		((TextElement)m_lbMessage).text = message;
		m_callback = callback;
		((TextElement)m_btnOk).text = lbBtnOk;
		((TextElement)m_btnCancel).text = lbCancel;
		((VisualElement)m_btnOk).style.right = StyleLength.op_Implicit(65f);
		((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		if (string.IsNullOrEmpty(lbCancel))
		{
			((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((VisualElement)m_btnOk).style.right = StyleLength.op_Implicit(230f);
		}
		m_data = roomData;
		m_spriteColor.visible = showRoomIcon;
		if (showRoomIcon && m_data != null)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(m_data.TypeId);
			if (recordByKey != null)
			{
				Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.iconType);
				m_spriteIcon.style.backgroundImage = new StyleBackground(val);
				((TextElement)m_lbLeve).text = "Lv." + m_data.Level;
			}
		}
		else
		{
			m_spriteColor.style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			m_spriteIcon.style.backgroundImage = StyleBackground.op_Implicit((Texture2D)null);
			((TextElement)m_lbLeve).text = string.Empty;
		}
	}

	private void OnOk(ClickEvent evt)
	{
		Hide();
		m_callback?.Invoke(obj: true);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
		m_callback?.Invoke(obj: false);
	}
}
