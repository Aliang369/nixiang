using UnityEngine;
using UnityEngine.EventSystems;

public class PointerUpBlocker : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		implementation.OnPointerUp(eventData);
	}
}
