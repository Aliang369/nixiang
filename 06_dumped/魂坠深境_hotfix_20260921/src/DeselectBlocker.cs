using UnityEngine;
using UnityEngine.EventSystems;

public class DeselectBlocker : MonoBehaviour, IDeselectHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		implementation.OnDeselect(eventData);
	}
}
