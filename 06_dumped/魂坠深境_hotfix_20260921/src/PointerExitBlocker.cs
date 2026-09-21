using UnityEngine;
using UnityEngine.EventSystems;

public class PointerExitBlocker : MonoBehaviour, IPointerExitHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		implementation.OnPointerExit(eventData);
	}
}
