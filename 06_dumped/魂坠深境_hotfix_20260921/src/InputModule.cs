using UnityEngine;
using UnityEngine.EventSystems;

public class InputModule : InputModuleBase, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, ISelectHandler, IDeselectHandler
{
	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnBeginDrag(" + base.gameObject.name + ")");
		}
		OnBeginDrag(eventData);
	}

	void IDragHandler.OnDrag(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnDrag(" + base.gameObject.name + ")");
		}
		OnDrag(eventData);
	}

	void IEndDragHandler.OnEndDrag(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnEndDrag(" + base.gameObject.name + ")");
		}
		OnEndDrag(eventData);
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnPointerClick(" + base.gameObject.name + ")");
		}
		OnPointerClick(eventData);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnPointerDown(" + base.gameObject.name + ")");
		}
		OnPointerDown(eventData);
	}

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnPointerUp(" + base.gameObject.name + ")");
		}
		OnPointerUp(eventData);
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnPointerEnter(" + base.gameObject.name + ")");
		}
		OnPointerEnter(eventData);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnPointerExit(" + base.gameObject.name + ")");
		}
		OnPointerExit(eventData);
	}

	void ISelectHandler.OnSelect(BaseEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnSelect(" + base.gameObject.name + ")");
		}
		OnSelect(eventData);
	}

	void IDeselectHandler.OnDeselect(BaseEventData eventData)
	{
		if (debug)
		{
			Debug.LogError("OnDeselect(" + base.gameObject.name + ")");
		}
		OnDeselect(eventData);
	}
}
