using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldMap
{
	private zbWorldWnd m_wndParent;

	public VisualElement rootContainer;

	public VisualElement zoomContent;

	public zbWorldItemMgr worldItemMgr;

	private Vector2 panLimitMin;

	private Vector2 panLimitMax;

	private const float zoomMin = 0.45f;

	private const float zoomMax = 1f;

	private Dictionary<int, Vector3> touchPoints;

	private float initialTouchDistance;

	private float initialZoomFactor;

	private Vector2? currPickedVector;

	private bool isPanningZoomContent;

	private bool isPointerDownOnZoomContent;

	private bool isMultiTouch;

	private Vector3 pointerDownPos;

	private Vector3 panStartPos;

	private float pointerDownTime;

	private const float dragThreshold = 10f;

	private const float dragRoomDelay = 0.5f;

	private float m_cellWidth;

	private float m_cellHeight;

	private float m_zoom;

	private Vector2 m_cachedRootSize;

	private Vector2 m_cachedZoomSize;

	private float m_cachedLimitZoom;

	public void OnEnable(zbWorldWnd wndParent, VisualElement root, VisualElement zoom)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		m_wndParent = wndParent;
		rootContainer = root;
		zoomContent = zoom;
		Scale scale = zoomContent.resolvedStyle.scale;
		m_zoom = ((Scale)(ref scale)).value.x;
		if (m_zoom <= 0f)
		{
			m_zoom = 1f;
		}
		worldItemMgr.Init(m_wndParent, zoomContent);
		wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>(rootContainer, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<PointerMoveEvent>(rootContainer, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<PointerUpEvent>(rootContainer, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		wndParent.m_uiEvent.RegisterCallback<WheelEvent>(rootContainer, (Action<WheelEvent>)OnWheelZoom, "", (TrickleDown)0);
		touchPoints.Clear();
	}

	public void Refresh()
	{
		worldItemMgr.Refresh();
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnSinglePointerDownImpl(evt);
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		}
		else if (((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.touch)
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
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
		else if (((PointerEventBase<PointerUpEvent>)(object)evt).pointerType == PointerType.touch)
		{
			if (touchPoints.ContainsKey(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId))
			{
				touchPoints.Remove(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			}
			OnPointerUpImpl(evt);
		}
	}

	private void OnWheelZoom(WheelEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		float num = (0f - evt.delta.y) * 0.01f;
		float num2 = m_zoom + num;
		num2 = Mathf.Clamp(num2, 0.45f, 1f);
		Vector3 zoomCenterInRoot = Vector2.op_Implicit(((MouseEventBase<WheelEvent>)(object)evt).localMousePosition);
		ApplyZoom(num2, zoomCenterInRoot);
	}

	private void OnSinglePointerDownImpl(PointerDownEvent evt)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		isMultiTouch = false;
		Vector3 position = ((PointerEventBase<PointerDownEvent>)(object)evt).position;
		pointerDownPos = position;
		panStartPos = zoomContent.transform.position;
		pointerDownTime = Time.realtimeSinceStartup;
		currPickedVector = null;
		isPanningZoomContent = false;
		isPointerDownOnZoomContent = false;
		Vector2 mousePosition = VisualElementExtensions.WorldToLocal((VisualElement)(object)worldItemMgr.worldGridElement, Vector2.op_Implicit(((PointerEventBase<PointerDownEvent>)(object)evt).position));
		currPickedVector = worldItemMgr.worldGridElement.PickElementAt(mousePosition);
		if (!currPickedVector.HasValue)
		{
			isPointerDownOnZoomContent = true;
		}
	}

	private void OnSinglePointerMoveImpl(PointerMoveEvent evt)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		if (currPickedVector.HasValue)
		{
			Vector3 val = ((PointerEventBase<PointerMoveEvent>)(object)evt).position - pointerDownPos;
			float magnitude = ((Vector3)(ref val)).magnitude;
			float num = Time.realtimeSinceStartup - pointerDownTime;
			if (magnitude > 10f && num < 0.5f && !isPanningZoomContent)
			{
				isPanningZoomContent = true;
				pointerDownPos = ((PointerEventBase<PointerMoveEvent>)(object)evt).position;
				panStartPos = zoomContent.transform.position;
			}
		}
		if (isPointerDownOnZoomContent || isPanningZoomContent)
		{
			Vector3 val2 = ((PointerEventBase<PointerMoveEvent>)(object)evt).position - pointerDownPos;
			Vector3 pan = panStartPos + val2;
			ApplyPan(pan);
		}
	}

	private void OnPointerUpImpl(PointerUpEvent evt)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (isMultiTouch)
		{
			return;
		}
		if (!isPointerDownOnZoomContent && !isPanningZoomContent && currPickedVector.HasValue)
		{
			WorldMapGridDetail worldMapGridDetail = zbSelf.Instance.worldHandler.GetWorldMapGridDetail((int)currPickedVector.Value.x, (int)currPickedVector.Value.y);
			if (worldMapGridDetail != null)
			{
				worldItemMgr.OnGridTouch(worldMapGridDetail);
			}
			else
			{
				zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10494"));
			}
		}
		isMultiTouch = false;
		currPickedVector = null;
		isPanningZoomContent = false;
		isPointerDownOnZoomContent = false;
	}

	private void OnDoublePointerDownImpl()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		isMultiTouch = true;
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		initialTouchDistance = Vector3.Distance(current, current2);
		Scale scale = zoomContent.resolvedStyle.scale;
		initialZoomFactor = ((Scale)(ref scale)).value.x;
	}

	private void OnDoublePointerMoveImpl()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		float num = Vector3.Distance(current, current2) / initialTouchDistance;
		float num2 = initialZoomFactor * num;
		num2 = Mathf.Clamp(num2, 0.45f, 1f);
		Vector3 zoomCenterInRoot = (current + current2) * 0.5f;
		ApplyZoom(num2, zoomCenterInRoot);
	}

	private void ApplyPan(Vector3 pan)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		SetPanLimit();
		pan.x = Mathf.Clamp(pan.x, panLimitMin.x, panLimitMax.x);
		pan.y = Mathf.Clamp(pan.y, panLimitMin.y, panLimitMax.y);
		zoomContent.transform.position = pan;
	}

	private void SetPanLimit()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Rect layout = rootContainer.layout;
		Vector2 size = ((Rect)(ref layout)).size;
		layout = zoomContent.layout;
		Vector2 size2 = ((Rect)(ref layout)).size;
		if (!(size == m_cachedRootSize) || !(size2 == m_cachedZoomSize) || !Mathf.Approximately(m_zoom, m_cachedLimitZoom))
		{
			m_cachedRootSize = size;
			m_cachedZoomSize = size2;
			m_cachedLimitZoom = m_zoom;
			Vector2 val = size2 * m_zoom;
			panLimitMin.x = size.x - val.x;
			panLimitMax.x = 0f;
			panLimitMin.y = size.y - val.y;
			panLimitMax.y = 0f;
		}
	}

	private Vector3 GetCenterPosZoomContent(Vector3 zoomCenterInRoot)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector2.op_Implicit(VisualElementExtensions.LocalToWorld(rootContainer, Vector2.op_Implicit(zoomCenterInRoot)));
		return Vector2.op_Implicit(VisualElementExtensions.WorldToLocal(zoomContent, Vector2.op_Implicit(val)));
	}

	private void ApplyZoom(float newZoom, Vector3 zoomCenterInRoot)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!Mathf.Approximately(newZoom, m_zoom))
		{
			Vector3 centerPosZoomContent = GetCenterPosZoomContent(zoomCenterInRoot);
			zoomContent.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(newZoom, newZoom)));
			m_zoom = newZoom;
			Vector3 centerPosZoomContent2 = GetCenterPosZoomContent(zoomCenterInRoot);
			Vector3 val = centerPosZoomContent - centerPosZoomContent2;
			Vector3 pan = zoomContent.transform.position - val * newZoom;
			ApplyPan(pan);
		}
	}

	public void GotoGrid(WorldMapGridDetail gotoGridDetail)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (gotoGridDetail != null)
		{
			(int col, int row) colAndRow = zbSelf.Instance.worldHandler.GetColAndRow(gotoGridDetail);
			int item = colAndRow.col;
			int item2 = colAndRow.row;
			float num = (float)item * m_cellWidth - m_cellWidth;
			float num2 = (float)item2 * m_cellHeight - m_cellHeight;
			float zoom = m_zoom;
			Rect layout = rootContainer.layout;
			float num3 = ((Rect)(ref layout)).width / 2f;
			layout = rootContainer.layout;
			float num4 = ((Rect)(ref layout)).height / 2f;
			float num5 = num3 - (num + m_cellWidth / 2f) * zoom;
			float num6 = num4 - (num2 + m_cellHeight / 2f) * zoom;
			ApplyPan(new Vector3(num5, num6, 0f));
			if (worldItemMgr.worldGridElement != null)
			{
				worldItemMgr.worldGridElement.BlinkCellAlpha(gotoGridDetail.X, gotoGridDetail.Y);
			}
		}
	}

	public zbWorldMap()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		worldItemMgr = new zbWorldItemMgr();
		panLimitMin = Vector2.zero;
		panLimitMax = Vector2.zero;
		touchPoints = new Dictionary<int, Vector3>();
		m_cellWidth = 140f;
		m_cellHeight = 140f;
		m_zoom = 1f;
		m_cachedRootSize = Vector2.zero;
		m_cachedZoomSize = Vector2.zero;
		m_cachedLimitZoom = float.NaN;
		base._002Ector();
	}
}
