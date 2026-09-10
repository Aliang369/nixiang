using UnityEngine;
using UnityEngine.EventSystems;

public class EmptyUIArea : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		if (GameButton.enableAll)
		{
			UIHelper.HideAllInfo(base.transform);
			if (Bootstrap.applicationState?.eventManager != null)
			{
				Bootstrap.applicationState.eventManager.UIEmptyClickedEvent.TriggerEvent();
			}
		}
	}
}
