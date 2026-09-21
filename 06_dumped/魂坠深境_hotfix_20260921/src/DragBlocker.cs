using UnityEngine;
using UnityEngine.EventSystems;

public class DragBlocker : MonoBehaviour, IDragHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		implementation.OnDrag(eventData);
	}
}
