using UnityEngine;
using UnityEngine.EventSystems;

public class BeginDragBlocker : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		implementation.OnBeginDrag(eventData);
	}
}
