using UnityEngine;
using UnityEngine.EventSystems;

public class PointerEnterBlocker : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		implementation.OnPointerEnter(eventData);
	}
}
