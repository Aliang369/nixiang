using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;

public class zbLegionChangeInfoWnd : zbBaseWnd
{
	private Button m_btnClose;

	private TextField m_textField;

	private Label m_lbPlaceHolder;

	private VisualElement[] m_iconFlags = (VisualElement[])(object)new VisualElement[8];

	private zbDebouncedPnl m_inputTouch;

	private Button m_btnConfirm;

	private Label m_lbCost;

	private JToken[] m_flagInfo;

	private int m_preIndex;

	private int[] m_ids;

	protected override void OnEnable()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_textField = UQueryExtensions.Q<TextField>(m_RootElement, "TextField", (string)null);
		m_inputTouch = UQueryExtensions.Q<zbDebouncedPnl>(m_RootElement, "input_touch", (string)null);
		m_inputTouch.RegisterClickEvent(this, OnInputClick);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_lbCost = UQueryExtensions.Q<Label>(m_RootElement, "lb_cost", (string)null);
		m_lbPlaceHolder = new Label();
		((VisualElement)m_lbPlaceHolder).name = "custom-placeholder";
		((VisualElement)m_lbPlaceHolder).style.position = StyleEnum<Position>.op_Implicit((Position)1);
		((VisualElement)m_lbPlaceHolder).style.left = StyleLength.op_Implicit(8f);
		((VisualElement)m_lbPlaceHolder).style.top = StyleLength.op_Implicit(2f);
		zbFontReplacer.Instance.ReplaceFontInUI((VisualElement)(object)m_lbPlaceHolder);
		((VisualElement)m_lbPlaceHolder).pickingMode = (PickingMode)1;
		((TextElement)m_lbPlaceHolder).text = zbUtil.Language("lang.10078");
		((VisualElement)m_textField).Add((VisualElement)(object)m_lbPlaceHolder);
		m_uiEvent.RegisterCallback<BlurEvent>((VisualElement)(object)m_textField, (Action)OnBlur, "", (TrickleDown)0);
		m_flagInfo = ((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["icon"][(object)"iconList"]).ToArray();
		m_uiEvent.RegisterCallback<ChangeEvent<string>>((VisualElement)(object)m_textField, (Action<ChangeEvent<string>>)OnTextChanged, "", (TrickleDown)0);
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
		m_uiEvent.RegisterCallback<FocusEvent>((VisualElement)(object)m_textField, (Action)OnFocus, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<BlurEvent>((VisualElement)(object)m_textField, (Action)OnBlur, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnClickConfirm, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Legion_ChangeInfo, OnUpdateEvent);
		AddDark();
	}

	public override void Refresh()
	{
		RefreshFlags();
		m_ids = m_flagInfo.Select((JToken x) => Extensions.Value<int>((IEnumerable<JToken>)x)).ToArray();
		int icon = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Icon;
		m_preIndex = Array.IndexOf(m_ids, icon);
		((VisualElement)UQueryExtensions.Q<Label>(m_iconFlags[m_preIndex], "lb_isChecked", (string)null)).visible = true;
		RefreshCost();
	}

	private void RefreshFlags()
	{
		for (int i = 0; i < 8; i++)
		{
			((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)UQueryExtensions.Q<Button>(m_iconFlags[i], "btn_check", (string)null), "lb_isChecked", (string)null)).visible = false;
		}
	}

	private void RefreshCost()
	{
		int num = ((!(((BaseField<string>)(object)m_textField).value == "") && !(((BaseField<string>)(object)m_textField).value.Trim() == zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.LegionName)) ? Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["nameChangeDiamond"]) : 0);
		int num2 = ((m_ids[m_preIndex] != zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Icon) ? Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["iconChangeDiamond"]) : 0);
		((TextElement)m_lbCost).text = "X" + (num + num2);
	}

	private void OnCheckFlag(ClickEvent evt, int index)
	{
		((VisualElement)UQueryExtensions.Q<Label>(m_iconFlags[m_preIndex], "lb_isChecked", (string)null)).visible = false;
		m_preIndex = index;
		IEventHandler target = ((EventBase)evt).target;
		((VisualElement)UQueryExtensions.Q<Label>((VisualElement)(object)((target is VisualElement) ? target : null), "lb_isChecked", (string)null)).visible = true;
		RefreshCost();
	}

	private void OnInputClick()
	{
		((Focusable)m_textField).Focus();
	}

	private void OnClickConfirm(ClickEvent evt)
	{
		string text = ((BaseField<string>)(object)m_textField).value.Trim();
		if (text.Length > 14)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10073"));
			return;
		}
		if (!zbCharChecker.IsSafeChatMessage(text))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10074"));
			return;
		}
		int num = ((!(((BaseField<string>)(object)m_textField).value == "") && !(((BaseField<string>)(object)m_textField).value.Trim() == zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.LegionName)) ? Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["nameChangeDiamond"]) : 0);
		int num2 = ((m_ids[m_preIndex] != zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Icon) ? Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.legionBaseJsonTable.GetRecord()["iconChangeDiamond"]) : 0);
		if (num > 0)
		{
			zbWsMsgHandler.Instance.Legion_NameChange_Req(((BaseField<string>)(object)m_textField).value);
		}
		if (num2 > 0)
		{
			zbWsMsgHandler.Instance.Legion_IconChange_Req(m_ids[m_preIndex]);
		}
	}

	private void OnBlur()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseField<string>)(object)m_textField).value == "")
		{
			((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
	}

	private void OnFocus()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((VisualElement)m_lbPlaceHolder).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
	}

	private void OnClose()
	{
		Hide();
		((BaseField<string>)(object)m_textField).value = "";
	}

	private void OnUpdateEvent(object data)
	{
		Hide();
	}

	private void OnTextChanged(ChangeEvent<string> evt)
	{
		_ = evt.newValue;
		RefreshCost();
	}
}
