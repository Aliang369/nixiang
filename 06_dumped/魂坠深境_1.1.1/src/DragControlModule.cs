using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragControlModule : InputModuleBase
{
	public Action<DragControlModule> OnCardBeginDrag;

	public Action<DragControlModule, PointerEventData> OnCardDrag;

	public Action<DragControlModule, PointerEventData> OnCardEndDrag;

	public Action<DragControlModule> OnCardClicked;

	public Action<DragControlModule, bool> OnCardSelected;

	public Action<DragControlModule, Transform, bool> OnCardPlaced;

	public Action<DragControlModule> OnCardReturned;

	public Action<DragControlModule, Transform> OnCardRemoved;

	private Func<Transform, bool> canDropValidator;

	private Func<Transform, bool> isSlotFullValidator;

	private bool isDragging;

	private Vector3 originalPosition;

	private Transform originalParent;

	private CanvasGroup canvasGroup;

	[SerializeField]
	private float dragThreshold = 5f;

	[SerializeField]
	private bool enableDrag = true;

	[SerializeField]
	private bool enableClick = true;

	private bool isSelected;

	public ItemDisplayBase itemDisplay => parent as ItemDisplayBase;

	public bool IsSelected => isSelected;

	public bool IsDragging => isDragging;

	public Transform OriginalParent => originalParent;

	public override void Init()
	{
		base.Init();
		canvasGroup = GetComponent<CanvasGroup>();
		if ((UnityEngine.Object)(object)canvasGroup == null)
		{
			canvasGroup = base.gameObject.AddComponent<CanvasGroup>();
		}
		isDragging = false;
		isSelected = false;
		SetEnable(EventType.BeginDrag, enable: true);
		SetEnable(EventType.Drag, enable: true);
		SetEnable(EventType.EndDrag, enable: true);
		SetEnable(EventType.PointerClick, enable: true);
		onHandleInput = (Action<InputModuleBase>)Delegate.Combine(onHandleInput, new Action<InputModuleBase>(OnHandleInput));
	}

	public override void Setup()
	{
		base.Setup();
	}

	public override void Dispose()
	{
		base.Dispose();
		OnCardBeginDrag = null;
		OnCardDrag = null;
		OnCardEndDrag = null;
		OnCardClicked = null;
		OnCardSelected = null;
		OnCardPlaced = null;
		OnCardReturned = null;
		OnCardRemoved = null;
		onHandleInput = (Action<InputModuleBase>)Delegate.Remove(onHandleInput, new Action<InputModuleBase>(OnHandleInput));
	}

	private void OnHandleInput(InputModuleBase module)
	{
		if (enableDrag && !(itemDisplay == null) && itemDisplay.item != null)
		{
			if (base.beginDrag)
			{
				HandleBeginDrag();
			}
			if (base.drag)
			{
				HandleDrag();
			}
			if (base.endDrag)
			{
				HandleEndDrag();
			}
			if (base.pointerClicked)
			{
				HandleClick();
			}
		}
	}

	public void SetSlotValidators(Func<Transform, bool> canDropValidator, Func<Transform, bool> isSlotFullValidator)
	{
		this.canDropValidator = canDropValidator;
		this.isSlotFullValidator = isSlotFullValidator;
	}

	private void HandleBeginDrag()
	{
		originalPosition = base.transform.localPosition;
		originalParent = base.transform.parent;
		isDragging = true;
		canvasGroup.blocksRaycasts = false;
		canvasGroup.alpha = 0.8f;
		OnCardBeginDrag?.Invoke(this);
	}

	private void HandleDrag()
	{
		if (isDragging && !(itemDisplay == null))
		{
			base.transform.position = eventData.position;
			OnCardDrag?.Invoke(this, eventData);
		}
	}

	private void HandleEndDrag()
	{
		if (!isDragging || itemDisplay == null)
		{
			return;
		}
		isDragging = false;
		canvasGroup.blocksRaycasts = true;
		canvasGroup.alpha = 1f;
		Transform transform = null;
		bool flag = false;
		if (eventData.pointerCurrentRaycast.gameObject != null)
		{
			GameObject gameObject = eventData.pointerCurrentRaycast.gameObject;
			DragControlModule componentInParent = gameObject.GetComponentInParent<DragControlModule>();
			ItemDisplayBase componentInParent2 = gameObject.GetComponentInParent<ItemDisplayBase>();
			if (componentInParent != null && componentInParent != this)
			{
				transform = componentInParent.transform.parent;
			}
			else if (componentInParent2 != null)
			{
				transform = componentInParent2.transform;
			}
			else
			{
				Transform transform2 = gameObject.transform;
				while (transform2 != null)
				{
					if (canDropValidator != null && canDropValidator(transform2))
					{
						transform = transform2;
						break;
					}
					transform2 = transform2.parent;
				}
			}
		}
		if (transform != null && transform != originalParent && (canDropValidator == null || canDropValidator(transform)) && (isSlotFullValidator == null || !isSlotFullValidator(transform)))
		{
			base.transform.SetParent(transform);
			base.transform.localPosition = Vector3.zero;
			flag = true;
			OnCardPlaced?.Invoke(this, transform, arg3: true);
		}
		if (!flag)
		{
			base.transform.SetParent(originalParent);
			base.transform.localPosition = originalPosition;
			OnCardReturned?.Invoke(this);
		}
		OnCardEndDrag?.Invoke(this, eventData);
	}

	private void HandleClick()
	{
		if (enableClick && !isDragging && !(itemDisplay == null))
		{
			isSelected = !isSelected;
			OnCardClicked?.Invoke(this);
			OnCardSelected?.Invoke(this, isSelected);
		}
	}

	public void SetSelected(bool selected)
	{
		isSelected = selected;
		OnCardSelected?.Invoke(this, isSelected);
	}

	public void SetEnableDrag(bool enable)
	{
		enableDrag = enable;
	}

	public void SetEnableClick(bool enable)
	{
		enableClick = enable;
	}

	public Item GetItem()
	{
		return itemDisplay?.item;
	}

	public void RemoveCard()
	{
		if (!(itemDisplay == null))
		{
			OnCardRemoved?.Invoke(this, base.transform.parent);
		}
	}

	public bool MoveToParent(Transform newParent)
	{
		if (newParent == null)
		{
			return false;
		}
		if (canDropValidator != null && !canDropValidator(newParent))
		{
			return false;
		}
		if (isSlotFullValidator != null && isSlotFullValidator(newParent))
		{
			return false;
		}
		_ = base.transform.parent;
		base.transform.SetParent(newParent);
		base.transform.localPosition = Vector3.zero;
		OnCardPlaced?.Invoke(this, newParent, arg3: true);
		return true;
	}

	public void ReturnToOriginalPosition()
	{
		if (originalParent != null)
		{
			base.transform.SetParent(originalParent);
			base.transform.localPosition = originalPosition;
			OnCardReturned?.Invoke(this);
		}
	}
}
