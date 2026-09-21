using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollRectGridLayoutGroup : LayoutGroup
{
	public class DisplayElement
	{
		public object data;

		private GameObject displayObject;

		public Action onHide;

		public Func<GameObject> onShow;

		private bool show;

		private Vector2 position;

		public void RefreshData(ScrollRectGridLayoutGroup group, int index)
		{
			position = group.GetElementAnchoredPosition(index);
		}

		public void RefreshVisibility(ScrollRectGridLayoutGroup group)
		{
			if (position.x - group.cellSize.x * 0.5f < group.maskCorners[2].x && position.x + group.cellSize.x * 0.5f > group.maskCorners[0].x && position.y - group.cellSize.y * 0.5f < group.maskCorners[2].y && position.y + group.cellSize.y * 0.5f > group.maskCorners[0].y)
			{
				Show();
			}
			else
			{
				Hide();
			}
		}

		private void Show()
		{
			if (!show)
			{
				displayObject = onShow();
			}
			show = true;
			displayObject.transform.localPosition = position;
		}

		private void Hide()
		{
			if (show)
			{
				show = false;
				onHide();
			}
		}
	}

	[SerializeField]
	public new RectOffset padding;

	[SerializeField]
	public Vector2 cellSize = new Vector2(100f, 100f);

	[SerializeField]
	private Vector2 spacing = Vector2.zero;

	[SerializeField]
	private GridLayoutGroup.Corner startCorner;

	[SerializeField]
	private GridLayoutGroup.Axis startAxis;

	[SerializeField]
	private new TextAnchor childAlignment;

	[SerializeField]
	private GridLayoutGroup.Constraint constraint;

	[SerializeField]
	private int constraintCount = 2;

	[SerializeField]
	public RectTransform mask;

	[NonSerialized]
	private int columns;

	[NonSerialized]
	private int rows;

	[NonSerialized]
	public readonly List<DisplayElement> displayElements = new List<DisplayElement>();

	[NonSerialized]
	private Vector3[] maskCorners;

	[NonSerialized]
	private Vector2 leftTop;

	protected ScrollRectGridLayoutGroup()
	{
	}

	public void AddDisplayElement(DisplayElement element)
	{
		displayElements.Add(element);
	}

	public void RemoveDisplayElement(DisplayElement element)
	{
		displayElements.Remove(element);
	}

	public void ClearAllDisplayElement()
	{
		displayElements.Clear();
	}

	public bool IsDisplayElementSame(List<DisplayElement> elements)
	{
		return elements.Equals(displayElements);
	}

	public void RefreshLayout()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Expected I4, but got Unknown
		switch (startAxis)
		{
		case GridLayoutGroup.Axis.Horizontal:
			LayoutHorizontal();
			break;
		case GridLayoutGroup.Axis.Vertical:
			LayoutVertical();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		RectTransform trans = this.RectTransform();
		Vector2 vector = trans.GetLocalCorners()[1];
		float num = ((columns == 0) ? 0f : ((float)(padding.left + padding.right) + spacing.x * (float)(columns - 1) + cellSize.x * (float)columns));
		float num2 = ((rows == 0) ? 0f : ((float)(padding.top + padding.bottom) + spacing.y * (float)(rows - 1) + cellSize.y * (float)rows));
		TextAnchor val = childAlignment;
		leftTop = (int)val switch
		{
			0 => Vector2.zero, 
			1 => new Vector2((trans.GetWidth() - num) / 2f, 0f), 
			2 => new Vector2(trans.GetWidth() - num, 0f), 
			3 => new Vector2(0f, (num2 - trans.GetHeight()) / 2f), 
			4 => new Vector2((trans.GetWidth() - num) / 2f, (num2 - trans.GetHeight()) / 2f), 
			5 => new Vector2(trans.GetWidth() - num, (num2 - trans.GetHeight()) / 2f), 
			6 => new Vector2(0f, num2 - trans.GetHeight()), 
			7 => new Vector2((trans.GetWidth() - num) / 2f, num2 - trans.GetHeight()), 
			8 => new Vector2(trans.GetWidth() - num, num2 - trans.GetHeight()), 
			_ => Vector2.zero, 
		};
		leftTop += vector;
		for (int i = 0; i < displayElements.Count; i++)
		{
			displayElements[i].RefreshData(this, i);
		}
		RefreshVisibility();
	}

	public void RefreshVisibility()
	{
		RectTransform rectTransform = this.RectTransform();
		Vector3[] worldCorners = mask.GetWorldCorners();
		maskCorners = new Vector3[4]
		{
			rectTransform.InverseTransformPoint(worldCorners[0]),
			rectTransform.InverseTransformPoint(worldCorners[1]),
			rectTransform.InverseTransformPoint(worldCorners[2]),
			rectTransform.InverseTransformPoint(worldCorners[3])
		};
		for (int i = 0; i < displayElements.Count; i++)
		{
			displayElements[i].RefreshVisibility(this);
		}
	}

	public bool ScrollToIndex(int index, float viewportAnchor = 0.28f)
	{
		if (index < 0 || index >= displayElements.Count || mask == null)
		{
			return false;
		}
		RefreshLayout();
		Canvas.ForceUpdateCanvases();
		ScrollRect componentInParent = GetComponentInParent<ScrollRect>(includeInactive: true);
		if (componentInParent == null)
		{
			return false;
		}
		RectTransform rectTransform = this.RectTransform();
		RectTransform rectTransform2 = ((componentInParent.content != null) ? componentInParent.content : rectTransform);
		Vector3 position = rectTransform.TransformPoint(GetElementAnchoredPosition(index));
		Vector3 vector = rectTransform2.InverseTransformPoint(position);
		viewportAnchor = Mathf.Clamp01(viewportAnchor);
		componentInParent.StopMovement();
		if (componentInParent.vertical)
		{
			float num = Mathf.Max(0f, rectTransform2.rect.height - mask.rect.height);
			float num2 = cellSize.y * Mathf.Abs(rectTransform.lossyScale.y / rectTransform2.lossyScale.y) * 0.5f;
			float num3 = rectTransform2.rect.yMax - (vector.y + num2);
			float num4 = Mathf.Max(0f, mask.rect.height - num2 * 2f) * viewportAnchor;
			float num5 = Mathf.Clamp(num3 - num4, 0f, num);
			componentInParent.verticalNormalizedPosition = ((num <= 0f) ? 1f : (1f - num5 / num));
		}
		if (componentInParent.horizontal)
		{
			float num6 = Mathf.Max(0f, rectTransform2.rect.width - mask.rect.width);
			float num7 = cellSize.x * Mathf.Abs(rectTransform.lossyScale.x / rectTransform2.lossyScale.x) * 0.5f;
			float num8 = vector.x - num7 - rectTransform2.rect.xMin;
			float num9 = Mathf.Max(0f, mask.rect.width - num7 * 2f) * viewportAnchor;
			float num10 = Mathf.Clamp(num8 - num9, 0f, num6);
			componentInParent.horizontalNormalizedPosition = ((num6 <= 0f) ? 0f : (num10 / num6));
		}
		Canvas.ForceUpdateCanvases();
		RefreshVisibility();
		return true;
	}

	private void LayoutHorizontal()
	{
		RectTransform trans = this.RectTransform();
		if (constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			columns = constraintCount;
		}
		else if (constraint == GridLayoutGroup.Constraint.FixedRowCount)
		{
			columns = Mathf.CeilToInt((float)displayElements.Count / (float)constraintCount - 0.001f);
		}
		else
		{
			columns = (int)((trans.GetWidth() - (float)padding.left - (float)padding.right + spacing.x) / (cellSize.x + spacing.x));
		}
		if (columns == 0)
		{
			columns = 1;
		}
		rows = Mathf.CeilToInt((float)displayElements.Count / (float)columns);
		trans.SetHeight((rows == 0) ? 0f : ((float)(padding.top + padding.bottom) + spacing.y * (float)(rows - 1) + cellSize.y * (float)rows));
		SetLayoutInputForAxis(trans.GetWidth(), trans.GetWidth(), 0f, 1);
	}

	private void LayoutVertical()
	{
		RectTransform trans = this.RectTransform();
		if (constraint == GridLayoutGroup.Constraint.FixedColumnCount)
		{
			rows = Mathf.CeilToInt((float)displayElements.Count / (float)constraintCount - 0.001f);
		}
		else if (constraint == GridLayoutGroup.Constraint.FixedRowCount)
		{
			rows = constraintCount;
		}
		else
		{
			rows = (int)((trans.GetHeight() - (float)padding.top - (float)padding.bottom + spacing.y) / (cellSize.y + spacing.y));
		}
		if (rows == 0)
		{
			rows = 1;
		}
		columns = Mathf.CeilToInt((float)displayElements.Count / (float)rows);
		trans.SetWidth((columns == 0) ? 0f : ((float)(padding.left + padding.right) + spacing.x * (float)(columns - 1) + cellSize.x * (float)columns));
		SetLayoutInputForAxis(trans.GetHeight(), trans.GetHeight(), 0f, 0);
	}

	public override void CalculateLayoutInputHorizontal()
	{
	}

	public override void CalculateLayoutInputVertical()
	{
	}

	public override void SetLayoutHorizontal()
	{
	}

	public override void SetLayoutVertical()
	{
	}

	private Vector2 GetElementAnchoredPosition(int index)
	{
		if (rows == 0 || columns == 0)
		{
			return Vector2.zero;
		}
		GridLayoutGroup.Corner corner = startCorner;
		GridLayoutGroup.Axis axis = startAxis;
		int num = default(int);
		switch (corner)
		{
		case GridLayoutGroup.Corner.UpperLeft:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = index - index / rows * rows;
				break;
			}
			num = index / columns;
			break;
		case GridLayoutGroup.Corner.UpperRight:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = index - index / rows * rows;
				break;
			}
			num = index / columns;
			break;
		case GridLayoutGroup.Corner.LowerLeft:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = rows - (index - index / rows * rows) - 1;
				break;
			}
			num = rows - index / columns - 1;
			break;
		case GridLayoutGroup.Corner.LowerRight:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = rows - (index - index / rows * rows) - 1;
				break;
			}
			num = rows - index / columns - 1;
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException((corner, axis));
			break;
		}
		int num2 = num;
		corner = startCorner;
		axis = startAxis;
		switch (corner)
		{
		case GridLayoutGroup.Corner.UpperLeft:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = index / rows;
				break;
			}
			num = index - index / columns * columns;
			break;
		case GridLayoutGroup.Corner.UpperRight:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = columns - index / rows - 1;
				break;
			}
			num = columns - (index - index / columns * columns) - 1;
			break;
		case GridLayoutGroup.Corner.LowerLeft:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = index / rows;
				break;
			}
			num = index - index / columns * columns;
			break;
		case GridLayoutGroup.Corner.LowerRight:
			if (axis != GridLayoutGroup.Axis.Horizontal)
			{
				if (axis != GridLayoutGroup.Axis.Vertical)
				{
					goto default;
				}
				num = columns - index / rows - 1;
				break;
			}
			num = columns - (index - index / columns * columns) - 1;
			break;
		default:
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException((corner, axis));
			break;
		}
		int num3 = num;
		return leftTop + new Vector2(((float)num3 + 0.5f) * cellSize.x + (float)num3 * spacing.x + (float)padding.left, 0f - (((float)num2 + 0.5f) * cellSize.y + (float)num2 * spacing.y + (float)padding.top));
	}
}
