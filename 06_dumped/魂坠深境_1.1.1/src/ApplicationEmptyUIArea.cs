using UnityEngine.EventSystems;

public class ApplicationEmptyUIArea : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public void OnPointerClick(PointerEventData eventData)
	{
		UIHelper.HideAllInfo(base.transform);
		UIBase.applicationState.eventManager?.UIEmptyClickedEvent.TriggerEvent();
	}
}
