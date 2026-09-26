using System;
using Proto.Enums;
using UnityEngine.UIElements;

public class zbDungeonExitWnd : zbBaseWnd
{
	private Label m_lblTip;

	private Button m_btnCancle;

	private Button m_btnOk;

	private zbDungeonHandler m_dungeonHandler;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lblTip = UQueryExtensions.Q<Label>(m_RootElement, "lb_message", (string)null);
		m_btnCancle = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancle, (Action<ClickEvent>)OnCancle, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(zbDungeonHandler dungeonBaseHandler)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		m_dungeonHandler = dungeonBaseHandler;
		((VisualElement)m_btnCancle).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(m_dungeonHandler.dungeonBaseInfo.PassState == EPassState.Failed));
		bool flag = false;
		if (m_dungeonHandler.dungeonBaseInfo.PassState == EPassState.Passed || m_dungeonHandler.dungeonBaseInfo.TriggerCount == m_dungeonHandler.dungeonBaseInfo.EventCount)
		{
			flag = true;
		}
		if (!flag || m_dungeonHandler.dungeonBaseInfo.PassState == EPassState.Failed)
		{
			((TextElement)m_lblTip).text = zbUtil.Language("lang.10502");
			return;
		}
		_ = m_dungeonHandler.dungeonBaseInfo.TriggerCount * 100 / m_dungeonHandler.dungeonBaseInfo.EventCount;
		((TextElement)m_lblTip).text = zbUtil.Language("lang.10503");
	}

	public void OnCancle(ClickEvent e)
	{
		Hide();
	}

	public void OnOk(ClickEvent e)
	{
		Hide();
		m_dungeonHandler.CloseDungeon_Req();
	}
}
