using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectExtendEvent : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler
{
	private ScrollRect scrollRect => GetComponentInParent<ScrollRect>();

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (scrollRect != null)
		{
			scrollRect.OnBeginDrag(eventData);
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (scrollRect != null)
		{
			scrollRect.OnDrag(eventData);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (scrollRect != null)
		{
			scrollRect.OnEndDrag(eventData);
		}
	}
}
