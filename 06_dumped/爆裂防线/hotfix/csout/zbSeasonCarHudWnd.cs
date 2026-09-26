using System;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonCarHudWnd : zbBaseWnd
{
	private Button m_btnTouch;

	private VisualElement m_btns;

	private Button m_btnFrom;

	private Button m_btnDetail;

	private Button m_btnSpeed;

	private Button m_btnTo;

	public float offsetX = 170f;

	public float offsetY = 50f;

	private zbSeasonCar m_car;

	private zbSeasonLineMgr.Line m_line;

	protected override void OnEnable()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnable();
		m_btnTouch = UQueryExtensions.Q<Button>(m_RootElement, "btn_touch", (string)null);
		m_btns = UQueryExtensions.Q<VisualElement>(m_RootElement, "btns", (string)null);
		m_btnFrom = UQueryExtensions.Q<Button>(m_RootElement, "btn_from", (string)null);
		m_btnDetail = UQueryExtensions.Q<Button>(m_RootElement, "btn_detail", (string)null);
		m_btnSpeed = UQueryExtensions.Q<Button>(m_RootElement, "btn_speed", (string)null);
		m_btnTo = UQueryExtensions.Q<Button>(m_RootElement, "btn_to", (string)null);
		m_btns.style.translate = StyleTranslate.op_Implicit(new Translate(Length.Percent(-50f), Length.Percent(((float)Screen.height >= 1680f) ? (-22f) : 22f)));
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTouch, (Action)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnFrom, (Action)OnFrom, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnTo, (Action)OnTo, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSpeed, (Action)OnSpeed, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnDetail, (Action)OnDetail, "", (TrickleDown)0);
	}

	public void RefreshCustom(zbSeasonCar car)
	{
		m_car = car;
		if ((Object)(object)m_car != (Object)null)
		{
			m_line = zbSeasonLineMgr.Instance.m_lines[m_car.LineId];
			FollowCar();
		}
	}

	private void FollowCar()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_car == (Object)null)
		{
			return;
		}
		Camera mainCamera = zbCameraHelper.Instance.mainCamera;
		if (!((Object)(object)mainCamera == (Object)null))
		{
			Vector3 val = mainCamera.WorldToScreenPoint(((Component)m_car).transform.position);
			if (!(val.z < 0f))
			{
				Vector2 val2 = zbUtil.ScreenToUIToolkit(new Vector2(val.x, val.y));
				m_btns.style.left = StyleLength.op_Implicit(val2.x);
				m_btns.style.top = StyleLength.op_Implicit(val2.y);
			}
		}
	}

	private void OnFrom()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.Round(m_line.start.x, 2, MidpointRounding.AwayFromZero);
		float num2 = (float)Math.Round(m_line.start.y, 2, MidpointRounding.AwayFromZero);
		Vector2 key = default(Vector2);
		((Vector2)(ref key))._002Ector(num, num2);
		zbSeasonMap zbSeasonMap2 = zbSeasonMgr.Instance?.SeasonMap;
		if (zbSeasonMap2 != null && zbSeasonMap2.m_buildDict.TryGetValue(key, out var value))
		{
			zbSeasonMap2.TrackGameObject(value);
		}
		OnClose();
	}

	private void OnTo()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)Math.Round(m_line.end.x, 2, MidpointRounding.AwayFromZero);
		float num2 = (float)Math.Round(m_line.end.y, 2, MidpointRounding.AwayFromZero);
		Vector2 key = default(Vector2);
		((Vector2)(ref key))._002Ector(num, num2);
		zbSeasonMap zbSeasonMap2 = zbSeasonMgr.Instance?.SeasonMap;
		if (zbSeasonMap2 != null && zbSeasonMap2.m_buildDict.TryGetValue(key, out var value))
		{
			zbSeasonMap2.TrackGameObject(value);
		}
		OnClose();
	}

	private void OnSpeed()
	{
	}

	private void OnDetail()
	{
	}

	private void OnClose()
	{
		Hide();
	}

	protected override void UpdateAtShow()
	{
		base.UpdateAtShow();
		FollowCar();
	}
}
