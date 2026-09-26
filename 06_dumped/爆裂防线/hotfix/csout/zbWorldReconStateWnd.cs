using System;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldReconStateWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private Button m_btnClose;

	private Label m_lbTitle;

	private Label m_lbDesc;

	private VisualElement m_spriteIcon;

	private Label m_lbState;

	private VisualElement m_progress;

	private Label m_lbTime;

	private Button m_btnConfirm;

	private WorldReconDetail m_reconDetail;

	private WorldMapGridDetail m_gridDetail;

	protected override void OnEnable()
	{
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_close", (string)null);
		m_lbTitle = UQueryExtensions.Q<Label>(m_RootElement, "lb_title", (string)null);
		m_lbDesc = UQueryExtensions.Q<Label>(m_RootElement, "lb_desc", (string)null);
		m_spriteIcon = UQueryExtensions.Q<VisualElement>(m_RootElement, "icon", (string)null);
		m_lbState = UQueryExtensions.Q<Label>(m_RootElement, "lb_state", (string)null);
		m_progress = UQueryExtensions.Q<VisualElement>(m_RootElement, "progress", (string)null);
		m_lbTime = UQueryExtensions.Q<Label>(m_RootElement, "lb_time", (string)null);
		m_btnConfirm = UQueryExtensions.Q<Button>(m_RootElement, "btn_confirm", (string)null);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnConfirm, (Action)OnConfirm, "", (TrickleDown)0);
		AddDark();
	}

	public void RefreshCustom(WorldReconDetail reconDetail)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		m_reconDetail = reconDetail;
		m_gridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetailByGridId(m_reconDetail.GridId);
		m_spriteIcon.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>($"ui/image/world/world_team{m_reconDetail.ReconId - 1}.png"));
		RefreshDetail();
		UpdateAtShow();
	}

	private void RefreshDetail()
	{
	}

	protected override void UpdateAtShow()
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		base.UpdatePerSecond();
		long num = m_reconDetail.StateTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num <= 0)
		{
			OnClose();
			return;
		}
		((TextElement)m_lbTime).text = zbUtil.FormatTimeHHMMSS(num);
		float num2 = Math.Clamp((float)num / (float)m_reconDetail.StateCd, 0f, 1f);
		m_progress.style.width = StyleLength.op_Implicit(new Length(num2 * 490f, (LengthUnit)0));
	}

	private void OnClose()
	{
		Hide();
	}

	private void OnConfirm()
	{
		OnClose();
	}
}
