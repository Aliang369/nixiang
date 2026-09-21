using System;
using Common.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class InputModuleBase : UIDisplayModule, ILateUpdate, IInputUpdate
{
	[Flags]
	public enum EventType
	{
		PointerDown = 1,
		PointerUp = 2,
		BeginDrag = 4,
		EndDrag = 8,
		Drag = 0x10,
		PointerEnter = 0x20,
		PointerExit = 0x40,
		PointerClick = 0x80,
		Select = 0x100,
		Deselect = 0x200
	}

	[SerializeField]
	protected EventType passes;

	[SerializeField]
	public EventType enables;

	[SerializeField]
	public bool enableRawOver;

	[SerializeField]
	public RectTransform rawOverTarget;

	[NonSerialized]
	public EventType data;

	[NonSerialized]
	public PointerEventData eventData;

	[NonSerialized]
	public Action<InputModuleBase> onHandleInput;

	[NonSerialized]
	public Action<InputModuleBase> onPointerClick;

	[NonSerialized]
	public bool debug;

	public DateTime lastPointerDownTime;

	public DateTime lastDragTime;

	public bool over;

	public bool rawOver;

	public bool pointerClicked
	{
		get
		{
			return data.HasFlag(EventType.PointerClick);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.PointerClick;
			}
			else
			{
				data &= ~EventType.PointerClick;
			}
		}
	}

	public bool pointerDown
	{
		get
		{
			return data.HasFlag(EventType.PointerDown);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.PointerDown;
			}
			else
			{
				data &= ~EventType.PointerDown;
			}
		}
	}

	public bool pointerEnter
	{
		get
		{
			return data.HasFlag(EventType.PointerEnter);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.PointerEnter;
			}
			else
			{
				data &= ~EventType.PointerEnter;
			}
		}
	}

	public bool pointerExit
	{
		get
		{
			return data.HasFlag(EventType.PointerExit);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.PointerExit;
			}
			else
			{
				data &= ~EventType.PointerExit;
			}
		}
	}

	public bool pointerUp
	{
		get
		{
			return data.HasFlag(EventType.PointerUp);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.PointerUp;
			}
			else
			{
				data &= ~EventType.PointerUp;
			}
		}
	}

	public bool beginDrag
	{
		get
		{
			return data.HasFlag(EventType.BeginDrag);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.BeginDrag;
			}
			else
			{
				data &= ~EventType.BeginDrag;
			}
		}
	}

	public bool drag
	{
		get
		{
			return data.HasFlag(EventType.Drag);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.Drag;
			}
			else
			{
				data &= ~EventType.Drag;
			}
		}
	}

	public bool endDrag
	{
		get
		{
			return data.HasFlag(EventType.EndDrag);
		}
		protected set
		{
			if (value)
			{
				data |= EventType.EndDrag;
			}
			else
			{
				data &= ~EventType.EndDrag;
			}
		}
	}

	public override void Setup()
	{
		over = false;
		ClearFrameInput();
	}

	void ILateUpdate.OnUpdate()
	{
		ClearFrameInput();
	}

	void IInputUpdate.UpdateInput()
	{
		CheckIsRawOver();
		UpdateFrameInput();
	}

	public bool CheckIsBlockedByInfo()
	{
		if (UIDisplayModule.gameState != null)
		{
			return UIDisplayModule.gameState.UIManager.pointerOverItemInfoDisplay != null;
		}
		if (UIDisplayModule.fortressState != null)
		{
			return UIDisplayModule.fortressState.uiManager.pointerOverItemInfoDisplay != null;
		}
		CustomRaycaster[] allRaycasters = GetAllRaycasters();
		if (allRaycasters == null)
		{
			return false;
		}
		CustomRaycaster customRaycaster = null;
		int num = -1;
		CustomRaycaster[] array = allRaycasters;
		foreach (CustomRaycaster customRaycaster2 in array)
		{
			for (int j = 0; j < customRaycaster2.m_RaycastResults.Count; j++)
			{
				if (IsMGraphic(customRaycaster2.m_RaycastResults[j]))
				{
					customRaycaster = customRaycaster2;
					num = j;
					break;
				}
			}
			if (customRaycaster != null)
			{
				customRaycaster = customRaycaster2;
				break;
			}
		}
		if (customRaycaster == null)
		{
			return false;
		}
		for (int k = num + 1; k < customRaycaster.m_RaycastResults.Count; k++)
		{
			if (IsItemInfo(customRaycaster.m_RaycastResults[k]))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsItemInfo(Graphic graphic)
	{
		Transform transform = graphic.transform;
		while (transform != null)
		{
			if (transform is IPointerEnterHandler)
			{
				return transform.GetComponent<ItemInfoDisplayBase>() != null;
			}
			transform = transform.parent;
		}
		return false;
	}

	private bool IsMGraphic(Graphic graphic)
	{
		Transform transform = graphic.transform;
		while (transform != null)
		{
			if (transform is IPointerEnterHandler)
			{
				return transform == base.transform;
			}
			transform = transform.parent;
		}
		return false;
	}

	private CustomRaycaster[] GetAllRaycasters()
	{
		if (UIDisplayModule.gameState != null)
		{
			return UIDisplayModule.gameState.UIManager.raycasters;
		}
		return null;
	}

	private void UpdateFrameInput()
	{
		if (base.gameObject.activeInHierarchy)
		{
			if (pointerDown)
			{
				EventSystem.current.SetSelectedGameObject(base.gameObject, eventData);
			}
			HandleInput();
		}
	}

	private void HandleInput()
	{
		if (pointerClicked)
		{
			try
			{
				onPointerClick?.Invoke(this);
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}
		try
		{
			onHandleInput?.Invoke(this);
		}
		catch (Exception message2)
		{
			Debug.LogError(message2);
		}
	}

	private void CheckIsRawOver()
	{
		if (base.gameObject.activeInHierarchy && enableRawOver)
		{
			RectTransform rectTransform = ((rawOverTarget == null) ? base.rectTransform : rawOverTarget);
			rawOver = RectTransformUtility.RectangleContainsScreenPoint(rectTransform, EventSystem.current.currentInputModule.input.mousePosition, GetCamera());
		}
	}

	private void ClearFrameInput()
	{
		data = (EventType)0;
		rawOver = false;
	}

	private T GetPassObject<T>()
	{
		Transform transform = base.transform.parent;
		while (transform != null)
		{
			T component = transform.GetComponent<T>();
			if (component != null)
			{
				return component;
			}
			transform = transform.parent;
		}
		return default(T);
	}

	private Camera GetCamera()
	{
		Transform transform = base.transform.parent;
		while (transform != null)
		{
			BaseRaycaster component = transform.GetComponent<BaseRaycaster>();
			if (component != null)
			{
				return component.eventCamera;
			}
			transform = transform.parent;
		}
		return null;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.BeginDrag))
		{
			GetPassObject<IBeginDragHandler>()?.OnBeginDrag(eventData);
		}
		if (enables.HasFlag(EventType.BeginDrag) && GameButton.enableAll)
		{
			beginDrag = true;
			this.eventData = eventData;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.Drag))
		{
			GetPassObject<IDragHandler>()?.OnDrag(eventData);
		}
		if (enables.HasFlag(EventType.Drag) && GameButton.enableAll)
		{
			drag = true;
			this.eventData = eventData;
			lastDragTime = DateTime.UtcNow;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.EndDrag))
		{
			GetPassObject<IEndDragHandler>()?.OnEndDrag(eventData);
		}
		if (enables.HasFlag(EventType.EndDrag) && GameButton.enableAll)
		{
			endDrag = true;
			this.eventData = eventData;
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.PointerClick))
		{
			GetPassObject<IPointerClickHandler>()?.OnPointerClick(eventData);
		}
		if (enables.HasFlag(EventType.PointerClick) && GameButton.enableAll)
		{
			pointerClicked = true;
			this.eventData = eventData;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.PointerDown))
		{
			GetPassObject<IPointerDownHandler>()?.OnPointerDown(eventData);
		}
		if (enables.HasFlag(EventType.PointerDown) && GameButton.enableAll)
		{
			pointerDown = true;
			this.eventData = eventData;
			lastPointerDownTime = DateTime.UtcNow;
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.PointerUp))
		{
			GetPassObject<IPointerUpHandler>()?.OnPointerUp(eventData);
		}
		if (enables.HasFlag(EventType.PointerUp) && GameButton.enableAll)
		{
			pointerUp = true;
			this.eventData = eventData;
			if ((float)(DateTime.Now - lastPointerDownTime).Seconds < 0.2f && Vector3.Distance(eventData.pressPosition, eventData.position) < 10f)
			{
				(this as IPointerClickHandler).OnPointerClick(eventData);
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.PointerEnter))
		{
			GetPassObject<IPointerEnterHandler>()?.OnPointerEnter(eventData);
		}
		if (enables.HasFlag(EventType.PointerEnter) && GameButton.enableAll)
		{
			pointerEnter = true;
			over = true;
			this.eventData = eventData;
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (passes.HasFlag(EventType.PointerExit))
		{
			GetPassObject<IPointerExitHandler>()?.OnPointerExit(eventData);
		}
		if (enables.HasFlag(EventType.PointerExit) && GameButton.enableAll)
		{
			pointerExit = true;
			over = false;
			this.eventData = eventData;
		}
	}

	public void OnSelect(BaseEventData eventData)
	{
	}

	public void OnDeselect(BaseEventData eventData)
	{
	}

	public void SetEnable(EventType eventType, bool enable)
	{
		if (enable)
		{
			enables |= eventType;
		}
		else
		{
			enables &= ~eventType;
		}
	}
}
