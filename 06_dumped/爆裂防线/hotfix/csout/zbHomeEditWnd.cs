using System;
using System.Collections.Generic;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbHomeEditWnd : zbBaseWnd
{
	public List<zbHomeEditFloor> floorList;

	public List<zbHomeEditRoom> roomList;

	public zbHomeEditDummyRoom dummyRoom;

	public zbHomeEditRoomAnim m_roomAnim;

	public VisualElement rootContainer;

	public VisualElement zoomContent;

	private Button m_btnClose;

	public Button m_btnBag;

	private Label m_lbCount;

	private Button m_btnSave;

	private Vector2 panLimitMin;

	private Vector2 panLimitMax;

	private const float zoomMin = 0.45f;

	private const float zoomMax = 1.2f;

	private Vector3 boundaryMoveDirection;

	private bool isAtBoundary;

	private Vector2 rootSize;

	private Vector2 zoomSize;

	private Dictionary<int, Vector3> touchPoints;

	private float initialTouchDistance;

	private float initialZoomFactor;

	private zbHomeEditRoom currPickedRoom;

	private bool isDraggingRoom;

	private bool isPanningZoomContent;

	private bool isPointerDownOnZoomContent;

	private bool isPointDown;

	private Vector3 pointerDownPos;

	private Vector3 panStartPos;

	private const float dragThreshold = 10f;

	private bool isPanning;

	private float curScale;

	protected override void Awake()
	{
		base.Awake();
		for (int num = 3; num >= -8; num--)
		{
			zbHomeEditFloor item = new zbHomeEditFloor(this, num);
			floorList.Add(item);
		}
		dummyRoom = new zbHomeEditDummyRoom(this);
		m_roomAnim = new zbHomeEditRoomAnim(this);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		foreach (zbHomeEditFloor floor in floorList)
		{
			floor.OnEnable();
		}
		dummyRoom.OnEnable();
		m_roomAnim.OnEnable();
		rootContainer = UQueryExtensions.Q(m_RootElement, "root", (string)null);
		zoomContent = UQueryExtensions.Q(m_RootElement, "zoomContent", (string)null);
		m_btnClose = UQueryExtensions.Q<Button>(m_RootElement, "btn_back", (string)null);
		m_btnBag = UQueryExtensions.Q<Button>(m_RootElement, "button_bag", (string)null);
		m_lbCount = UQueryExtensions.Q<Label>(m_RootElement, "lb_num", (string)null);
		m_btnSave = UQueryExtensions.Q<Button>(m_RootElement, "btn_save", (string)null);
		m_uiEvent.RegisterCallback<PointerDownEvent>(rootContainer, (Action<PointerDownEvent>)OnPointerDown, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerMoveEvent>(rootContainer, (Action<PointerMoveEvent>)OnPointerMove, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<PointerUpEvent>(rootContainer, (Action<PointerUpEvent>)OnPointerUp, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<WheelEvent>(rootContainer, (Action<WheelEvent>)OnWheelZoom, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnClose, (Action<ClickEvent>)OnClose, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnBag, (Action<ClickEvent>)OnBag, "", (TrickleDown)0);
		m_uiEvent.RegisterCallback<ClickEvent>((VisualElement)(object)m_btnSave, (Action<ClickEvent>)OnSave, "", (TrickleDown)0);
		touchPoints.Clear();
		m_gameEevent.RegisterEvent(EGameEvent.Event_HomelandRoomEdit_Ack, OnRefreshAck);
	}

	public override void Refresh()
	{
		zbSelf.Instance.homeEditHandler.Init();
		RefreshDetail();
	}

	public void RefreshDetail()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbHomeEditFloor floor in floorList)
		{
			floor.Refresh();
		}
		((TextElement)m_lbCount).text = zbSelf.Instance.homeEditHandler.roomBagList.Count.ToString();
		if (zbSelf.Instance.homeEditHandler.GetEditedRoomList().Length != 0 && zbSelf.Instance.homeEditHandler.roomBagList.Count <= 0)
		{
			((VisualElement)m_btnSave).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
		}
		else
		{
			((VisualElement)m_btnSave).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	protected override void UpdateAtShow()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		base.UpdateAtShow();
		if (isAtBoundary)
		{
			float num = 1000f;
			Vector3 val = boundaryMoveDirection * Time.deltaTime * num;
			ApplyPan(zoomContent.transform.position + val);
		}
	}

	private void OnPointerDown(PointerDownEvent evt)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.mouse)
		{
			OnSinglePointerDownImpl(evt);
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
		}
		else
		{
			if (!(((PointerEventBase<PointerDownEvent>)(object)evt).pointerType == PointerType.touch))
			{
				return;
			}
			PointerCaptureHelper.CapturePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerDownEvent>)(object)evt).pointerId);
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
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
		}
		else if (((PointerEventBase<PointerUpEvent>)(object)evt).pointerType == PointerType.touch)
		{
			PointerCaptureHelper.ReleasePointer((IEventHandler)(object)rootContainer, ((PointerEventBase<PointerUpEvent>)(object)evt).pointerId);
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
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		float num = (0f - evt.delta.y) * 0.01f;
		Scale scale = zoomContent.resolvedStyle.scale;
		float num2 = ((Scale)(ref scale)).value.x + num;
		num2 = Mathf.Clamp(num2, 0.45f, 1.2f);
		Vector3 zoomCenterInRoot = Vector2.op_Implicit(((MouseEventBase<WheelEvent>)(object)evt).localMousePosition);
		ApplyZoom(num2, zoomCenterInRoot);
	}

	private void OnSinglePointerDownImpl(PointerDownEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = (pointerDownPos = ((PointerEventBase<PointerDownEvent>)(object)evt).position);
		panStartPos = zoomContent.transform.position;
		isPointDown = true;
		currPickedRoom = null;
		isDraggingRoom = false;
		isPanningZoomContent = false;
		isPointerDownOnZoomContent = false;
		isPanning = false;
		zbHomeEditRoom zbHomeEditRoom2 = PickRoomAt(Vector2.op_Implicit(val));
		if (zbHomeEditRoom2 != null && zbHomeEditRoom2.roomData != null)
		{
			currPickedRoom = zbHomeEditRoom2;
		}
		if (currPickedRoom == null || !currPickedRoom.canEdit)
		{
			isPointerDownOnZoomContent = true;
		}
	}

	private void OnSinglePointerMoveImpl(PointerMoveEvent evt)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((PointerEventBase<PointerMoveEvent>)(object)evt).position - pointerDownPos;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (currPickedRoom != null && currPickedRoom.canEdit)
		{
			if (!isDraggingRoom && currPickedRoom.IsEditing())
			{
				currPickedRoom.SetVisible(visible: false);
				isDraggingRoom = true;
				dummyRoom.BeginMove(currPickedRoom, ((PointerEventBase<PointerMoveEvent>)(object)evt).localPosition);
			}
			if (isDraggingRoom)
			{
				dummyRoom.DragMove(((PointerEventBase<PointerMoveEvent>)(object)evt).localPosition);
			}
			if (!isDraggingRoom && magnitude > 10f && !isPanningZoomContent)
			{
				isPanningZoomContent = true;
				pointerDownPos = ((PointerEventBase<PointerMoveEvent>)(object)evt).position;
				panStartPos = zoomContent.transform.position;
			}
		}
		if ((isPointerDownOnZoomContent || isPanningZoomContent) && !isAtBoundary)
		{
			Vector3 val2 = ((PointerEventBase<PointerMoveEvent>)(object)evt).position - pointerDownPos;
			Vector3 pan = panStartPos + val2;
			ApplyPan(pan);
			isPanning = true;
		}
		if (currPickedRoom != null && currPickedRoom.isEditing && isPointDown && magnitude > 10f)
		{
			CheckBoundaryMove(Vector2.op_Implicit(((PointerEventBase<PointerMoveEvent>)(object)evt).position));
		}
	}

	private void CheckBoundaryMove(Vector2 uiPos)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		float num = Screen.width;
		float num2 = Screen.height;
		float num3 = num * 0.15f;
		float num4 = num2 * 0.15f;
		Vector2 val = zbUtil.UIToolkitToScreen(uiPos, rootContainer);
		Vector3 zero = Vector3.zero;
		if (val.x < num3)
		{
			zero.x = 1f;
		}
		else if (val.x > num - num3)
		{
			zero.x = -1f;
		}
		if (val.y > num2 - num4)
		{
			zero.y = 1f;
		}
		else if (val.y < num4)
		{
			zero.y = -1f;
		}
		if (zero == Vector3.zero)
		{
			isAtBoundary = false;
		}
		else
		{
			isAtBoundary = true;
		}
		boundaryMoveDirection = zero;
	}

	private void OnPointerUpImpl(PointerUpEvent evt)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		isPointDown = false;
		isAtBoundary = false;
		boundaryMoveDirection = Vector3.zero;
		Vector3 position = ((PointerEventBase<PointerUpEvent>)(object)evt).position;
		zbHomeEditRoom zbHomeEditRoom2 = PickRoomAt(Vector2.op_Implicit(position));
		if (isDraggingRoom || (!isPanning && zbSelf.Instance.homeEditHandler.curEditRoomId > 0 && zbHomeEditRoom2 != null && zbHomeEditRoom2.roomData == null))
		{
			FinishRoomDrag(position);
			dummyRoom.EndMove();
		}
		else if (isPointerDownOnZoomContent || isPanningZoomContent)
		{
			if (currPickedRoom != null && !currPickedRoom.canEdit && !isPanning)
			{
				if (currPickedRoom.roomData.Level <= 0)
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10743"));
				}
				else
				{
					zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10105"));
				}
			}
			else if (currPickedRoom == null)
			{
				zbSelf.Instance.homeEditHandler.curEditRoomId = 0;
				RefreshDetail();
			}
		}
		else if (currPickedRoom != null && currPickedRoom.canEdit)
		{
			OnRoomClick(currPickedRoom);
		}
		if (currPickedRoom != null)
		{
			currPickedRoom.SetVisible(visible: true);
		}
		currPickedRoom = null;
		isDraggingRoom = false;
		isPanningZoomContent = false;
		isPointerDownOnZoomContent = false;
		isPanning = false;
	}

	private void OnDoublePointerDownImpl()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		initialTouchDistance = Vector2.Distance(Vector2.op_Implicit(current), Vector2.op_Implicit(current2));
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
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<int, Vector3>.ValueCollection.Enumerator enumerator = touchPoints.Values.GetEnumerator();
		enumerator.MoveNext();
		Vector3 current = enumerator.Current;
		enumerator.MoveNext();
		Vector3 current2 = enumerator.Current;
		float num = Vector2.Distance(Vector2.op_Implicit(current), Vector2.op_Implicit(current2)) / initialTouchDistance;
		float num2 = initialZoomFactor * num;
		num2 = Mathf.Clamp(num2, 0.45f, 1.2f);
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
		panLimitMin.x = rootSize.x - zoomSize.x * curScale - rootSize.x / 2f;
		panLimitMax.x = rootSize.x / 2f;
		panLimitMin.y = rootSize.y - zoomSize.y * curScale - rootSize.y / 2f;
		panLimitMax.y = rootSize.y / 2f;
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 centerPosZoomContent = GetCenterPosZoomContent(zoomCenterInRoot);
		curScale = newZoom;
		zoomContent.style.scale = StyleScale.op_Implicit(new Scale(new Vector2(newZoom, newZoom)));
		Vector3 centerPosZoomContent2 = GetCenterPosZoomContent(zoomCenterInRoot);
		Vector3 val = centerPosZoomContent - centerPosZoomContent2;
		Vector3 pan = zoomContent.transform.position - val * newZoom;
		ApplyPan(pan);
	}

	private List<zbHomeEditRoom> GetVisibleRooms()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		List<zbHomeEditRoom> list = new List<zbHomeEditRoom>();
		Rect worldBound = rootContainer.worldBound;
		foreach (zbHomeEditFloor floor in floorList)
		{
			foreach (zbHomeEditRoom room in floor.roomList)
			{
				if (room.roomRootElement != null)
				{
					Rect worldBound2 = room.roomRootElement.worldBound;
					if (!(((Rect)(ref worldBound2)).width <= 0f) && !(((Rect)(ref worldBound2)).height <= 0f) && ((Rect)(ref worldBound)).Overlaps(worldBound2))
					{
						list.Add(room);
					}
				}
			}
		}
		return list;
	}

	public zbHomeEditRoom GetCanSetRoom(int length)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (length < 1 || length > 3)
		{
			return null;
		}
		List<zbHomeEditRoom> visibleRooms = GetVisibleRooms();
		List<int[]> list = new List<int[]>();
		switch (length)
		{
		case 1:
			foreach (zbHomeEditRoom item in visibleRooms)
			{
				if (item.roomData == null && (int)item.roomRootElement.resolvedStyle.display == 0)
				{
					return item;
				}
			}
			break;
		case 2:
			list.Add(new int[2] { 1, 2 });
			list.Add(new int[2] { 2, 3 });
			list.Add(new int[2] { 4, 5 });
			list.Add(new int[2] { 5, 6 });
			break;
		case 3:
			list.Add(new int[3] { 1, 2, 3 });
			list.Add(new int[3] { 4, 5, 6 });
			break;
		}
		foreach (int[] item2 in list)
		{
			foreach (zbHomeEditRoom item3 in visibleRooms)
			{
				if (item3.x != item2[0] || item3.roomData != null)
				{
					continue;
				}
				bool flag = true;
				int y = item3.y;
				for (int i = 1; i < item2.Length; i++)
				{
					if (GetZoomData(item2[i], y) != null)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return item3;
				}
			}
		}
		return null;
	}

	private zbHomeEditRoom PickRoomAt(Vector2 mousePosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		foreach (zbHomeEditFloor floor in floorList)
		{
			zbHomeEditRoom zbHomeEditRoom2 = floor.PickRoomAt(mousePosition);
			if (zbHomeEditRoom2 != null)
			{
				return zbHomeEditRoom2;
			}
		}
		return null;
	}

	private void FinishRoomDrag(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		zbHomeEditRoom zbHomeEditRoom2 = PickRoomAt(Vector2.op_Implicit(position));
		if (zbHomeEditRoom2 != null && zbHomeEditRoom2.roomData == null && zbSelf.Instance.homeEditHandler.curEditRoomId > 0)
		{
			int editedRoomX = zbSelf.Instance.homeEditHandler.GetEditedRoomX(zbHomeEditRoom2.x, zbHomeEditRoom2.y);
			if (editedRoomX > 0)
			{
				zbSelf.Instance.homeEditHandler.RecordPositionBeforeChange(editedRoomX, zbHomeEditRoom2.y);
				zbSelf.Instance.homeEditHandler.UpdateRoomDetail(editedRoomX, zbHomeEditRoom2.y);
				RefreshDetail();
			}
		}
	}

	private void OnRoomClick(zbHomeEditRoom room)
	{
		if (zbSelf.Instance.homeEditHandler.curEditRoomId == 0 && room.roomData == null)
		{
			return;
		}
		if (zbSelf.Instance.homeEditHandler.curEditRoomId == 0 && room.roomData != null)
		{
			zbSelf.Instance.homeEditHandler.StartEditRoom(room.roomData);
			zbSelf.Instance.homeEditHandler.curEditRoomId = room.roomData.RoomId;
		}
		else if (zbSelf.Instance.homeEditHandler.curEditRoomId > 0 && room.roomData == null)
		{
			int editedRoomX = zbSelf.Instance.homeEditHandler.GetEditedRoomX(room.x, room.y);
			if (editedRoomX > 0)
			{
				zbSelf.Instance.homeEditHandler.RecordPositionBeforeChange(editedRoomX, room.y);
				zbSelf.Instance.homeEditHandler.UpdateRoomDetail(editedRoomX, room.y);
			}
		}
		else if (zbSelf.Instance.homeEditHandler.curEditRoomId > 0 && room.roomData != null)
		{
			zbSelf.Instance.homeEditHandler.StartEditRoom(room.roomData);
			zbSelf.Instance.homeEditHandler.curEditRoomId = room.roomData.RoomId;
		}
		RefreshDetail();
		zbAudioMgr.Instance.PlayAudio(zbAudioDefine.UI_CLICK);
	}

	public HomelandRoomDetail GetZoomData(int x, int y)
	{
		foreach (HomelandRoomDetail room in zbSelf.Instance.homeEditHandler.roomList)
		{
			if (room.X == x && room.Y == y)
			{
				return room;
			}
		}
		return null;
	}

	public int GetRoomLength(int x, int y)
	{
		HomelandRoomDetail zoomData = GetZoomData(x, y);
		if (zoomData != null)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(zoomData.TypeId);
			if (recordByKey != null)
			{
				return recordByKey.length;
			}
		}
		return 0;
	}

	public void ResfeshNextLocateRoom(Vector3 mousePos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		zbHomeEditRoom nextRoom = null;
		float num = Screen.height;
		Rect layout = rootContainer.layout;
		float num2 = num - ((Rect)(ref layout)).size.y;
		Scale scale = zoomContent.resolvedStyle.scale;
		float num3 = num2 * ((Scale)(ref scale)).value.y * 0.5f;
		zbHomeEditRoom zbHomeEditRoom2 = PickRoomAt(Vector2.op_Implicit(new Vector3(mousePos.x, mousePos.y + num3, mousePos.z)));
		if (zbHomeEditRoom2 != null && zbHomeEditRoom2.roomData == null && zbSelf.Instance.homeEditHandler.curEditRoomId > 0)
		{
			int editedRoomX = zbSelf.Instance.homeEditHandler.GetEditedRoomX(zbHomeEditRoom2.x, zbHomeEditRoom2.y);
			if (editedRoomX > 0)
			{
				foreach (zbHomeEditFloor floor in floorList)
				{
					foreach (zbHomeEditRoom room in floor.roomList)
					{
						if (room.x == editedRoomX && room.y == zbHomeEditRoom2.y)
						{
							nextRoom = room;
							break;
						}
					}
				}
			}
		}
		foreach (zbHomeEditFloor floor2 in floorList)
		{
			foreach (zbHomeEditRoom room2 in floor2.roomList)
			{
				room2.RefreshNextRoomFrame(nextRoom);
			}
		}
	}

	public void OnClose(ClickEvent evt)
	{
		HomelandRoomEditDetail[] editedRoomList = zbSelf.Instance.homeEditHandler.GetEditedRoomList();
		if (zbSelf.Instance.homeEditHandler.roomBagList.Count > 0)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10109"), zbUtil.Language("lang.10108"), OnMessageBoxConfirm);
		}
		else if (editedRoomList.Length != 0)
		{
			zbUIMgr.Instance.ShowMessageBox(zbUtil.Language("lang.10109"), zbUtil.Language("lang.10114"), OnMessageBoxConfirm);
		}
		else
		{
			Hide();
		}
	}

	private void OnMessageBoxConfirm(bool isOk)
	{
		if (isOk)
		{
			Hide();
		}
	}

	public void OnBag(ClickEvent evt)
	{
		zbUIMgr.Instance.ShowHomeEditBag();
	}

	public void OnSave(ClickEvent evt)
	{
		if (zbSelf.Instance.homeEditHandler.roomBagList.Count > 0)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.10110"));
			return;
		}
		HomelandRoomEditDetail[] editedRoomList = zbSelf.Instance.homeEditHandler.GetEditedRoomList();
		if (editedRoomList.Length > 1)
		{
			HomelandRoomEditDetail[] allRoomList = zbSelf.Instance.homeEditHandler.GetAllRoomList();
			zbWsMsgHandler.Instance.HomelandV2_RoomEdit_Req(allRoomList);
		}
		else if (editedRoomList.Length == 1)
		{
			zbWsMsgHandler.Instance.HomelandV2_RoomMove_Req(editedRoomList[0].RoomId, editedRoomList[0].X, editedRoomList[0].Y);
		}
	}

	private void OnRefreshAck(object data)
	{
		Hide();
	}

	public zbHomeEditWnd()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		floorList = new List<zbHomeEditFloor>();
		roomList = new List<zbHomeEditRoom>();
		panLimitMin = Vector2.zero;
		panLimitMax = Vector2.zero;
		boundaryMoveDirection = Vector3.zero;
		rootSize = new Vector2(720f, 1480f);
		zoomSize = new Vector2(1600f, 1980f);
		touchPoints = new Dictionary<int, Vector3>();
		curScale = 1f;
		base._002Ector();
	}
}
