using System;
using UnityEngine.UIElements;

public class zbPvpModChangeWnd : zbBaseWnd
{
	[NonSerialized]
	private Button m_btnConfirm;

	private Button m_btnCancel;

	private zbPvpModSelected m_oldModSelect;

	private zbPvpModSelected m_NewModSelect;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnCancel, "", (TrickleDown)0);
		m_oldModSelect = new zbPvpModSelected(UQueryExtensions.Q<VisualElement>(m_RootElement, "oldMod", (string)null), this);
		m_NewModSelect = new zbPvpModSelected(UQueryExtensions.Q<VisualElement>(m_RootElement, "newMod", (string)null), this);
		((TextElement)UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "oldMod", (string)null), "lb_modDesc", (string)null)).text = zbUtil.Language("lang.11460");
		((TextElement)UQueryExtensions.Q<Label>(UQueryExtensions.Q<VisualElement>(m_RootElement, "newMod", (string)null), "lb_modDesc", (string)null)).text = zbUtil.Language("lang.11457");
		m_gameEevent.RegisterEvent(EGameEvent.Event_PvpSet_ModSubmit, OnEventUpdate);
	}

	public void RefreshCustom()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList.Count <= 0)
		{
			((VisualElement)m_btnConfirm).style.left = StyleLength.op_Implicit(240f);
			((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
		else
		{
			((VisualElement)m_btnConfirm).style.left = StyleLength.op_Implicit(380f);
			((VisualElement)m_btnCancel).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		m_oldModSelect.Refresh(zbServerData.sGameData.PvpInfo.ValidModInfo);
		m_NewModSelect.Refresh(zbServerData.sGameData.PvpInfo.SetModInfo);
	}

	protected void OnSwitchConfirm(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.PvpSet_ModSubmit_Req();
		}
	}

	public void OnConfirm()
	{
		if (zbServerData.sGameData.PvpInfo.ValidModInfo.SelectList.Count <= 0)
		{
			OnSwitchConfirm(isOk: true);
		}
		else
		{
			zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11458"), OnSwitchConfirm);
		}
	}

	protected void OnSwitchCancel(bool isOk)
	{
		if (isOk)
		{
			zbWsMsgHandler.Instance.PvpSet_ModCancel_Req();
		}
	}

	public void OnCancel()
	{
		zbUIMgr.Instance.ShowMessageBox("Tip", zbUtil.Language("lang.11459"), OnSwitchCancel);
	}

	private void OnEventUpdate(object data = null)
	{
		Hide();
	}
}
