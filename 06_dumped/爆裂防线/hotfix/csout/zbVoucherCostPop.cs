using System;
using UnityEngine.UIElements;

public class zbVoucherCostPop : zbBaseWnd
{
	private Action m_actionCall;

	private Button m_btnTouchTest;

	private Button m_btnClose;

	private Button m_btnBuy;

	private VisualElement m_check;

	private Label m_lbltip;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbltip = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_btnTouchTest = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_check = UQueryExtensions.Q<VisualElement>(m_RootElement, "checkbox", (string)null);
		m_btnBuy = UQueryExtensions.Q<Button>(m_RootElement, "btn_buy", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBuy, (Action)OnBuy, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouchTest, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>(m_check, (Action)OnChcek, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(float price, Action callback = null)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		m_actionCall = callback;
		int consumption = zbSetSave.Instance.GetConsumption();
		m_check.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(consumption != 1));
		((TextElement)m_lbltip).text = string.Format(zbUtil.Language("lang.10423"), price);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnChcek()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((int)m_check.resolvedStyle.display == 1)
		{
			m_check.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			zbSetSave.Instance.SetConsumption(1);
		}
		else
		{
			m_check.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			zbSetSave.Instance.SetConsumption(0);
		}
	}

	private void OnBuy()
	{
		m_actionCall?.Invoke();
		OnClose();
	}
}
