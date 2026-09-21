using UnityEngine;
using UnityEngine.EventSystems;

public class PointerDownBlocker : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		implementation.OnPointerDown(eventData);
	}
}
