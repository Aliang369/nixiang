using System.Collections.Generic;
using UnityEngine;

namespace Common.UI;

public class PrefabPool<T> : PrefabPoolBase<T> where T : Component
{
	public PrefabPool(T prefab, Transform defaultParent = null)
	{
		if (defaultParent == null)
		{
			defaultParent = prefab.transform.parent;
		}
		base.prefab = prefab;
		base.defaultParent = defaultParent;
		activePool = new List<T>();
		inactivePool = new List<T>();
		prefab.gameObject.SetActive(value: false);
	}

	public T Spawn(Transform parent = null, bool setAsLastSibling = true)
	{
		if (prefab == null)
		{
			return null;
		}
		if (parent == null)
		{
			parent = defaultParent;
		}
		T val;
		if (inactivePool.Count > 0)
		{
			val = inactivePool[inactivePool.Count - 1];
			inactivePool.RemoveAt(inactivePool.Count - 1);
		}
		else
		{
			val = Object.Instantiate(prefab, parent);
		}
		val.gameObject.SetActive(value: true);
		val.gameObject.transform.SetParent(parent);
		if (setAsLastSibling)
		{
			val.gameObject.transform.SetAsLastSibling();
		}
		activePool.Add(val);
		return val;
	}

	internal override void Dispose()
	{
		foreach (T item in new List<T>(activePool))
		{
			Despawn(item);
		}
		inactivePool.Clear();
	}
}
