using UnityEngine;
using UnityEngine.EventSystems;

public class PointerClickBlocker : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		implementation?.OnPointerClick(eventData);
	}
}
