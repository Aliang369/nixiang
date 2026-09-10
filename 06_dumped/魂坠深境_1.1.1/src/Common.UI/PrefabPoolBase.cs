using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI;

public abstract class PrefabPoolBase
{
	internal abstract void Dispose();
}
public abstract class PrefabPoolBase<T> : PrefabPoolBase where T : Component
{
	[NonSerialized]
	protected List<T> activePool;

	[NonSerialized]
	protected Transform defaultParent;

	[NonSerialized]
	protected List<T> inactivePool;

	[NonSerialized]
	protected T prefab;

	public int activeCount => activePool.Count;

	public T GetActiveElement(int index)
	{
		return activePool[index];
	}

	public int IndexOf(T obj)
	{
		return activePool.IndexOf(obj);
	}

	public IReadOnlyList<T> GetActiveElements()
	{
		return activePool;
	}

	public virtual void Despawn(T obj)
	{
		if (activePool.Contains(obj))
		{
			obj.gameObject.SetActive(value: false);
			activePool.Remove(obj);
			inactivePool.Add(obj);
		}
	}

	public void DespawnAll()
	{
		while (activePool.Count > 0)
		{
			Despawn(activePool[0]);
		}
	}

	public virtual void DestroyInActive()
	{
		foreach (T item in inactivePool)
		{
			UnityEngine.Object.Destroy(item.gameObject);
		}
		inactivePool.Clear();
	}
}
