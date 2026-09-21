using System;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

public sealed class RecycleObsessionSelectionDisplay : MonoBehaviour
{
	[NonSerialized]
	private GlobalItemDisplay itemDisplay;

	[NonSerialized]
	private InputModule inputModule;

	[NonSerialized]
	private Action<GlobalItemDisplay> onClick;

	[NonSerialized]
	private GameObject selectionDimmer;

	[NonSerialized]
	private GameObject selectionMark;

	public void Configure(Action<GlobalItemDisplay> clickHandler, bool selected, Sprite markSprite)
	{
		EnsureVisuals(markSprite);
		if ((object)itemDisplay == null)
		{
			itemDisplay = GetComponent<GlobalItemDisplay>();
		}
		InfoModule module = itemDisplay.GetModule<InfoModule>();
		if (module != null)
		{
			module.infoPanel = null;
		}
		if ((object)inputModule == null)
		{
			inputModule = itemDisplay.GetModule<InputModule>();
		}
		if (inputModule != null && onClick == null)
		{
			InputModule obj = inputModule;
			obj.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(obj.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
		}
		onClick = clickHandler;
		SetSelected(selected);
	}

	private void EnsureVisuals(Sprite markSprite)
	{
		if (selectionDimmer == null)
		{
			Transform transform = base.transform.Find("SelectionDimmer");
			if (transform == null)
			{
				GameObject obj = new GameObject("SelectionDimmer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				obj.layer = base.gameObject.layer;
				obj.transform.SetParent(base.transform, worldPositionStays: false);
				transform = obj.transform;
				RectTransform obj2 = (RectTransform)transform;
				obj2.anchorMin = Vector2.zero;
				obj2.anchorMax = Vector2.one;
				obj2.offsetMin = Vector2.zero;
				obj2.offsetMax = Vector2.zero;
				Image component = obj.GetComponent<Image>();
				component.color = new Color(0f, 0f, 0f, 0.45f);
				component.raycastTarget = false;
			}
			selectionDimmer = transform.gameObject;
		}
		if (selectionMark == null)
		{
			Transform transform2 = base.transform.Find("SelectionMark");
			if (transform2 == null)
			{
				GameObject obj3 = new GameObject("SelectionMark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
				obj3.layer = base.gameObject.layer;
				obj3.transform.SetParent(base.transform, worldPositionStays: false);
				transform2 = obj3.transform;
			}
			RectTransform obj4 = (RectTransform)transform2;
			obj4.anchorMin = Vector2.one;
			obj4.anchorMax = Vector2.one;
			obj4.pivot = Vector2.one;
			obj4.anchoredPosition = new Vector2(-4f, -4f);
			obj4.sizeDelta = new Vector2(36f, 36f);
			Image component2 = transform2.GetComponent<Image>();
			component2.sprite = markSprite;
			component2.color = Color.white;
			component2.preserveAspect = true;
			component2.raycastTarget = false;
			selectionMark = transform2.gameObject;
			selectionMark.SetActive(value: false);
		}
		selectionDimmer.transform.SetAsLastSibling();
		selectionMark.transform.SetAsLastSibling();
	}

	public void SetSelected(bool selected)
	{
		if (selectionDimmer != null)
		{
			selectionDimmer.SetActive(!selected);
		}
		if (selectionMark != null)
		{
			selectionMark.SetActive(selected);
		}
	}

	private void OnHandleInput(InputModuleBase input)
	{
		if (input.pointerClicked && !(input.lastPointerDownTime < input.lastDragTime))
		{
			onClick?.Invoke(itemDisplay);
		}
	}

	private void OnDestroy()
	{
		if (inputModule != null)
		{
			InputModule obj = inputModule;
			obj.onHandleInput = (Action<InputModuleBase>)Delegate.Remove(obj.onHandleInput, new Action<InputModuleBase>(OnHandleInput));
		}
	}
}
