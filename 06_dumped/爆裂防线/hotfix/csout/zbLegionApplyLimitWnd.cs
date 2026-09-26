using System;
using Best.HTTP.Shared.Extensions;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbLegionApplyLimitWnd : zbBaseWnd
{
	private Button m_btnClose;

	private Button m_btnConfirm;

	private Button m_btnAutoApply;

	private Button m_btnNeedReq;

	private Button m_btnReduce;

	private Button m_btnIncrease;

	private Button m_btnTouch;

	private Label m_isAutoApply;

	private Label m_isNeedReq;

	private Label m_lbLvLmit;

	private Label m_lbTitle;

	protected override void OnEnable()
	{
		base.OnEnable();
		AddDark();
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnAutoApply = UQueryExtensions.Q<Button>(m_RootElement, "btn_autoApply", (string)null);
		m_btnNeedReq = UQueryExtensions.Q<Button>(m_RootElement, "btn_needReq", (string)null);
		m_btnReduce = UQueryExtensions.Q<Button>(m_RootElement, "btn_reduce", (string)null);
		m_btnIncrease = UQueryExtensions.Q<Button>(m_RootElement, "btn_increase", (string)null);
		m_lbLvLmit = UQueryExtensions.Q<Label>(m_RootElement, "lb_lvLimit", (string)null);
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "title", (string)null);
		m_isAutoApply = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnAutoApply, "lb_isCheck", (string)null);
		m_isNeedReq = UQueryExtensions.Q<Label>((VisualElement)(object)m_btnNeedReq, "lb_isCheck", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action<ClickEvent>)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnReduce, (Action<ClickEvent>)OnReduce, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnIncrease, (Action<ClickEvent>)OnIncrease, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnAutoApply, (Action<ClickEvent>)OnClickAutoApply, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnNeedReq, (Action<ClickEvent>)OnClickNeedReq, "", (TrickleDown)0);
	}

	public override void Refresh()
	{
		EBoolState confirm = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Confirm;
		((VisualElement)m_isAutoApply).visible = confirm != EBoolState.True;
		((VisualElement)m_isNeedReq).visible = confirm == EBoolState.True;
		((TextElement)m_lbLvLmit).text = zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.JoinLvl.ToString();
	}

	private void OnClose(ClickEvent evt)
	{
		Hide();
	}

	private void OnReduce(ClickEvent evt)
	{
		int num = Extensions.ToInt32(((TextElement)m_lbLvLmit).text, 0);
		if (num > 1)
		{
			((TextElement)m_lbLvLmit).text = (num - 1).ToString();
		}
	}

	private void OnIncrease(ClickEvent evt)
	{
		int num = Extensions.ToInt32(((TextElement)m_lbLvLmit).text, 0);
		if (num <= 80)
		{
			((TextElement)m_lbLvLmit).text = (num + 1).ToString();
		}
	}

	private void OnClickAutoApply(ClickEvent evt)
	{
		((VisualElement)m_isAutoApply).visible = true;
		((VisualElement)m_isNeedReq).visible = false;
	}

	private void OnClickNeedReq(ClickEvent evt)
	{
		((VisualElement)m_isAutoApply).visible = false;
		((VisualElement)m_isNeedReq).visible = true;
	}

	private void OnConfirm(ClickEvent evt)
	{
		EBoolState confirm = (((VisualElement)m_isNeedReq).visible ? EBoolState.True : EBoolState.False);
		zbWsMsgHandler.Instance.Legion_JoinSet_Req(Extensions.ToInt32(((TextElement)m_lbLvLmit).text, 0), confirm);
	}
}
