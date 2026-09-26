using System;
using UnityEngine.UIElements;

public class zbMineWarOccupyWnd : zbBaseWnd
{
	private Button m_btnOk;

	private Button m_btnCancel;

	private Button m_btnClose;

	private int leftTime;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnOk = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnOk, (Action<ClickEvent>)OnOk, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom()
	{
		leftTime = 10;
		UpdatePerSecond();
	}

	private void OnOk(ClickEvent evt)
	{
		zbWsMsgHandler.Instance.MineWar_OccupyPoint_Req(zbSelf.Instance.mineWarHandler.changeInfo.changePointId, zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId, zbSelf.Instance.mineWarHandler.changeInfo.changeUUID);
		Hide();
		zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = 0;
	}

	private void OnClose(ClickEvent evt)
	{
		zbSelf.Instance.mineWarHandler.changeInfo.changePlayerId = 0;
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		((TextElement)m_btnOk).text = zbUtil.Language("lang.10206") + " (" + leftTime + ")";
		leftTime--;
		if (leftTime == -1)
		{
			OnOk(null);
		}
	}
}
