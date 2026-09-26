using System;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldReconWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private VisualElement m_spriteIcon;

	private VisualElement m_stateIdle;

	private Label m_lbState;

	private VisualElement m_stateExplore;

	private VisualElement m_progress;

	private Label m_lbTime;

	private Button m_btnCancel;

	private Button m_btnConfirm;

	private WorldReconDetail m_reconDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_stateIdle = UQueryExtensions.Q<VisualElement>(m_RootElement, "idle", (string)null);
		m_lbState = UQueryExtensions.Q<Label>(m_RootElement, "lb_state", (string)null);
		m_stateExplore = UQueryExtensions.Q<VisualElement>(m_RootElement, "explore", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnCancel = UQueryExtensions.Q<Button>(m_RootElement, "btn_cancel", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnCancel, (Action)OnCancel, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
		m_gameEevent.RegisterEvent(EGameEvent.Event_WorldReconExploreCancel_Ack, OnEventUpdate);
	}

	public void RefreshCustom(WorldReconDetail reconDetail)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		m_reconDetail = reconDetail;
		((TextElement)m_lbTitle).text = string.Format(zbUtil.Language("lang.10469"), m_reconDetail.ReconId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/world/world_team{m_reconDetail.ReconId - 1}.png"));
		if (m_reconDetail.State == EWorldReconState.Idle)
		{
			m_stateIdle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_stateExplore.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10739");
			((TextElement)m_lbState).text = zbUtil.Language("lang.10742");
			((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10470");
		}
		else if (m_reconDetail.State == EWorldReconState.Explore && zbSelf.Instance.timeHandler.GetServerTime() < m_reconDetail.StateTm)
		{
			m_stateIdle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			m_stateExplore.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			UpdateAtShow();
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10740");
			((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10215");
		}
		else if (m_reconDetail.State == EWorldReconState.Explored)
		{
			m_stateIdle.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			m_stateExplore.style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
			((TextElement)m_lbDesc).text = zbUtil.Language("lang.10741");
			((TextElement)m_lbState).text = zbUtil.Language("lang.10210");
			((TextElement)m_btnConfirm).text = zbUtil.Language("lang.10215");
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		WorldReconDetail reconDetail = m_reconDetail;
		if (reconDetail != null && reconDetail.State == EWorldReconState.Explore)
		{
			long num = m_reconDetail.StateTm - zbSelf.Instance.timeHandler.GetServerTime();
			if (num > 0)
			{
				((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
				float num2 = Math.Clamp((float)num / (float)m_reconDetail.StateCd, 0f, 1f);
				m_progress.style.width = StyleLength.op_Implicit(new Length(num2 * 490f, (LengthUnit)0));
			}
			else
			{
				OnClose();
			}
		}
	}

	private void OnCancel()
	{
		zbUIMgr.Instance.ShowWorldExploreCancel(m_reconDetail);
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnConfirm()
	{
		switch (m_reconDetail.State)
		{
		case EWorldReconState.EmptyValue:
		case EWorldReconState.Idle:
		{
			WorldMapGridDetail canExploreGrid = zbSelf.Instance.worldHandler.GetCanExploreGrid();
			if (canExploreGrid == null)
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10226"));
				break;
			}
			OnClose();
			(zbUIMgr.Instance.ShowWorld(canExploreGrid, delegate(bool isOK)
			{
				if (isOK)
				{
					zbUIMgr.Instance.ShowWorldReconExplore(canExploreGrid);
				}
			}) as zbWorldWnd).SetPanelVisible(vis: false);
			break;
		}
		case EWorldReconState.Explore:
		{
			OnClose();
			WorldMapGridDetail worldMapGridDetailByGridId = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(m_reconDetail.GridId);
			zbUIMgr.Instance.ShowWorld(worldMapGridDetailByGridId);
			break;
		}
		case EWorldReconState.Explored:
			zbWsMsgHandler.Instance.World_ReconExplored_Req(m_reconDetail.ReconId);
			break;
		}
	}

	private void OnEventUpdate(object obj)
	{
		OnClose();
	}
}
