using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public static class RectTransformExtensions
{
	public class AnchoredPositionRef
	{
		private readonly RectTransform rectTransform;

		public float x
		{
			get
			{
				return rectTransform.anchoredPosition.x;
			}
			set
			{
				rectTransform.anchoredPosition = new Vector2(value, rectTransform.anchoredPosition.y);
			}
		}

		public float y
		{
			get
			{
				return rectTransform.anchoredPosition.y;
			}
			set
			{
				rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, value);
			}
		}

		public AnchoredPositionRef(RectTransform rectTransform)
		{
			this.rectTransform = rectTransform;
		}
	}

	public class AnchoredPosition3DRef
	{
		private readonly RectTransform rectTransform;

		public float x
		{
			get
			{
				return rectTransform.anchoredPosition3D.x;
			}
			set
			{
				Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
				anchoredPosition3D.x = value;
				rectTransform.anchoredPosition3D = anchoredPosition3D;
			}
		}

		public float y
		{
			get
			{
				return rectTransform.anchoredPosition3D.y;
			}
			set
			{
				Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
				anchoredPosition3D.y = value;
				rectTransform.anchoredPosition3D = anchoredPosition3D;
			}
		}

		public float z
		{
			get
			{
				return rectTransform.anchoredPosition3D.z;
			}
			set
			{
				Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
				anchoredPosition3D.z = value;
				rectTransform.anchoredPosition3D = anchoredPosition3D;
			}
		}

		public AnchoredPosition3DRef(RectTransform rectTransform)
		{
			this.rectTransform = rectTransform;
		}
	}

	public static void DestroyChildren(this RectTransform go)
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in go.transform)
		{
			list.Add(item.gameObject);
		}
		list.ForEach(Object.Destroy);
	}

	public static float GetHeight(this RectTransform trans)
	{
		return trans.rect.height;
	}

	public static Vector2 GetSize(this RectTransform trans)
	{
		return trans.rect.size;
	}

	public static float GetWidth(this RectTransform trans)
	{
		return trans.rect.width;
	}

	public static void SetAnchoredPosition(this RectTransform rectTransform, float? x = null, float? y = null)
	{
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		if (x.HasValue && !float.IsNaN(x.Value))
		{
			anchoredPosition.x = x.Value;
		}
		if (y.HasValue && !float.IsNaN(y.Value))
		{
			anchoredPosition.y = y.Value;
		}
		rectTransform.anchoredPosition = anchoredPosition;
	}

	public static void SetAnchoredPosition3D(this RectTransform rectTransform, float? x = null, float? y = null, float? z = null)
	{
		Vector3 anchoredPosition3D = rectTransform.anchoredPosition3D;
		if (x.HasValue && !float.IsNaN(x.Value))
		{
			anchoredPosition3D.x = x.Value;
		}
		if (y.HasValue && !float.IsNaN(y.Value))
		{
			anchoredPosition3D.y = y.Value;
		}
		if (z.HasValue && !float.IsNaN(z.Value))
		{
			anchoredPosition3D.z = z.Value;
		}
		rectTransform.anchoredPosition3D = anchoredPosition3D;
	}

	public static void SetHeight(this RectTransform trans, float newSize)
	{
		trans.SetSize(new Vector2(trans.rect.size.x, newSize));
	}

	public static void SetSize(this RectTransform trans, Vector2 newSize)
	{
		Vector2 size = trans.rect.size;
		Vector2 vector = newSize - size;
		trans.offsetMin -= new Vector2(vector.x * trans.pivot.x, vector.y * trans.pivot.y);
		trans.offsetMax += new Vector2(vector.x * (1f - trans.pivot.x), vector.y * (1f - trans.pivot.y));
	}

	public static void SetWidth(this RectTransform trans, float newSize)
	{
		trans.SetSize(new Vector2(newSize, trans.rect.size.y));
	}

	public static Vector3 AnchoredToLocalPosition(this RectTransform transform, Vector2 anchoredPosition)
	{
		RectTransform rectTransform = transform.parent.RectTransform();
		return Vector2.Scale(rectTransform.rect.size, transform.anchorMin + (transform.anchorMax - transform.anchorMin) * transform.pivot - rectTransform.pivot) + anchoredPosition;
	}

	public static Vector3 AnchoredToWorldPosition(this RectTransform transform, Vector2 anchoredPosition)
	{
		return transform.parent.TransformPoint(transform.AnchoredToLocalPosition(anchoredPosition));
	}

	public static Vector3 WorldPositionToAnchored(this RectTransform transform, Vector3 worldPosition)
	{
		return transform.LocalPositionToAnchored(transform.parent.InverseTransformPoint(worldPosition));
	}

	public static Vector3 LocalPositionToAnchored(this RectTransform transform, Vector3 localPosition)
	{
		RectTransform rectTransform = transform.parent.RectTransform();
		Vector2 vector = Vector2.Scale(rectTransform.rect.size, transform.anchorMin + (transform.anchorMax - transform.anchorMin) * transform.pivot - rectTransform.pivot);
		return localPosition - (Vector3)vector;
	}

	public static Vector3[] GetLocalCorners(this RectTransform transform)
	{
		Vector3[] array = new Vector3[4];
		transform.GetLocalCorners(array);
		return array;
	}

	public static Vector3[] GetWorldCorners(this RectTransform transform)
	{
		Vector3[] array = new Vector3[4];
		transform.GetWorldCorners(array);
		return array;
	}

	public static Rect GetWorldRect(this RectTransform rectTransform)
	{
		Vector3[] worldCorners = rectTransform.GetWorldCorners();
		return new Rect(worldCorners[0], worldCorners[2] - worldCorners[0]);
	}

	public static Vector3 WorldPositionFromCanvasToCanvas(this Vector3 fromPosition, Canvas fromCanvas, Canvas toCanvas)
	{
		return fromPosition.WorldToCanvasScreenPosition(fromCanvas).CanvasToWorldPosition(toCanvas);
	}

	public static Vector3 WorldToCanvasScreenPosition(this Vector3 worldPosition, Canvas fromCanvas)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)fromCanvas.renderMode == 0)
		{
			return worldPosition;
		}
		Camera eventCamera = fromCanvas.GetEventCamera();
		if (eventCamera == null)
		{
			return Vector3.zero;
		}
		Vector3 result = eventCamera.WorldToScreenPoint(worldPosition);
		result.z = ((Component)(object)fromCanvas).transform.InverseTransformPoint(worldPosition).z;
		return result;
	}

	public static Vector3 CanvasToWorldPosition(this Vector3 canvasPosition, Canvas fromCanvas)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if ((int)fromCanvas.renderMode == 0)
		{
			return canvasPosition;
		}
		Camera eventCamera = fromCanvas.GetEventCamera();
		if (eventCamera == null)
		{
			return Vector3.zero;
		}
		return eventCamera.ScreenPointToRay(canvasPosition).GetPoint(eventCamera.transform.InverseTransformPoint(((Component)(object)fromCanvas).transform.TransformPoint(canvasPosition)).z);
	}

	public static Vector3 ScreenToOverlayCanvasPosition(this Vector3 screenPosition)
	{
		return new Vector3(screenPosition.x * (float)Screen.width, screenPosition.y * (float)Screen.height, screenPosition.z);
	}

	public static AnchoredPositionRef AnchoredPosition(this RectTransform rectTransform)
	{
		return new AnchoredPositionRef(rectTransform);
	}

	public static AnchoredPosition3DRef AnchoredPosition3D(this RectTransform rectTransform)
	{
		return new AnchoredPosition3DRef(rectTransform);
	}
}
