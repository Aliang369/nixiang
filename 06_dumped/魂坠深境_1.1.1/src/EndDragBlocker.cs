using UnityEngine;
using UnityEngine.EventSystems;

public class EndDragBlocker : MonoBehaviour, IEndDragHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		implementation.OnEndDrag(eventData);
	}
}
