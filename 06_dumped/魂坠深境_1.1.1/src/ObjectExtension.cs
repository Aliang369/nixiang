using UnityEngine;

public static class ObjectExtension
{
	public static T InstantiatePrefabIn<T>(this T original, Transform parent) where T : Object
	{
		return Object.Instantiate(original, parent);
	}

	public static bool IsChildrenOf(this GameObject gameObject, GameObject other)
	{
		if (gameObject == null || other == null)
		{
			return false;
		}
		Transform parent = gameObject.transform.parent;
		while (parent != null)
		{
			if (parent == other.transform)
			{
				return true;
			}
			parent = parent.transform.parent;
		}
		return false;
	}
}
