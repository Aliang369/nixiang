using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelScrollRect : UIBehaviour, IDragHandler, IEventSystemHandler, IBeginDragHandler, IEndDragHandler, ICanvasElement
{
	[SerializeField]
	private RectTransform m_Content;

	[SerializeField]
	private bool m_Horizontal = true;

	[SerializeField]
	private bool m_Vertical = true;

	[SerializeField]
	private ScrollRect.MovementType m_MovementType = ScrollRect.MovementType.Elastic;

	[SerializeField]
	private float m_Elasticity = 0.1f;

	[SerializeField]
	private bool m_Inertia = true;

	[SerializeField]
	private float m_DecelerationRate = 0.135f;

	[SerializeField]
	private float m_ScrollSensitivity = 1f;

	[SerializeField]
	private RectTransform m_Viewport;

	[SerializeField]
	private Scrollbar m_HorizontalScrollbar;

	[SerializeField]
	private Scrollbar m_VerticalScrollbar;

	[SerializeField]
	private ScrollRect.ScrollbarVisibility m_HorizontalScrollbarVisibility;

	[SerializeField]
	private ScrollRect.ScrollbarVisibility m_VerticalScrollbarVisibility;

	[SerializeField]
	private float m_HorizontalScrollbarSpacing;

	[SerializeField]
	private float m_VerticalScrollbarSpacing;

	[SerializeField]
	private ScrollRect.ScrollRectEvent m_OnValueChanged = new ScrollRect.ScrollRectEvent();

	[Header("Snapping Settings")]
	[SerializeField]
	private bool m_Snapping;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_SnappingPosition = 0.5f;

	[SerializeField]
	private float m_SnappingSpeed = 10f;

	[SerializeField]
	private float m_SnappingSpeedThreshold = 10f;

	[SerializeField]
	private RectTransform m_snappingTarget;

	[SerializeField]
	private bool m_SnappingScale;

	[SerializeField]
	private float m_SnappingScaleDistance = 200f;

	[SerializeField]
	private float m_MinSnappingScale = 1f;

	[SerializeField]
	private float m_MaxSnappingScale = 1.1f;

	[NonSerialized]
	private bool m_SnappingActive;

	[NonSerialized]
	public Func<RectTransform, bool> snappingElementFilter;

	[NonSerialized]
	public Action onEndDrag;

	[NonSerialized]
	private Vector2 m_PointerStartLocalCursor = Vector2.zero;

	[NonSerialized]
	protected Vector2 m_ContentStartPosition = Vector2.zero;

	[NonSerialized]
	protected Bounds m_ContentBounds;

	[NonSerialized]
	private Bounds m_ViewBounds;

	[NonSerialized]
	private Vector2 m_Velocity;

	[NonSerialized]
	private bool m_Dragging;

	[NonSerialized]
	private bool m_Scrolling;

	[NonSerialized]
	private Vector2 m_PrevPosition = Vector2.zero;

	[NonSerialized]
	private Bounds m_PrevContentBounds;

	[NonSerialized]
	private Bounds m_PrevViewBounds;

	[NonSerialized]
	private bool m_HasRebuiltLayout;

	[NonSerialized]
	private bool m_HSliderExpand;

	[NonSerialized]
	private bool m_VSliderExpand;

	[NonSerialized]
	private float m_HSliderHeight;

	[NonSerialized]
	private float m_VSliderWidth;

	[NonSerialized]
	private RectTransform m_Rect;

	[NonSerialized]
	private RectTransform m_HorizontalScrollbarRect;

	[NonSerialized]
	private RectTransform m_VerticalScrollbarRect;

	private DrivenRectTransformTracker m_Tracker;

	private readonly Vector3[] m_Corners = new Vector3[4];

	private RectTransform m_ViewRect => m_Viewport;

	private RectTransform rectTransform
	{
		get
		{
			if (m_Rect == null)
			{
				m_Rect = GetComponent<RectTransform>();
			}
			return m_Rect;
		}
	}

	public ScrollRect.ScrollRectEvent onValueChanged
	{
		get
		{
			return m_OnValueChanged;
		}
		set
		{
			m_OnValueChanged = value;
		}
	}

	public Vector2 normalizedPosition
	{
		get
		{
			return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
		}
		set
		{
			SetNormalizedPosition(value.x, 0);
			SetNormalizedPosition(value.y, 1);
		}
	}

	public float horizontalNormalizedPosition
	{
		get
		{
			UpdateBounds();
			if (m_ContentBounds.size.x <= m_ViewBounds.size.x || Mathf.Approximately(m_ContentBounds.size.x, m_ViewBounds.size.x))
			{
				return (m_ViewBounds.min.x > m_ContentBounds.min.x) ? 1 : 0;
			}
			return (m_ViewBounds.min.x - m_ContentBounds.min.x) / (m_ContentBounds.size.x - m_ViewBounds.size.x);
		}
		set
		{
			SetNormalizedPosition(value, 0);
		}
	}

	public float verticalNormalizedPosition
	{
		get
		{
			UpdateBounds();
			if (m_ContentBounds.size.y <= m_ViewBounds.size.y || Mathf.Approximately(m_ContentBounds.size.y, m_ViewBounds.size.y))
			{
				return (m_ViewBounds.min.y > m_ContentBounds.min.y) ? 1 : 0;
			}
			return (m_ViewBounds.min.y - m_ContentBounds.min.y) / (m_ContentBounds.size.y - m_ViewBounds.size.y);
		}
		set
		{
			SetNormalizedPosition(value, 1);
		}
	}

	private bool hScrollingNeeded
	{
		get
		{
			if (Application.isPlaying)
			{
				return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
			}
			return true;
		}
	}

	private bool vScrollingNeeded
	{
		get
		{
			if (Application.isPlaying)
			{
				return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
			}
			return true;
		}
	}

	public virtual float minWidth => -1f;

	public virtual float preferredWidth => -1f;

	public virtual float flexibleWidth => -1f;

	public virtual float minHeight => -1f;

	public virtual float preferredHeight => -1f;

	public virtual float flexibleHeight => -1f;

	public virtual int layoutPriority => -1;

	protected WheelScrollRect()
	{
	}

	public virtual void Rebuild(CanvasUpdate executing)
	{
		if (executing == CanvasUpdate.Prelayout)
		{
			UpdateCachedData();
		}
		if (executing == CanvasUpdate.PostLayout)
		{
			UpdateBounds();
			UpdateScrollbars(Vector2.zero);
			UpdatePrevData();
			m_HasRebuiltLayout = true;
		}
	}

	public virtual void LayoutComplete()
	{
	}

	public virtual void GraphicUpdateComplete()
	{
	}

	private void UpdateCachedData()
	{
		Transform transform = base.transform;
		m_HorizontalScrollbarRect = ((m_HorizontalScrollbar == null) ? null : (m_HorizontalScrollbar.transform as RectTransform));
		m_VerticalScrollbarRect = ((m_VerticalScrollbar == null) ? null : (m_VerticalScrollbar.transform as RectTransform));
		bool num = m_ViewRect.parent == transform;
		bool flag = !m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform;
		bool flag2 = !m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform;
		bool flag3 = num & flag & flag2;
		m_HSliderExpand = flag3 && (bool)m_HorizontalScrollbarRect && m_HorizontalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
		m_VSliderExpand = flag3 && (bool)m_VerticalScrollbarRect && m_VerticalScrollbarVisibility == ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
		m_HSliderHeight = ((m_HorizontalScrollbarRect == null) ? 0f : m_HorizontalScrollbarRect.rect.height);
		m_VSliderWidth = ((m_VerticalScrollbarRect == null) ? 0f : m_VerticalScrollbarRect.rect.width);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_Horizontal && (bool)m_HorizontalScrollbar)
		{
			m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
		}
		if (m_Vertical && (bool)m_VerticalScrollbar)
		{
			m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
		}
		CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
		SetDirty();
	}

	protected override void OnDisable()
	{
		CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);
		if ((bool)m_HorizontalScrollbar)
		{
			m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
		}
		if ((bool)m_VerticalScrollbar)
		{
			m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
		}
		m_Dragging = false;
		m_Scrolling = false;
		m_HasRebuiltLayout = false;
		m_Tracker.Clear();
		m_Velocity = Vector2.zero;
		LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		base.OnDisable();
	}

	public override bool IsActive()
	{
		if (base.IsActive())
		{
			return m_Content != null;
		}
		return false;
	}

	private void EnsureLayoutHasRebuilt()
	{
		if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			Canvas.ForceUpdateCanvases();
		}
	}

	public virtual void StopMovement()
	{
		m_Velocity = Vector2.zero;
	}

	public void ResetContentPositionForRebuild()
	{
		if ((bool)m_Content)
		{
			StopMovement();
			m_Content.anchoredPosition = Vector2.zero;
			UpdateBounds();
			UpdatePrevData();
		}
	}

	public void RebuildContentLayoutForSnapping()
	{
		if ((bool)m_Content)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
			Canvas.ForceUpdateCanvases();
			UpdateBounds();
			UpdatePrevData();
		}
	}

	public virtual void OnScroll(PointerEventData data)
	{
		if (!IsActive())
		{
			return;
		}
		EnsureLayoutHasRebuilt();
		UpdateBounds();
		Vector2 scrollDelta = data.scrollDelta;
		scrollDelta.y *= -1f;
		if (m_Vertical && !m_Horizontal)
		{
			if (Mathf.Abs(scrollDelta.x) > Mathf.Abs(scrollDelta.y))
			{
				scrollDelta.y = scrollDelta.x;
			}
			scrollDelta.x = 0f;
		}
		if (m_Horizontal && !m_Vertical)
		{
			if (Mathf.Abs(scrollDelta.y) > Mathf.Abs(scrollDelta.x))
			{
				scrollDelta.x = scrollDelta.y;
			}
			scrollDelta.y = 0f;
		}
		if (data.IsScrolling())
		{
			m_Scrolling = true;
		}
		Vector2 anchoredPosition = m_Content.anchoredPosition;
		anchoredPosition += scrollDelta * m_ScrollSensitivity;
		if (m_MovementType == ScrollRect.MovementType.Clamped)
		{
			anchoredPosition += CalculateOffset(anchoredPosition - m_Content.anchoredPosition);
		}
		SetContentAnchoredPosition(anchoredPosition);
		UpdateBounds();
	}

	public virtual void OnInitializePotentialDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_Velocity = Vector2.zero;
		}
	}

	public virtual void OnBeginDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && IsActive())
		{
			UpdateBounds();
			m_PointerStartLocalCursor = Vector2.zero;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewRect, eventData.position, eventData.pressEventCamera, ref m_PointerStartLocalCursor);
			m_ContentStartPosition = m_Content.anchoredPosition;
			m_Dragging = true;
		}
	}

	public virtual void OnEndDrag(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_Dragging = false;
			onEndDrag?.Invoke();
		}
	}

	public virtual void OnDrag(PointerEventData eventData)
	{
		Vector2 vector = default(Vector2);
		if (!m_Dragging || eventData.button != PointerEventData.InputButton.Left || !IsActive() || !RectTransformUtility.ScreenPointToLocalPointInRectangle(m_ViewRect, eventData.position, eventData.pressEventCamera, ref vector))
		{
			return;
		}
		UpdateBounds();
		Vector2 vector2 = vector - m_PointerStartLocalCursor;
		Vector2 vector3 = m_ContentStartPosition + vector2;
		Vector2 vector4 = CalculateOffset(vector3 - m_Content.anchoredPosition);
		vector3 += vector4;
		if (m_MovementType == ScrollRect.MovementType.Elastic)
		{
			if (vector4.x != 0f)
			{
				vector3.x -= RubberDelta(vector4.x, m_ViewBounds.size.x);
			}
			if (vector4.y != 0f)
			{
				vector3.y -= RubberDelta(vector4.y, m_ViewBounds.size.y);
			}
		}
		SetContentAnchoredPosition(vector3);
	}

	protected virtual void SetContentAnchoredPosition(Vector2 position)
	{
		if (!m_Horizontal)
		{
			position.x = m_Content.anchoredPosition.x;
		}
		if (!m_Vertical)
		{
			position.y = m_Content.anchoredPosition.y;
		}
		if (position != m_Content.anchoredPosition)
		{
			m_Content.anchoredPosition = position;
			UpdateBounds();
		}
	}

	protected virtual void LateUpdate()
	{
		if (!m_Content)
		{
			return;
		}
		EnsureLayoutHasRebuilt();
		UpdateBounds();
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		Vector2 vector = CalculateOffset(Vector2.zero);
		if (unscaledDeltaTime > 0f)
		{
			if (!m_Dragging && !m_SnappingActive && (vector != Vector2.zero || m_Velocity != Vector2.zero))
			{
				Vector2 anchoredPosition = m_Content.anchoredPosition;
				for (int i = 0; i < 2; i++)
				{
					if (m_MovementType == ScrollRect.MovementType.Elastic && vector[i] != 0f)
					{
						float currentVelocity = m_Velocity[i];
						float num = m_Elasticity;
						if (m_Scrolling)
						{
							num *= 3f;
						}
						anchoredPosition[i] = Mathf.SmoothDamp(m_Content.anchoredPosition[i], m_Content.anchoredPosition[i] + vector[i], ref currentVelocity, num, float.PositiveInfinity, unscaledDeltaTime);
						if (Mathf.Abs(currentVelocity) < 1f)
						{
							currentVelocity = 0f;
						}
						m_Velocity[i] = currentVelocity;
					}
					else if (m_Inertia)
					{
						m_Velocity[i] *= Mathf.Pow(m_DecelerationRate, unscaledDeltaTime);
						if (Mathf.Abs(m_Velocity[i]) < 1f)
						{
							m_Velocity[i] = 0f;
						}
						anchoredPosition[i] += m_Velocity[i] * unscaledDeltaTime;
					}
					else
					{
						m_Velocity[i] = 0f;
					}
				}
				if (m_MovementType == ScrollRect.MovementType.Clamped)
				{
					vector = CalculateOffset(anchoredPosition - m_Content.anchoredPosition);
					anchoredPosition += vector;
				}
				SetContentAnchoredPosition(anchoredPosition);
			}
			if (m_Dragging && m_Inertia)
			{
				Vector3 b = (m_Content.anchoredPosition - m_PrevPosition) / unscaledDeltaTime;
				m_Velocity = Vector3.Lerp(m_Velocity, b, unscaledDeltaTime * 10f);
			}
		}
		if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
		{
			UpdateScrollbars(vector);
			UISystemProfilerApi.AddMarker("ScrollRect.value", (UnityEngine.Object)this);
			m_OnValueChanged.Invoke(normalizedPosition);
			UpdatePrevData();
		}
		UpdateScrollbarVisibility();
		m_Scrolling = false;
		ApplySnappingScale();
		ApplySnapping();
	}

	protected void UpdatePrevData()
	{
		if (m_Content == null)
		{
			m_PrevPosition = Vector2.zero;
		}
		else
		{
			m_PrevPosition = m_Content.anchoredPosition;
		}
		m_PrevViewBounds = m_ViewBounds;
		m_PrevContentBounds = m_ContentBounds;
	}

	private void UpdateScrollbars(Vector2 offset)
	{
		if ((bool)m_HorizontalScrollbar)
		{
			if (m_ContentBounds.size.x > 0f)
			{
				m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x);
			}
			else
			{
				m_HorizontalScrollbar.size = 1f;
			}
			m_HorizontalScrollbar.value = horizontalNormalizedPosition;
		}
		if ((bool)m_VerticalScrollbar)
		{
			if (m_ContentBounds.size.y > 0f)
			{
				m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y);
			}
			else
			{
				m_VerticalScrollbar.size = 1f;
			}
			m_VerticalScrollbar.value = verticalNormalizedPosition;
		}
	}

	private void SetHorizontalNormalizedPosition(float value)
	{
		SetNormalizedPosition(value, 0);
	}

	private void SetVerticalNormalizedPosition(float value)
	{
		SetNormalizedPosition(value, 1);
	}

	protected virtual void SetNormalizedPosition(float value, int axis)
	{
		EnsureLayoutHasRebuilt();
		UpdateBounds();
		float num = m_ContentBounds.size[axis] - m_ViewBounds.size[axis];
		float num2 = m_ViewBounds.min[axis] - value * num;
		float num3 = m_Content.anchoredPosition[axis] + num2 - m_ContentBounds.min[axis];
		Vector3 vector = m_Content.anchoredPosition;
		if (Mathf.Abs(vector[axis] - num3) > 0.01f)
		{
			vector[axis] = num3;
			m_Content.anchoredPosition = vector;
			m_Velocity[axis] = 0f;
			UpdateBounds();
		}
	}

	private static float RubberDelta(float overStretching, float viewSize)
	{
		return (1f - 1f / (Mathf.Abs(overStretching) * 0.55f / viewSize + 1f)) * viewSize * Mathf.Sign(overStretching);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		SetDirty();
	}

	public virtual void CalculateLayoutInputHorizontal()
	{
	}

	public virtual void CalculateLayoutInputVertical()
	{
	}

	public virtual void SetLayoutHorizontal()
	{
		m_Tracker.Clear();
		UpdateCachedData();
		if (m_HSliderExpand || m_VSliderExpand)
		{
			m_Tracker.Add(this, m_ViewRect, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
			m_ViewRect.anchorMin = Vector2.zero;
			m_ViewRect.anchorMax = Vector2.one;
			m_ViewRect.sizeDelta = Vector2.zero;
			m_ViewRect.anchoredPosition = Vector2.zero;
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
			m_ViewBounds = new Bounds(m_ViewRect.rect.center, m_ViewRect.rect.size);
			m_ContentBounds = GetBounds();
		}
		if (m_VSliderExpand && vScrollingNeeded)
		{
			m_ViewRect.sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), m_ViewRect.sizeDelta.y);
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_Content);
			m_ViewBounds = new Bounds(m_ViewRect.rect.center, m_ViewRect.rect.size);
			m_ContentBounds = GetBounds();
		}
		if (m_HSliderExpand && hScrollingNeeded)
		{
			m_ViewRect.sizeDelta = new Vector2(m_ViewRect.sizeDelta.x, 0f - (m_HSliderHeight + m_HorizontalScrollbarSpacing));
			m_ViewBounds = new Bounds(m_ViewRect.rect.center, m_ViewRect.rect.size);
			m_ContentBounds = GetBounds();
		}
		if (m_VSliderExpand && vScrollingNeeded && m_ViewRect.sizeDelta.x == 0f && m_ViewRect.sizeDelta.y < 0f)
		{
			m_ViewRect.sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), m_ViewRect.sizeDelta.y);
		}
	}

	public virtual void SetLayoutVertical()
	{
		UpdateScrollbarLayout();
		m_ViewBounds = new Bounds(m_ViewRect.rect.center, m_ViewRect.rect.size);
		m_ContentBounds = GetBounds();
	}

	private void UpdateScrollbarVisibility()
	{
		UpdateOneScrollbarVisibility(vScrollingNeeded, m_Vertical, m_VerticalScrollbarVisibility, m_VerticalScrollbar);
		UpdateOneScrollbarVisibility(hScrollingNeeded, m_Horizontal, m_HorizontalScrollbarVisibility, m_HorizontalScrollbar);
	}

	private static void UpdateOneScrollbarVisibility(bool xScrollingNeeded, bool xAxisEnabled, ScrollRect.ScrollbarVisibility scrollbarVisibility, Scrollbar scrollbar)
	{
		if (!scrollbar)
		{
			return;
		}
		if (scrollbarVisibility == ScrollRect.ScrollbarVisibility.Permanent)
		{
			if (scrollbar.gameObject.activeSelf != xAxisEnabled)
			{
				scrollbar.gameObject.SetActive(xAxisEnabled);
			}
		}
		else if (scrollbar.gameObject.activeSelf != xScrollingNeeded)
		{
			scrollbar.gameObject.SetActive(xScrollingNeeded);
		}
	}

	private void UpdateScrollbarLayout()
	{
		if (m_VSliderExpand && (bool)m_HorizontalScrollbar)
		{
			m_Tracker.Add(this, m_HorizontalScrollbarRect, DrivenTransformProperties.AnchoredPositionX | DrivenTransformProperties.AnchorMinX | DrivenTransformProperties.AnchorMaxX | DrivenTransformProperties.SizeDeltaX);
			m_HorizontalScrollbarRect.anchorMin = new Vector2(0f, m_HorizontalScrollbarRect.anchorMin.y);
			m_HorizontalScrollbarRect.anchorMax = new Vector2(1f, m_HorizontalScrollbarRect.anchorMax.y);
			m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0f, m_HorizontalScrollbarRect.anchoredPosition.y);
			if (vScrollingNeeded)
			{
				m_HorizontalScrollbarRect.sizeDelta = new Vector2(0f - (m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
			}
			else
			{
				m_HorizontalScrollbarRect.sizeDelta = new Vector2(0f, m_HorizontalScrollbarRect.sizeDelta.y);
			}
		}
		if (m_HSliderExpand && (bool)m_VerticalScrollbar)
		{
			m_Tracker.Add(this, m_VerticalScrollbarRect, DrivenTransformProperties.AnchoredPositionY | DrivenTransformProperties.AnchorMinY | DrivenTransformProperties.AnchorMaxY | DrivenTransformProperties.SizeDeltaY);
			m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0f);
			m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1f);
			m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0f);
			if (hScrollingNeeded)
			{
				m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0f - (m_HSliderHeight + m_HorizontalScrollbarSpacing));
			}
			else
			{
				m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0f);
			}
		}
	}

	protected void UpdateBounds()
	{
		m_ViewBounds = new Bounds(m_ViewRect.rect.center, m_ViewRect.rect.size);
		m_ContentBounds = GetBounds();
		if (m_Content == null)
		{
			return;
		}
		Vector3 contentSize = m_ContentBounds.size;
		Vector3 contentPos = m_ContentBounds.center;
		Vector2 contentPivot = m_Content.pivot;
		AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
		m_ContentBounds.size = contentSize;
		m_ContentBounds.center = contentPos;
		if (m_MovementType != ScrollRect.MovementType.Clamped)
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		if (m_ViewBounds.max.x > m_ContentBounds.max.x)
		{
			zero.x = Math.Min(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
		}
		else if (m_ViewBounds.min.x < m_ContentBounds.min.x)
		{
			zero.x = Math.Max(m_ViewBounds.min.x - m_ContentBounds.min.x, m_ViewBounds.max.x - m_ContentBounds.max.x);
		}
		if (m_ViewBounds.min.y < m_ContentBounds.min.y)
		{
			zero.y = Math.Max(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
		}
		else if (m_ViewBounds.max.y > m_ContentBounds.max.y)
		{
			zero.y = Math.Min(m_ViewBounds.min.y - m_ContentBounds.min.y, m_ViewBounds.max.y - m_ContentBounds.max.y);
		}
		if (zero.sqrMagnitude > float.Epsilon)
		{
			contentPos = m_Content.anchoredPosition + zero;
			if (!m_Horizontal)
			{
				contentPos.x = m_Content.anchoredPosition.x;
			}
			if (!m_Vertical)
			{
				contentPos.y = m_Content.anchoredPosition.y;
			}
			AdjustBounds(ref m_ViewBounds, ref contentPivot, ref contentSize, ref contentPos);
		}
	}

	internal static void AdjustBounds(ref Bounds viewBounds, ref Vector2 contentPivot, ref Vector3 contentSize, ref Vector3 contentPos)
	{
		Vector3 vector = viewBounds.size - contentSize;
		if (vector.x > 0f)
		{
			contentPos.x -= vector.x * (contentPivot.x - 0.5f);
			contentSize.x = viewBounds.size.x;
		}
		if (vector.y > 0f)
		{
			contentPos.y -= vector.y * (contentPivot.y - 0.5f);
			contentSize.y = viewBounds.size.y;
		}
	}

	private Bounds GetBounds()
	{
		if (m_Content == null)
		{
			return default(Bounds);
		}
		m_Content.GetWorldCorners(m_Corners);
		Matrix4x4 viewWorldToLocalMatrix = m_ViewRect.worldToLocalMatrix;
		return InternalGetBounds(m_Corners, ref viewWorldToLocalMatrix);
	}

	internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 viewWorldToLocalMatrix)
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < 4; i++)
		{
			Vector3 lhs = viewWorldToLocalMatrix.MultiplyPoint3x4(corners[i]);
			vector = Vector3.Min(lhs, vector);
			vector2 = Vector3.Max(lhs, vector2);
		}
		Bounds result = new Bounds(vector, Vector3.zero);
		result.Encapsulate(vector2);
		return result;
	}

	private Vector2 CalculateOffset(Vector2 delta)
	{
		return InternalCalculateOffset(ref m_ViewBounds, ref m_ContentBounds, m_Horizontal, m_Vertical, m_MovementType, ref delta);
	}

	internal static Vector2 InternalCalculateOffset(ref Bounds viewBounds, ref Bounds contentBounds, bool horizontal, bool vertical, ScrollRect.MovementType movementType, ref Vector2 delta)
	{
		Vector2 zero = Vector2.zero;
		if (movementType == ScrollRect.MovementType.Unrestricted)
		{
			return zero;
		}
		Vector2 vector = contentBounds.min;
		Vector2 vector2 = contentBounds.max;
		if (horizontal)
		{
			vector.x += delta.x;
			vector2.x += delta.x;
			float num = viewBounds.max.x - vector2.x;
			float num2 = viewBounds.min.x - vector.x;
			if (num2 < -0.001f)
			{
				zero.x = num2;
			}
			else if (num > 0.001f)
			{
				zero.x = num;
			}
		}
		if (vertical)
		{
			vector.y += delta.y;
			vector2.y += delta.y;
			float num3 = viewBounds.max.y - vector2.y;
			float num4 = viewBounds.min.y - vector.y;
			if (num3 > 0.001f)
			{
				zero.y = num3;
			}
			else if (num4 < -0.001f)
			{
				zero.y = num4;
			}
		}
		return zero;
	}

	protected void SetDirty()
	{
		if (IsActive())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	protected void SetDirtyCaching()
	{
		if (IsActive())
		{
			CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	private void ApplySnapping()
	{
		if (!m_Snapping)
		{
			return;
		}
		if (m_Dragging || m_Scrolling || m_Velocity.magnitude > m_SnappingSpeedThreshold)
		{
			m_SnappingActive = false;
		}
		else
		{
			if (!m_Content || m_Content.childCount == 0)
			{
				return;
			}
			RectTransform nearestElementToSnappingTarget = GetNearestElementToSnappingTarget();
			if (!(nearestElementToSnappingTarget == null))
			{
				Vector2 vector = Vector2.zero;
				Vector2 snappingOffsetInContentParent = GetSnappingOffsetInContentParent(nearestElementToSnappingTarget);
				if (m_Horizontal)
				{
					float x = m_Content.anchoredPosition.x - snappingOffsetInContentParent.x;
					vector = new Vector2(x, m_Content.anchoredPosition.y);
				}
				else if (m_Vertical)
				{
					float y = m_Content.anchoredPosition.y - snappingOffsetInContentParent.y;
					vector = new Vector2(m_Content.anchoredPosition.x, y);
				}
				if (Vector2.Distance(m_Content.anchoredPosition, vector) > 0.01f)
				{
					m_SnappingActive = true;
					StopMovement();
					Vector2 contentAnchoredPosition = Vector2.Lerp(m_Content.anchoredPosition, vector, m_SnappingSpeed * Time.unscaledDeltaTime);
					SetContentAnchoredPosition(contentAnchoredPosition);
				}
				else
				{
					StopMovement();
					SetContentAnchoredPosition(vector);
					m_SnappingActive = false;
				}
			}
		}
	}

	public RectTransform GetNearestElementToSnappingTarget()
	{
		if (m_snappingTarget == null)
		{
			return null;
		}
		RectTransform result = null;
		float num = float.PositiveInfinity;
		for (int i = 0; i < m_Content.childCount; i++)
		{
			RectTransform rectTransform = m_Content.GetChild(i) as RectTransform;
			if (!(rectTransform == null) && rectTransform.gameObject.activeSelf && (snappingElementFilter == null || snappingElementFilter(rectTransform)))
			{
				float elementDistanceToSnappingTarget = GetElementDistanceToSnappingTarget(rectTransform);
				if (elementDistanceToSnappingTarget < num)
				{
					num = elementDistanceToSnappingTarget;
					result = rectTransform;
				}
			}
		}
		return result;
	}

	private void ApplySnappingScale()
	{
		if (!m_SnappingScale || m_snappingTarget == null || !m_Content || m_SnappingScaleDistance <= 0f)
		{
			return;
		}
		for (int i = 0; i < m_Content.childCount; i++)
		{
			RectTransform rectTransform = m_Content.GetChild(i) as RectTransform;
			if (!(rectTransform == null) && rectTransform.gameObject.activeSelf)
			{
				float elementDistanceToSnappingTarget = GetElementDistanceToSnappingTarget(rectTransform);
				float t = 1f - Mathf.Clamp01(elementDistanceToSnappingTarget / m_SnappingScaleDistance);
				float num = Mathf.Lerp(m_MinSnappingScale, m_MaxSnappingScale, t);
				rectTransform.localScale = Vector3.one * num;
			}
		}
	}

	private float GetElementDistanceToSnappingTarget(RectTransform element)
	{
		if (m_Horizontal)
		{
			return Mathf.Abs(GetSnappingElementPosition(element).x - GetSnappingTargetPosition().x);
		}
		if (m_Vertical)
		{
			return Mathf.Abs(GetSnappingElementPosition(element).y - GetSnappingTargetPosition().y);
		}
		return 0f;
	}

	private Vector3 GetSnappingElementPosition(RectTransform element)
	{
		RectTransform rectTransform = element.GetComponent<IWheelScrollRectSnappingAnchor>()?.snappingAnchor;
		return GetElementCenterPosition((rectTransform != null) ? rectTransform : element);
	}

	private Vector3 GetElementCenterPosition(RectTransform element)
	{
		return element.TransformPoint(element.rect.center);
	}

	private Vector3 GetSnappingTargetPosition()
	{
		if (!(m_snappingTarget != null))
		{
			return GetPosFromSlider(m_SnappingPosition);
		}
		return GetElementCenterPosition(m_snappingTarget);
	}

	private Vector2 GetSnappingOffsetInContentParent(RectTransform element)
	{
		RectTransform rectTransform = m_Content.parent as RectTransform;
		Vector3 snappingElementPosition = GetSnappingElementPosition(element);
		Vector3 snappingTargetPosition = GetSnappingTargetPosition();
		if (rectTransform == null)
		{
			Vector3 vector = snappingElementPosition - snappingTargetPosition;
			return new Vector2(vector.x, vector.y);
		}
		Vector3 vector2 = rectTransform.InverseTransformPoint(snappingElementPosition);
		Vector3 vector3 = rectTransform.InverseTransformPoint(snappingTargetPosition);
		Vector3 vector4 = vector2 - vector3;
		return new Vector2(vector4.x, vector4.y);
	}

	private Vector3 GetPosFromSlider(float sliderPos)
	{
		Vector3 result = m_ViewRect.position;
		if (m_Horizontal)
		{
			result = m_ViewRect.TransformPoint(new Vector3(m_ViewRect.rect.width * (sliderPos - m_ViewRect.pivot.x), 0f, 0f));
		}
		else if (m_Vertical)
		{
			result = m_ViewRect.TransformPoint(new Vector3(0f, m_ViewRect.rect.height * (sliderPos - m_ViewRect.pivot.y), 0f));
		}
		return result;
	}

	public void SnapToElementInstant(RectTransform targetElement)
	{
		if (!(targetElement == null) && (bool)m_Content)
		{
			EnsureLayoutHasRebuilt();
			UpdateBounds();
			Vector2 contentAnchoredPosition = Vector2.zero;
			Vector2 snappingOffsetInContentParent = GetSnappingOffsetInContentParent(targetElement);
			if (m_Horizontal)
			{
				float x = m_Content.anchoredPosition.x - snappingOffsetInContentParent.x;
				contentAnchoredPosition = new Vector2(x, m_Content.anchoredPosition.y);
			}
			else if (m_Vertical)
			{
				float y = m_Content.anchoredPosition.y - snappingOffsetInContentParent.y;
				contentAnchoredPosition = new Vector2(m_Content.anchoredPosition.x, y);
			}
			StopMovement();
			SetContentAnchoredPosition(contentAnchoredPosition);
		}
	}

	public void SnapToElement(RectTransform targetElement, float speed, Action onComplete)
	{
		if (!(targetElement == null) && (bool)m_Content)
		{
			EnsureLayoutHasRebuilt();
			UpdateBounds();
			Vector2 targetPosition = Vector2.zero;
			Vector2 snappingOffsetInContentParent = GetSnappingOffsetInContentParent(targetElement);
			if (m_Horizontal)
			{
				float x = m_Content.anchoredPosition.x - snappingOffsetInContentParent.x;
				targetPosition = new Vector2(x, m_Content.anchoredPosition.y);
			}
			else if (m_Vertical)
			{
				float y = m_Content.anchoredPosition.y - snappingOffsetInContentParent.y;
				targetPosition = new Vector2(m_Content.anchoredPosition.x, y);
			}
			StopMovement();
			StartCoroutine(SnapToElementCoroutine(targetPosition, speed, onComplete));
		}
	}

	private IEnumerator SnapToElementCoroutine(Vector2 targetPosition, float speed, Action onComplete)
	{
		Vector2 startPosition = m_Content.anchoredPosition;
		float num = Vector2.Distance(startPosition, targetPosition);
		float duration = num / speed;
		float elapsedTime = 0f;
		while (elapsedTime < duration)
		{
			elapsedTime += Time.unscaledDeltaTime;
			float t = Mathf.Clamp01(elapsedTime / duration);
			Vector2 contentAnchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
			SetContentAnchoredPosition(contentAnchoredPosition);
			yield return null;
		}
		SetContentAnchoredPosition(targetPosition);
		onComplete?.Invoke();
	}
}
