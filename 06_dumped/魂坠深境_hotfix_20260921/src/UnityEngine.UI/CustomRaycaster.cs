using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class CustomRaycaster : BaseRaycaster
{
	public enum BlockingObjects
	{
		None,
		TwoD,
		ThreeD,
		All
	}

	[SerializeField]
	public bool ignoreReversedGraphics = true;

	[SerializeField]
	public BlockingObjects blockingObjects;

	[SerializeField]
	public LayerMask blockingMask = -1;

	[NonSerialized]
	protected const int kNoEventMaskSet = -1;

	[NonSerialized]
	private Canvas m_Canvas;

	[NonSerialized]
	public List<Graphic> m_RaycastResults = new List<Graphic>();

	[NonSerialized]
	private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

	public override int sortOrderPriority
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if ((int)canvas.renderMode == 0)
			{
				return canvas.sortingOrder;
			}
			return base.sortOrderPriority;
		}
	}

	public override int renderOrderPriority
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			if ((int)canvas.renderMode == 0)
			{
				return canvas.rootCanvas.renderOrder;
			}
			return base.renderOrderPriority;
		}
	}

	private Canvas canvas
	{
		get
		{
			if ((Object)(object)m_Canvas != null)
			{
				return m_Canvas;
			}
			m_Canvas = GetComponent<Canvas>();
			return m_Canvas;
		}
	}

	public override Camera eventCamera
	{
		get
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			Canvas val = canvas;
			RenderMode renderMode = val.renderMode;
			if ((int)renderMode == 0 || ((int)renderMode == 1 && val.worldCamera == null))
			{
				return null;
			}
			return val.worldCamera ?? Camera.main;
		}
	}

	protected CustomRaycaster()
	{
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)canvas == null)
		{
			return;
		}
		IList<Graphic> raycastableGraphicsForCanvas = GraphicRegistry.GetRaycastableGraphicsForCanvas(canvas);
		if (raycastableGraphicsForCanvas == null || raycastableGraphicsForCanvas.Count == 0)
		{
			return;
		}
		Camera camera = eventCamera;
		int num = (((int)canvas.renderMode != 0 && !(camera == null)) ? camera.targetDisplay : canvas.targetDisplay);
		Vector3 vector = RelativeMouseAtScaled(eventData.position);
		if (vector != Vector3.zero)
		{
			if ((int)vector.z != num)
			{
				return;
			}
		}
		else
		{
			vector = eventData.position;
		}
		Vector2 vector2;
		if (camera == null)
		{
			float num2 = Screen.width;
			float num3 = Screen.height;
			if (num > 0 && num < Display.displays.Length)
			{
				num2 = Display.displays[num].systemWidth;
				num3 = Display.displays[num].systemHeight;
			}
			vector2 = new Vector2(vector.x / num2, vector.y / num3);
		}
		else
		{
			vector2 = camera.ScreenToViewportPoint(vector);
		}
		if (vector2.x < 0f || vector2.x > 1f || vector2.y < 0f || vector2.y > 1f)
		{
			return;
		}
		float num4 = float.MaxValue;
		Ray ray = default(Ray);
		if (camera != null)
		{
			ray = camera.ScreenPointToRay(vector);
		}
		if ((int)canvas.renderMode != 0 && blockingObjects != BlockingObjects.None && camera != null)
		{
			float z = ray.direction.z;
			if (!Mathf.Approximately(0f, z))
			{
				Mathf.Abs((camera.farClipPlane - camera.nearClipPlane) / z);
			}
		}
		m_RaycastResults.Clear();
		Raycast(canvas, camera, vector, raycastableGraphicsForCanvas, m_RaycastResults);
		int count = m_RaycastResults.Count;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = m_RaycastResults[i].gameObject;
			bool flag = true;
			if (ignoreReversedGraphics)
			{
				if (camera == null)
				{
					Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
					flag = Vector3.Dot(Vector3.forward, rhs) > 0f;
				}
				else
				{
					Vector3 vector3 = camera.transform.rotation * Vector3.forward * camera.nearClipPlane;
					flag = Vector3.Dot(gameObject.transform.position - camera.transform.position - vector3, gameObject.transform.forward) >= 0f;
				}
			}
			if (!flag)
			{
				continue;
			}
			float num5 = 0f;
			Transform transform = gameObject.transform;
			Vector3 forward = transform.forward;
			if (camera == null || (int)canvas.renderMode == 0)
			{
				num5 = 0f;
			}
			else
			{
				num5 = Vector3.Dot(forward, transform.position - ray.origin) / Vector3.Dot(forward, ray.direction);
				if (num5 < 0f)
				{
					continue;
				}
			}
			if (!(num5 >= num4))
			{
				RaycastResult item = new RaycastResult
				{
					gameObject = gameObject,
					module = this,
					distance = num5,
					screenPosition = vector,
					displayIndex = num,
					index = resultAppendList.Count,
					depth = m_RaycastResults[i].depth,
					sortingLayer = canvas.sortingLayerID,
					sortingOrder = canvas.sortingOrder,
					worldPosition = ray.origin + ray.direction * num5,
					worldNormal = -forward
				};
				resultAppendList.Add(item);
			}
		}
	}

	private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
	{
		int count = foundGraphics.Count;
		for (int i = 0; i < count; i++)
		{
			Graphic graphic = foundGraphics[i];
			if (graphic.raycastTarget && !graphic.canvasRenderer.cull && graphic.depth != -1 && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera, graphic.raycastPadding) && (!(eventCamera != null) || !(eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)) && graphic.Raycast(pointerPosition, eventCamera))
			{
				s_SortedGraphics.Add(graphic);
			}
		}
		s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
		count = s_SortedGraphics.Count;
		for (int num = 0; num < count; num++)
		{
			results.Add(s_SortedGraphics[num]);
		}
		s_SortedGraphics.Clear();
	}

	public static Vector3 RelativeMouseAtScaled(Vector2 position)
	{
		if (Display.main.renderingWidth != Display.main.systemWidth || Display.main.renderingHeight != Display.main.systemHeight)
		{
			float num = (float)Display.main.systemWidth / (float)Display.main.systemHeight;
			Vector2 vector = new Vector2(Display.main.renderingWidth, Display.main.renderingHeight);
			Vector2 zero = Vector2.zero;
			if (Screen.fullScreen)
			{
				float num2 = (float)Screen.width / (float)Screen.height;
				if ((float)Display.main.systemHeight * num2 < (float)Display.main.systemWidth)
				{
					vector.x = (float)Display.main.renderingHeight * num;
					zero.x = (vector.x - (float)Display.main.renderingWidth) * 0.5f;
				}
				else
				{
					vector.y = (float)Display.main.renderingWidth / num;
					zero.y = (vector.y - (float)Display.main.renderingHeight) * 0.5f;
				}
			}
			Vector2 vector2 = vector - zero;
			if (position.y < 0f - zero.y || position.y > vector2.y || position.x < 0f - zero.x || position.x > vector2.x)
			{
				Vector2 vector3 = position;
				if (!Screen.fullScreen)
				{
					vector3.x -= (float)(Display.main.renderingWidth - Display.main.systemWidth) * 0.5f;
					vector3.y -= (float)(Display.main.renderingHeight - Display.main.systemHeight) * 0.5f;
				}
				else
				{
					vector3 += zero;
					vector3.x *= (float)Display.main.systemWidth / vector.x;
					vector3.y *= (float)Display.main.systemHeight / vector.y;
				}
				Vector3 result = Display.RelativeMouseAt(vector3);
				if (result.z != 0f)
				{
					return result;
				}
			}
			return new Vector3(position.x, position.y, 0f);
		}
		return Display.RelativeMouseAt(position);
	}
}
