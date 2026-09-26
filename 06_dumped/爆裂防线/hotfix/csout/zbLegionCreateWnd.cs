using System;
using System.Collections.Generic;
using System.Linq;
using Best.HTTP.Shared.Extensions;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionCreateWnd : zbBaseWnd
{
	private Label m_title;

	private Button m_btnClose;

	private TextField m_textField;

	private Label m_lbPlaceHolder;

	private Label m_lbName;

	private Label m_lbAvatar;

	private VisualElement[] m_iconFlags = (VisualElement[])(object)new VisualElement[8];

	private Label m_lbReqLimit;

	private Label m_lbLvLimit;

	private Label m_lbAutoApply;

	private Label m_lbNeedReq;

	private Label m_lbLvLimitVal;

	private Button m_btnAuto;

	private Label m_btnAutoLabel;

	private Button m_btnNeed;

	private Label m_btnNeedLabel;

	private Button m_btnConfirm;

	private Button m_btnIncrease;

	private Button m_btnReduce;

	private zbDebouncedPnl m_inputTouch;

	private Button m_btnTouch;

	private JToken[] m_flagInfo;

	private EBoolState m_isNeed;

	private int m_preIndex;

	protected override void OnEnable()
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_title = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnAuto = UQueryExtensions.Q<Button>(m_RootElement, "btn_autoApply", (string)null);
		m_btnAutoLabel = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnAuto, "lb_isCheck", (string)null);
		m_btnNeed = UQueryExtensions.Q<Button>(m_RootElement, "btn_needReq", (string)null);
		m_btnNeedLabel = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnNeed, "lb_isCheck", (string)null);
		m_btnIncrease = UQueryExtensions.Q<Button>(m_RootElement, "btn_increase", (string)null);
		m_btnReduce = UQueryExtensions.Q<Button>(m_RootElement, "btn_reduce", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_textField = UQueryExtensions.Q<TextField>(m_RootElement, "TextField", (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_lbPlaceHolder = new Label();
		((VisualElement)m_lbPlaceHolder).name = "custom-placeholder";
		((VisualElement)m_lbPlaceHolder).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)m_lbPlaceHolder).style.left = StyleLength.op_Implicit(8f);
		((VisualElement)m_lbPlaceHolder).style.top = StyleLength.op_Implicit(2f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)m_lbPlaceHolder);
		((VisualElement)m_lbPlaceHolder).pickingMode = (PickingMode)1;
		((TextElement)m_lbPlaceHolder).text = zbUtil.Language("lang.10078");
		((VisualElement)m_textField).Add((VisualElement)(object)m_lbPlaceHolder);
		m_lbName = UQueryExtensions.Q<Label>(m_RootElement, "lb_name", (string)null);
		m_lbAvatar = UQueryExtensions.Q<Label>(m_RootElement, "lb_avatar", (string)null);
		m_lbLvLimitVal = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvLimitVal", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_flagInfo = ((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["icon"][(object)"iconList"]).ToArray();
		AddDark();
		for (int i = 0; i < 8; i++)
		{
			m_iconFlags[i] = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon_flag" + i, (string)null);
			m_iconFlags[i].style.backgroundImage = new StyleBackground(zbSelf.Instance.legionHandler.GetFlagIconById((int)m_flagInfo[i]));
			Button val = UQueryExtensions.Q<Button>(m_iconFlags[i], "btn_check", (string)null);
			int tempIndex = i;
			m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)val, (Action<ClickEvent>)delegate(ClickEvent evt)
			{
				OnCheckFlag(evt, tempIndex);
			}, "", (TrickleDown)0);
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)val, "lb_isChecked", (string)null)).visible = false;
		}
		((VisualElement)UQueryExtensions.Q<Label>(m_iconFlags[m_preIndex], "lb_isChecked", (string)null)).visible = true;
		UpdateButtonStatus();
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAuto, (Action<ClickEvent>)OnClickAutoApply, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNeed, (Action<ClickEvent>)OnClickNeedReq, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIncrease, (Action<ClickEvent>)OnClickIncrease, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReduce, (Action<ClickEvent>)OnClickReduce, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnClickConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<FocusEvent>((VisualElement)(object)m_textField, (Action)OnFocus, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<BlurEvent>((VisualElement)(object)m_textField, (Action)OnBlur, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_textField, (Action)OnInputClick, "", (TrickleDown)0);
	}

	private void OnInputClick()
	{
		((Focusable)m_textField).Focus();
	}

	private void OnFocus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnBlur()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseField<string>)(object)m_textField).value == "")
		{
			((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	public override void Refresh()
	{
		((TextElement)UQueryExtensions.Q<Label>((VisualElement)(object)m_btnConfirm, "lb_cost", (string)null)).text = "X" + Extensions.Value<string>((IEnumerable<JToken>)zbSelf.Instance.legionHandler.GetLegionBase()[(object)"createDiamond"]);
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnClickAutoApply(ClickEvent evt)
	{
		m_isNeed = EBoolState.False;
		UpdateButtonStatus();
	}

	private void OnClickNeedReq(ClickEvent evt)
	{
		m_isNeed = EBoolState.True;
		UpdateButtonStatus();
	}

	private void OnClickIncrease(ClickEvent evt)
	{
		int num = Extensions.ToInt32(((TextElement)m_lbLvLimitVal).text, 0);
		if (num < 80)
		{
			((TextElement)m_lbLvLimitVal).text = (num + 1).ToString();
		}
	}

	private void OnClickReduce(ClickEvent evt)
	{
		int num = Extensions.ToInt32(((TextElement)m_lbLvLimitVal).text, 0);
		if (num > 1)
		{
			((TextElement)m_lbLvLimitVal).text = (num - 1).ToString();
		}
	}

	private void OnCheckFlag(ClickEvent evt, int index)
	{
		((VisualElement)UQueryExtensions.Q<Label>(m_iconFlags[m_preIndex], "lb_isChecked", (string)null)).visible = false;
		m_preIndex = index;
		IEventHandler target = ((EventBase)evt).target;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)((target is VisualElement) ? target : null), "lb_isChecked", (string)null)).visible = true;
	}

	private void OnClickConfirm(ClickEvent evt)
	{
		string text = ((BaseField<string>)(object)m_textField).value.Trim();
		if (text == "")
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10078"));
		}
		else if (text.Length > 14)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10073"));
		}
		else if (!zbCharChecker.IsSafeChatMessage(text))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
		}
		else
		{
			zbWsMsgHandler.Instance.Legion_Create_Req((int)m_flagInfo[m_preIndex], text, Extensions.ToInt32(((TextElement)m_lbLvLimitVal).text, 0), m_isNeed);
		}
	}

	private void UpdateButtonStatus()
	{
		if (m_isNeed == EBoolState.False)
		{
			((VisualElement)m_btnAutoLabel).visible = true;
			((VisualElement)m_btnNeedLabel).visible = false;
		}
		else
		{
			((VisualElement)m_btnAutoLabel).visible = false;
			((VisualElement)m_btnNeedLabel).visible = true;
		}
	}
}
