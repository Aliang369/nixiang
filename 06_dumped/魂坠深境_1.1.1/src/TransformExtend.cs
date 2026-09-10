using UnityEngine;

public static class TransformExtend
{
	public static RectTransform RectTransform<T>(this T gameObject) where T : MonoBehaviour
	{
		return gameObject.transform as RectTransform;
	}

	public static RectTransform RectTransform(this Transform transform)
	{
		return transform as RectTransform;
	}

	public static Transform[] GetChildren(this Transform transform)
	{
		int childCount = transform.childCount;
		Transform[] array = new Transform[childCount];
		for (int i = 0; i < childCount; i++)
		{
			array[i] = transform.GetChild(i);
		}
		return array;
	}

	public static void SetPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
	{
		Vector3 position = transform.position;
		if (x.HasValue && !float.IsNaN(x.Value))
		{
			position.x = x.Value;
		}
		if (y.HasValue && !float.IsNaN(y.Value))
		{
			position.y = y.Value;
		}
		if (z.HasValue && !float.IsNaN(z.Value))
		{
			position.z = z.Value;
		}
		transform.position = position;
	}

	public static void SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
	{
		Vector3 localPosition = transform.localPosition;
		if (x.HasValue && !float.IsNaN(x.Value))
		{
			localPosition.x = x.Value;
		}
		if (y.HasValue && !float.IsNaN(y.Value))
		{
			localPosition.y = y.Value;
		}
		if (z.HasValue && !float.IsNaN(z.Value))
		{
			localPosition.z = z.Value;
		}
		transform.localPosition = localPosition;
	}
}
