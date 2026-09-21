using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectDragger : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private ScrollRect scrollRect;

	private void Awake()
	{
		scrollRect = GetComponent<ScrollRect>();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
	}

	public void OnDrag(PointerEventData eventData)
	{
		UpdateDragArea(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		FinishDragArea(eventData);
	}

	public void FinishDragArea(PointerEventData eventData)
	{
		if (!(scrollRect == null))
		{
			scrollRect.movementType = ScrollRect.MovementType.Elastic;
		}
	}

	public void UpdateDragArea(PointerEventData dragData)
	{
		if (!(scrollRect == null))
		{
			scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
			float num = Mathf.Max(1f, scrollRect.content.rect.height - scrollRect.viewport.rect.height);
			float num2 = dragData.delta.y / num;
			scrollRect.verticalNormalizedPosition -= num2;
		}
	}
}
