using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextMeshProUGUIPointerEvent : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerMoveHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private bool passPointerClick;

	[NonSerialized]
	public Action<int> onClicked;

	[NonSerialized]
	public Action<int> onEnter;

	[NonSerialized]
	public Action onExit;

	[NonSerialized]
	private TextMeshProUGUI text;

	[NonSerialized]
	private int currentOver;

	private void Awake()
	{
		currentOver = -1;
		text = GetComponent<TextMeshProUGUI>();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(position: new Vector3(eventData.position.x, eventData.position.y, 0f), text: text, camera: null);
		if (num > -1)
		{
			onClicked?.Invoke(num);
		}
		if (passPointerClick)
		{
			this.PassPointerClick(eventData);
		}
	}

	void IPointerMoveHandler.OnPointerMove(PointerEventData eventData)
	{
		if (PlatformHelper.IsMobile())
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(position: new Vector3(eventData.position.x, eventData.position.y, 0f), text: text, camera: null);
		if (currentOver != num)
		{
			if (currentOver != -1)
			{
				onExit?.Invoke();
			}
			currentOver = num;
			if (currentOver != -1)
			{
				onEnter?.Invoke(currentOver);
			}
		}
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		if (PlatformHelper.IsMobile())
		{
			return;
		}
		int num = TMP_TextUtilities.FindIntersectingLink(position: new Vector3(eventData.position.x, eventData.position.y, 0f), text: text, camera: null);
		if (currentOver != num)
		{
			if (currentOver != -1)
			{
				onExit?.Invoke();
			}
			currentOver = num;
			if (currentOver != -1)
			{
				onEnter?.Invoke(currentOver);
			}
		}
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		if (!PlatformHelper.IsMobile() && currentOver != -1)
		{
			onExit?.Invoke();
			currentOver = -1;
		}
	}
}
