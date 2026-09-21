using UnityEngine;
using UnityEngine.EventSystems;

public class SelectBlocker : MonoBehaviour, ISelectHandler, IEventSystemHandler, IInputEventBlocker
{
	[SerializeField]
	private ReferenceInputModule implementation;

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		implementation.OnSelect(eventData);
	}
}
