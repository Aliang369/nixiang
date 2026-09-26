using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class zbSeasonMapController
{
	private zbSeasonWnd m_wndParent;

	private zbSeasonMapCinema m_seasonMapCinema;

	private VisualElement m_mapbg;

	private Dictionary<int, Vector3> touchPoints;

	private float initialTouchDistance;

	private bool isPointDown;

	private Vector3 pointerDownPos;

	private Vector3 pointerLastPos;

	private bool isPanning;

	private bool isMultiTouch;

	private Vector3 inertiaVelocity;

	private float inertiaDamping;

	private float inertiaThreshold;

	private float lastMoveTime;

	private float stayDuration;

	private float zoomSpeed;

	private GameObject m_trackingTarget;

	private bool m_isTracking;

	private bool m_isTrackingStart;

	private float m_trackingSpeed;

	private bool m_isContinuousTracking;

	private bool m_isTrackingLocked;

	private float m_focusTimer;

	private const float FOCUS_DURATION = 0.3f;

	private Action m_onTrackingComplete;

	private Action m_onFocusComplete;

	public zbSeasonMapController(zbSeasonWnd wndParent)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		touchPoints = new Dictionary<int, Vector3>();
		inertiaVelocity = Vector3.zero;
		inertiaDamping = 0.93f;
		inertiaThreshold = 0.05f * ((float)Screen.height / 1480f);
		zoomSpeed = 0.08f;
		m_trackingSpeed = 15f;
		base._002Ector();
		m_wndParent = wndParent;
		m_seasonMapCinema = new zbSeasonMapCinema();
		m_seasonMapCinema.Init(m_wndParent);
	}

	public void OnEnable()
	{
		m_mapbg = UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "touch", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>(m_mapbg, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerMoveEvent>(m_mapbg, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerUpEvent>(m_mapbg, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<WheelEvent>(m_mapbg, (Action<WheelEvent>)OnWheelZoom, "", (TrickleDown)0);
	}

	public void StartContinuousTracking(GameObject target, Action onFocusComplete = null)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			m_trackingTarget = target;
			m_isTracking = true;
			m_isTrackingStart = true;
			m_isContinuousTracking = true;
			m_isTrackingLocked = false;
			m_focusTimer = 0f;
			m_onFocusComplete = onFocusComplete;
			m_onTrackingComplete = null;
			inertiaVelocity = Vector3.zero;
		}
	}

	public void StartTracking(GameObject target, Action onComplete = null)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)target == (Object)null))
		{
			m_trackingTarget = target;
			m_isTracking = true;
			m_isTrackingStart = true;
			m_isContinuousTracking = false;
			m_isTrackingLocked = false;
			m_focusTimer = 0f;
			m_onFocusComplete = null;
			m_onTrackingComplete = onComplete;
			inertiaVelocity = Vector3.zero;
		}
	}

	public void StopTracking()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		m_trackingTarget = null;
		m_isTracking = false;
		m_isTrackingStart = false;
		m_isContinuousTracking = false;
		m_isTrackingLocked = false;
		m_onTrackingComplete = null;
		m_onFocusComplete = null;
		inertiaVelocity = Vector3.zero;
	}

	private void UpdateTracking()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isTracking || (Object)(object)m_trackingTarget == (Object)null)
		{
			return;
		}
		Camera mainCamera = zbCameraHelper.Instance.mainCamera;
		Vector3 position = ((Component)mainCamera).transform.position;
		Vector3 position2 = m_trackingTarget.transform.position;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(position2.x, position2.y, position.z);
		val = m_seasonMapCinema.ClampCameraPosition(val);
		if (m_isContinuousTracking && m_isTrackingLocked)
		{
			Vector3 delta = val - position;
			delta.z = 0f;
			m_seasonMapCinema.ApplyPan(delta);
			return;
		}
		m_focusTimer += Time.deltaTime;
		Vector3 delta2 = Vector3.Lerp(position, val, Time.deltaTime * m_trackingSpeed) - position;
		delta2.z = 0f;
		m_seasonMapCinema.ApplyPan(delta2);
		if (!(m_focusTimer >= 0.3f) && !(Vector3.Distance(((Component)mainCamera).transform.position, val) < 0.1f))
		{
			return;
		}
		if (m_isContinuousTracking)
		{
			m_isTrackingLocked = true;
			if (m_onFocusComplete != null)
			{
				Action onFocusComplete = m_onFocusComplete;
				m_onFocusComplete = null;
				onFocusComplete();
			}
			return;
		}
		((Component)mainCamera).transform.position = val;
		if (m_onTrackingComplete != null)
		{
			Action onTrackingComplete = m_onTrackingComplete;
			m_onTrackingComplete = null;
			onTrackingComplete();
		}
		StopTracking();
	}

	public void Update()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (m_isTracking && (Object)(object)m_trackingTarget != (Object)null)
		{
			UpdateTracking();
		}
		else if (!isPointDown && ((Vector3)(ref inertiaVelocity)).magnitude > inertiaThreshold)
		{
			if (stayDuration > 0.5f)
			{
				inertiaVelocity = Vector3.zero;
			}
			Vector3 delta = inertiaVelocity * Time.deltaTime;
			ApplyPan(delta);
			inertiaVelocity *= inertiaDamping;
			if (((Vector3)(ref inertiaVelocity)).magnitude <= inertiaThreshold)
			{
				inertiaVelocity = Vector3.zero;
			}
		}
	}

	private void ApplyZoom(float delta)
	{
		m_seasonMapCinema.ApplyZoom(delta);
	}

	private void ApplyPan(Vector3 delta)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isTracking)
		{
			Vector3 delta2 = default(Vector3);
			((Vector3)(ref delta2))._002Ector(0f - delta.x, delta.y, 0f);
			m_seasonMapCinema.ApplyPan(delta2);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		if (m_isTracking)
		{
			StopTracking();
		}
		inertiaVelocity = Vector3.zero;
		if (((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnSinglePointerDownImpl(evt);
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_mapbg, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		}
		else
		{
			if (!(((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.touch))
			{
				return;
			}
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_mapbg, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
			if (!touchPoints.ContainsKey(((PointerEventBase<PointerDownEvent>)(object)evt).pointerId))
			{
				touchPoints[((PointerEventBase<PointerDownEvent>)(object)evt).pointerId] = ((PointerEventBase<PointerDownEvent>)(object)evt).position;
				if (touchPoints.Count == 1)
				{
					OnSinglePointerDownImpl(evt);
				}
				else if (touchPoints.Count == 2)
				{
					OnDoublePointerDownImpl();
				}
			}
		}
	}

	private void OnPointerMove(PointerMoveEvent evt)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (((PointerEventBase<PointerMoveEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnSinglePointerMoveImpl(evt);
		}
		else
		{
			if (!(((PointerEventBase<PointerMoveEvent>)(object)evt).pointerType == PointerType.touch))
			{
				return;
			}
			Vector3 position = ((PointerEventBase<PointerMoveEvent>)(object)evt).position;
			if (touchPoints.ContainsKey(((PointerEventBase<PointerMoveEvent>)(object)evt).pointerId))
			{
				touchPoints[((PointerEventBase<PointerMoveEvent>)(object)evt).pointerId] = position;
				if (touchPoints.Count == 1)
				{
					OnSinglePointerMoveImpl(evt);
				}
				else if (touchPoints.Count == 2)
				{
					OnDoublePointerMoveImpl();
				}
			}
		}
	}

	private void OnPointerUp(PointerUpEvent evt)
	{
		if (((PointerEventBase<PointerUpEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnPointerUpImpl(evt);
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_mapbg, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
		else if (((PointerEventBase<PointerUpEvent>)(object)evt).pointerType == PointerType.touch)
		{
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_mapbg, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			if (touchPoints.ContainsKey(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId))
			{
				touchPoints.Remove(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			}
			OnPointerUpImpl(evt);
		}
	}

	private void OnWheelZoom(WheelEvent evt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (m_isTracking)
		{
			StopTracking();
		}
		float delta = evt.delta.y * zoomSpeed;
		ApplyZoom(delta);
	}

	private void OnSinglePointerDownImpl(PointerDownEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		pointerLastPos = (pointerDownPos = ((PointerEventBase<PointerDownEvent>)(object)evt).position);
		isPanning = false;
		isPointDown = true;
		isMultiTouch = false;
		lastMoveTime = Time.time;
		stayDuration = 0f;
	}

	private void OnSinglePointerMoveImpl(PointerMoveEvent evt)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isTracking && (!(Vector2.Distance(Vector2.op_Implicit(pointerDownPos), Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position)) < 5f) || !(Time.time - lastMoveTime < 0.2f)) && isPointDown)
		{
			Vector3 val = zbCameraHelper.Instance.mainCamera.ScreenToWorldPoint(new Vector3(((PointerEventBase<PointerMoveEvent>)(object)evt).position.x, ((PointerEventBase<PointerMoveEvent>)(object)evt).position.y, zbCameraHelper.Instance.mainCamera.nearClipPlane));
			Vector3 val2 = zbCameraHelper.Instance.mainCamera.ScreenToWorldPoint(new Vector3(pointerLastPos.x, pointerLastPos.y, zbCameraHelper.Instance.mainCamera.nearClipPlane));
			Vector3 val3 = val - val2;
			ApplyPan(val3);
			isPanning = true;
			if (((Vector3)(ref val3)).magnitude > 0.03f)
			{
				inertiaVelocity = val3 / Time.deltaTime;
			}
			else
			{
				inertiaVelocity = Vector3.zero;
			}
			lastMoveTime = Time.time;
			pointerLastPos = ((PointerEventBase<PointerMoveEvent>)(object)evt).position;
		}
	}

	private void OnPointerUpImpl(PointerUpEvent evt)
	{
		if (!isPanning && isPointDown && !isMultiTouch)
		{
			OnPickEvent(evt);
		}
		isPanning = false;
		isPointDown = false;
		isMultiTouch = false;
		stayDuration = Time.time - lastMoveTime;
	}

	private void OnDoublePointerDownImpl()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		isMultiTouch = true;
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		initialTouchDistance = Vector2.Distance(Vector2.op_Implicit(current), Vector2.op_Implicit(current2));
	}

	private void OnDoublePointerMoveImpl()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		float num = Vector2.Distance(Vector2.op_Implicit(current), Vector2.op_Implicit(current2));
		float num2 = initialTouchDistance - num;
		initialTouchDistance = num;
		float delta = num2 * zoomSpeed;
		ApplyZoom(delta);
	}

	private void OnPickEvent(PointerUpEvent evt)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(zbUtil.GetWorldPosByToolKitPosition(((EventBase)evt).target, Vector2.op_Implicit(((PointerEventBase<PointerUpEvent>)(object)evt).localPosition), zbCameraHelper.Instance.mainCamera));
		Collider2D[] array = Physics2D.OverlapPointAll(new Vector2(val.x, val.y));
		foreach (Collider2D val2 in array)
		{
			zbSeasonCar car = ((Component)val2).GetComponent<zbSeasonCar>();
			if ((Object)(object)car != (Object)null)
			{
				car.OnClick();
				StartContinuousTracking(((Component)car).gameObject, delegate
				{
					zbUIMgr.Instance.ShowSeasonCarHud(car);
				});
				break;
			}
			zbSeasonBuild component = ((Component)val2).GetComponent<zbSeasonBuild>();
			if ((Object)(object)component != (Object)null)
			{
				component.OnClick();
				break;
			}
		}
	}
}
