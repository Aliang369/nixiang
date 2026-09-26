using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeMap
{
	private zbHomeWnd m_wndParent;

	private SpriteRenderer m_spriteBg;

	private Collider2D m_bgCol;

	private VisualElement m_eleHomeBg;

	private zbHomeMapCinema m_homeMapCinema;

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

	public zbHomeMap(zbHomeWnd wndParent, zbHomeMapCinema homeMapCinema)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		touchPoints = new Dictionary<int, Vector3>();
		inertiaVelocity = Vector3.zero;
		inertiaDamping = 0.93f;
		inertiaThreshold = 0.05f * ((float)Screen.height / 1480f);
		zoomSpeed = 0.075f;
		m_trackingSpeed = 5f;
		base._002Ector();
		m_wndParent = wndParent;
		m_homeMapCinema = homeMapCinema;
	}

	public void OnEnable()
	{
		m_spriteBg = ((Component)m_wndParent.GetMyGameObjectSceneRoot().transform.Find("homeMap/front/bg_bottom")).GetComponent<SpriteRenderer>();
		m_bgCol = ((Component)m_spriteBg).GetComponent<Collider2D>();
		m_eleHomeBg = UQueryExtensions.Q<VisualElement>(m_wndParent.GetRootElement(), "home_bottom", (string)null);
		m_wndParent.m_uiEvent.RegisterCallback<PointerDownEvent>(m_eleHomeBg, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerMoveEvent>(m_eleHomeBg, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerUpEvent>(m_eleHomeBg, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<PointerCancelEvent>(m_eleHomeBg, (Action<PointerCancelEvent>)OnPointerCancel, "", (TrickleDown)0);
		m_wndParent.m_uiEvent.RegisterCallback<WheelEvent>(m_eleHomeBg, (Action<WheelEvent>)OnWheelZoom, "", (TrickleDown)0);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_WorldInfo, OnEventUpdateWorldlandInfo);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Update_HomelandInfo, OnEventUpdateHomelandInfo);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_HeroTrial_Data, OnEventUpdateRoomHeroMgr);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Suit_Refresh, OnEventUpdateRoomHeroMgr);
		m_wndParent.m_gameEevent.RegisterEvent(EGameEvent.Event_Gun_Refresh, OnEventUpdateRoomHeroMgr);
	}

	public void StartTracking(GameObject target)
	{
		if (!((Object)(object)target == (Object)null))
		{
			m_trackingTarget = target;
			m_isTracking = true;
			m_isTrackingStart = true;
		}
	}

	public void StopTracking()
	{
		m_trackingTarget = null;
		m_isTracking = false;
		m_isTrackingStart = false;
	}

	private void UpdateTracking()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (!m_isTracking || (Object)(object)m_trackingTarget == (Object)null)
		{
			return;
		}
		Vector3 position = m_trackingTarget.transform.position;
		Vector3 position2 = ((Component)zbCameraHelper.Instance.mainCamera).transform.position;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(position.x, position.y, position2.z);
		if (m_isTrackingStart)
		{
			Vector3 delta = Vector3.Lerp(position2, val, Time.deltaTime * m_trackingSpeed) - position2;
			delta.z = 0f;
			m_homeMapCinema.ApplyPan(delta);
			if (Vector3.Distance(position2, val) < 0.01f)
			{
				m_isTrackingStart = false;
			}
		}
		else
		{
			Vector3 delta2 = val - position2;
			delta2.z = 0f;
			m_homeMapCinema.ApplyPan(delta2);
		}
	}

	public void Update()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (m_isTracking && (Object)(object)m_trackingTarget != (Object)null)
		{
			UpdateTracking();
		}
		else if (!isPointDown && ((Vector3)(ref inertiaVelocity)).magnitude > inertiaThreshold)
		{
			if (stayDuration > 0.5f)
			{
				inertiaVelocity = Vector2.op_Implicit(Vector2.zero);
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
		m_homeMapCinema.ApplyZoom(delta);
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
			m_homeMapCinema.ApplyPan(delta2);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		inertiaVelocity = Vector3.zero;
		if (((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnSinglePointerDownImpl(evt);
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_eleHomeBg, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		}
		else
		{
			if (!(((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.touch))
			{
				return;
			}
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)m_eleHomeBg, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
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
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_eleHomeBg, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
		else if (((PointerEventBase<PointerUpEvent>)(object)evt).pointerType == PointerType.touch)
		{
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_eleHomeBg, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			if (touchPoints.ContainsKey(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId))
			{
				touchPoints.Remove(((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
			}
			OnPointerUpImpl(evt);
		}
	}

	private void OnPointerCancel(PointerCancelEvent evt)
	{
		PointerCaptureHelper.ReleasePointer((IEventHandler)(object)m_eleHomeBg, ((PointerEventBase<PointerCancelEvent>)(object)evt).pointerId);
		if (touchPoints.ContainsKey(((PointerEventBase<PointerCancelEvent>)(object)evt).pointerId))
		{
			touchPoints.Remove(((PointerEventBase<PointerCancelEvent>)(object)evt).pointerId);
		}
		if (touchPoints.Count == 0)
		{
			isPanning = false;
			isPointDown = false;
			isMultiTouch = false;
		}
	}

	private void OnWheelZoom(WheelEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if ((!(Vector2.Distance(Vector2.op_Implicit(pointerDownPos), Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position)) < 5f) || !(Time.time - lastMoveTime < 0.2f)) && isPointDown)
		{
			Vector3 val = zbCameraHelper.Instance.mainCamera.ScreenToWorldPoint(new Vector3(((PointerEventBase<PointerMoveEvent>)(object)evt).position.x, ((PointerEventBase<PointerMoveEvent>)(object)evt).position.y, zbCameraHelper.Instance.mainCamera.nearClipPlane));
			Vector3 val2 = zbCameraHelper.Instance.mainCamera.ScreenToWorldPoint(new Vector3(pointerLastPos.x, pointerLastPos.y, zbCameraHelper.Instance.mainCamera.nearClipPlane));
			Vector3 val3 = val - val2;
			ApplyPan(val3);
			isPanning = true;
			if ((double)((Vector3)(ref val3)).magnitude > 0.03)
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
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_CLICK);
		if (zbHomeMapPick.PickRoomHeroTrial(evt) || zbHomeMapPick.IsPickedFloorUnlock(evt))
		{
			return;
		}
		zbRoomEmpty zbRoomEmpty2 = zbHomeMapPick.PickRoomCreate(evt);
		if (zbSelf.Instance.homeEditHandler.createRoomRecord != null)
		{
			if ((Object)(object)zbRoomEmpty2 != (Object)null)
			{
				OnRoomCreate(zbRoomEmpty2);
			}
			else if ((Object)(object)zbHomeMapPick.PickRoom(evt) != (Object)null)
			{
				zbUIMgr.Instance.BubbleInfo(string.Format(zbUtil.Language("lang.10571"), zbSelf.Instance.homeEditHandler.createRoomRecord.name));
			}
			return;
		}
		if ((Object)(object)zbRoomEmpty2 != (Object)null)
		{
			zbUIMgr.Instance.ShowHomeCreate();
			return;
		}
		zbRoom zbRoom2 = zbHomeMapPick.PickRoom(evt);
		if ((Object)(object)zbRoom2 != (Object)null && zbSelf.Instance.homeEditHandler.createRoomRecord == null && !zbVersionMgr.IsReleased(zbRoom2.roomRecord.preShow))
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10432"));
			return;
		}
		zbWorker pickWorker = zbHomeMapPick.PickWorker(evt);
		if ((Object)(object)pickWorker != (Object)null && zbSelf.Instance.homeEditHandler.createRoomRecord == null)
		{
			if (((IEnumerable<WorkerDetail>)zbServerData.sGameData.HomelandInfo.WorkerList).Any((WorkerDetail w) => w.WorkerId == pickWorker.m_homelandWorkerDetail.WorkerId))
			{
				zbUIMgr.Instance.ShowWorkerDetail(pickWorker.m_homelandWorkerDetail);
			}
			return;
		}
		zbMergeLeft zbMergeLeft2 = zbHomeMapPick.PickMergeLeft(evt);
		if ((Object)(object)zbMergeLeft2 != (Object)null)
		{
			RoomComposeData roomComposeData = zbMergeLeft2.GetRoomComposeData();
			zbWsMsgHandler.Instance.HomelandV2_RoomCompose_Req(roomComposeData.roomList, roomComposeData.x, roomComposeData.y);
			return;
		}
		zbMergeRight zbMergeRight2 = zbHomeMapPick.PickMergeRight(evt);
		if ((Object)(object)zbMergeRight2 != (Object)null)
		{
			RoomComposeData roomComposeData2 = zbMergeRight2.GetRoomComposeData();
			zbWsMsgHandler.Instance.HomelandV2_RoomCompose_Req(roomComposeData2.roomList, roomComposeData2.x, roomComposeData2.y);
		}
		else if ((Object)(object)zbHomeMapPick.PickWall(evt) != (Object)null)
		{
			zbLinkHelper.OpenWall();
		}
		else
		{
			if (!((Object)(object)zbRoom2 != (Object)null) || zbSelf.Instance.homeEditHandler.createRoomRecord != null)
			{
				return;
			}
			_ = zbRoom2.roomData.TypeId;
			if (zbRoom2.IsFightRoom())
			{
				zbLinkHelper.OpenWarRoom(zbRoom2.roomData.TypeId);
			}
			else if (zbRoom2.roomData.Level == 0)
			{
				zbUIMgr.Instance.ShowRoomDetail(zbRoom2.roomData);
			}
			else if (zbRoom2.roomData.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId)
			{
				if (zbSelf.Instance.worldHandler.CanWorldResIdle())
				{
					zbUIMgr.Instance.ShowWorldResourceCollect();
				}
				else
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10745"));
				}
			}
			else if (zbRoom2.roomData.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.SpecimenId)
			{
				zbLinkHelper.OpenHandbook();
			}
			else if (zbRoom2.roomData.RoomId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.CommRadarId)
			{
				zbUIMgr.Instance.ShowRoomDetail(zbRoom2.roomData);
			}
			else
			{
				zbUIMgr.Instance.ShowRoomDetail(zbRoom2.roomData);
			}
		}
	}

	private void OnEventUpdateHomelandInfo(object data)
	{
		zbHomeMgr.Instance.Refresh();
	}

	private void OnEventUpdateWorldlandInfo(object data)
	{
		foreach (zbRoom room in zbHomeMgr.Instance.roomMgr.roomList)
		{
			if (room.roomData.TypeId == zbServerData.sGameData.HomelandInfo.BaseInfo.RoomMark.HallId)
			{
				room.RefreshRoomState();
				break;
			}
		}
	}

	private void OnEventUpdateRoomHeroMgr(object data)
	{
		zbHomeMgr.Instance.roomHeroMgr.Refresh();
	}

	private void OnRoomCreate(zbRoomEmpty pickRoomCreate)
	{
		if (zbSelf.Instance.homelandHandler.GetRoomCountByType(zbSelf.Instance.homeEditHandler.createRoomRecord.id) >= zbSelf.Instance.homeEditHandler.createRoomRecord.maxNum)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10133"));
			return;
		}
		if (zbSelf.Instance.homeEditHandler.createRoomCostList != null)
		{
			int[][] createRoomCostList = zbSelf.Instance.homeEditHandler.createRoomCostList;
			foreach (int[] array in createRoomCostList)
			{
				if (zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)array[0], array[1]) < array[2])
				{
					zbUIMgr.Instance.ShowTip((EResourceType)array[0], array[1], array[2]);
					return;
				}
			}
		}
		int x = pickRoomCreate.x;
		if (zbSelf.Instance.homeEditHandler.createRoomRecord.length == 2)
		{
			if (pickRoomCreate.x == 3)
			{
				x = 2;
			}
			else if (pickRoomCreate.x == 6)
			{
				x = 5;
			}
		}
		else if (zbSelf.Instance.homeEditHandler.createRoomRecord.length == 3)
		{
			x = ((pickRoomCreate.x < 4) ? 1 : 4);
		}
		zbWsMsgHandler.Instance.HomelandV2_RoomCreate_Req(zbSelf.Instance.homeEditHandler.createRoomRecord.id, x, pickRoomCreate.y);
	}
}
