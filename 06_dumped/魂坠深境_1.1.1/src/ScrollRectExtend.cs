using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScrollRectExtend : ScrollRect
{
	public class ScrollRectExtendEvent : UnityEvent<PointerEventData>
	{
	}

	public class PointerData
	{
		public int index;

		public Vector3 position;
	}

	[SerializeField]
	private List<ScrollRect> transferScrollRects;

	[SerializeField]
	private ScrollRectGridLayoutGroup contentGroup;

	[SerializeField]
	private GameObject topMask;

	[SerializeField]
	private GameObject buttonMask;

	[SerializeField]
	private bool enableScale;

	[SerializeField]
	private float minScale;

	[SerializeField]
	private float maxScale;

	[NonSerialized]
	private bool enableVerticalMask;

	[NonSerialized]
	private int dragCount;

	[NonSerialized]
	private List<PointerData> dragDatas = new List<PointerData>();

	[NonSerialized]
	protected ScrollRectExtendEvent onBeginDrag = new ScrollRectExtendEvent();

	[NonSerialized]
	protected ScrollRectExtendEvent onDrag = new ScrollRectExtendEvent();

	[NonSerialized]
	protected ScrollRectExtendEvent onEndDrag = new ScrollRectExtendEvent();

	protected override void Awake()
	{
		base.Awake();
		dragCount = 0;
		transferScrollRects = new List<ScrollRect>();
		if (contentGroup == null)
		{
			contentGroup = base.content.GetComponent<ScrollRectGridLayoutGroup>();
		}
		enableVerticalMask = topMask != null && buttonMask != null;
		if (enableVerticalMask)
		{
			topMask?.gameObject.SetActive(value: false);
			buttonMask?.gameObject.SetActive(value: false);
			base.onValueChanged.AddListener(UpdateVerticalMask);
		}
		if (contentGroup != null)
		{
			base.onValueChanged.AddListener(delegate
			{
				contentGroup?.RefreshVisibility();
			});
		}
	}

	public override void OnInitializePotentialDrag(PointerEventData eventData)
	{
		base.OnInitializePotentialDrag(eventData);
		foreach (ScrollRect transferScrollRect in transferScrollRects)
		{
			transferScrollRect.OnInitializePotentialDrag(eventData);
		}
	}

	public override void OnScroll(PointerEventData eventData)
	{
		if (!GameButton.enableAll)
		{
			return;
		}
		if (enableScale)
		{
			Vector3 localScale = base.transform.localScale;
			localScale.x += eventData.scrollDelta.y * 0.1f;
			localScale.y += eventData.scrollDelta.y * 0.1f;
			localScale.x = Mathf.Clamp(localScale.x, minScale, maxScale);
			localScale.y = Mathf.Clamp(localScale.y, minScale, maxScale);
			base.transform.localScale = localScale;
		}
		foreach (ScrollRect transferScrollRect in transferScrollRects)
		{
			transferScrollRect.OnScroll(eventData);
		}
	}

	public override void OnBeginDrag(PointerEventData eventData)
	{
		if (!GameButton.enableAll)
		{
			return;
		}
		dragCount++;
		base.OnBeginDrag(eventData);
		onBeginDrag?.Invoke(eventData);
		dragDatas.Add(new PointerData
		{
			index = eventData.pointerId,
			position = eventData.position
		});
		foreach (ScrollRect transferScrollRect in transferScrollRects)
		{
			transferScrollRect.OnBeginDrag(eventData);
		}
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (!GameButton.enableAll)
		{
			return;
		}
		base.OnDrag(eventData);
		if (enableScale && dragCount == 2)
		{
			_ = dragDatas.Count;
			_ = 2;
		}
		onDrag?.Invoke(eventData);
		foreach (ScrollRect transferScrollRect in transferScrollRects)
		{
			transferScrollRect.OnDrag(eventData);
		}
	}

	public override void OnEndDrag(PointerEventData eventData)
	{
		if (!GameButton.enableAll)
		{
			return;
		}
		dragCount--;
		base.OnEndDrag(eventData);
		onEndDrag?.Invoke(eventData);
		dragDatas.RemoveAll((PointerData p) => p.index == eventData.pointerId);
		foreach (ScrollRect transferScrollRect in transferScrollRects)
		{
			transferScrollRect.OnEndDrag(eventData);
		}
	}

	private void UpdateVerticalMask(Vector2 value)
	{
		if (enableVerticalMask)
		{
			topMask.SetActive(value.y <= 1f && base.content.anchoredPosition.y > 1f);
			buttonMask.SetActive(value.y >= 0f);
			Vector3[] worldCorners = base.viewport.GetWorldCorners();
			Vector3[] worldCorners2 = base.content.GetWorldCorners();
			topMask.SetActive(worldCorners[1].y < worldCorners2[1].y);
			buttonMask.SetActive(worldCorners[0].y > worldCorners2[0].y);
		}
	}
}
