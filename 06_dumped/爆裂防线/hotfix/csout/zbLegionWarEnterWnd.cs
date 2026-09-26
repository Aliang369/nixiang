using System;
using UnityEngine.UIElements;

public class zbLegionWarEnterWnd : zbBaseWnd
{
	private Label m_lbZone;

	private Button m_btnBack;

	private Button m_btnEnter;

	private Label m_lbTitle;

	private Button m_btnLegionWarTask;

	private VisualElement m_taskNotify;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_lbZone = UQueryExtensions.Q<Label>(m_RootElement, "lb_zone", (string)null);
		m_btnBack = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnEnter = UQueryExtensions.Q<Button>(m_RootElement, "btn_enter", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_btnLegionWarTask = UQueryExtensions.Q<Button>(m_RootElement, "btn_legionWarTask", (string)null);
		m_taskNotify = zbUtil.CreateNotifyTag((VisualElement)(object)m_btnLegionWarTask);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnLegionWarTask, (Action)OnTask, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBack, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnEnter, (Action)OnEnter, "", (TrickleDown)0);
		m_gameEevent.RegisterEvent(EGameEvent.Event_Update_LegionWarEnter, OnUpdateLegionWarEnter);
	}

	public override void Refresh()
	{
		RefreshNotify();
	}

	private void RefreshNotify()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		m_taskNotify.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)(!zbSelf.Instance.legionWarHandler.HasTaskNotify()));
	}

	private void OnEnter()
	{
		if (zbSelf.Instance.legionWarHandler.legionWarLegionInfo.LegionZone != 0)
		{
			zbWsMsgHandler.Instance.LegionWar_EnterWar_Req();
		}
		else
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11205"));
		}
	}

	private void OnTask()
	{
		zbUIMgr.Instance.ShowLegionWarTask();
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdatePerSecond()
	{
		base.UpdatePerSecond();
		RefreshNotify();
	}

	private void OnUpdateLegionWarEnter(object data)
	{
		Hide();
		zbUIMgr.Instance.ShowLegionWar();
	}
}
