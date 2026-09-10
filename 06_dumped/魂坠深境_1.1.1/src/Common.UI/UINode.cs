using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI;

public abstract class UINode : MonoBehaviour
{
	[NonSerialized]
	public object _setupData;

	[NonSerialized]
	public (object key, object value)[] _appendData;

	[NonSerialized]
	protected static bool hasDirty;

	[NonSerialized]
	private RectTransform _rectTransform;

	[NonSerialized]
	protected List<UIBase> children = new List<UIBase>();

	[NonSerialized]
	protected List<UIBase> childrenTemp;

	[NonSerialized]
	protected List<UIBase> childrenToRemove;

	[NonSerialized]
	public UIManager manager;

	[NonSerialized]
	public UINode parent;

	[NonSerialized]
	protected List<PrefabPoolBase> pools = new List<PrefabPoolBase>();

	[NonSerialized]
	protected bool updating;

	[NonSerialized]
	protected bool inited;

	public RectTransform rectTransform => _rectTransform ?? (_rectTransform = base.transform as RectTransform);

	public void InitUI(UIBase child)
	{
		if (child == null)
		{
			Debug.LogError($"child == null:{child == null} [{base.gameObject.name}]", this);
		}
		else
		{
			if (child.inited)
			{
				return;
			}
			if (updating)
			{
				if (childrenTemp == null)
				{
					childrenTemp = new List<UIBase>();
				}
				childrenTemp.Add(child);
			}
			else
			{
				children.Add(child);
			}
			if (child is IInputUpdate item)
			{
				manager.inputUpdaters.Add(item);
			}
			if (child is IFrameUpdate item2)
			{
				manager.frameUpdaters.Add(item2);
			}
			if (child is ILateUpdate item3)
			{
				manager.lateUpdaters.Add(item3);
			}
			child.manager = manager;
			child.parent = this;
			child.OnConfigContext();
			child._InternalInit();
		}
	}

	public void DisposeUI(UIBase child)
	{
		if (!children.Contains(child))
		{
			return;
		}
		if (updating)
		{
			if (childrenToRemove == null)
			{
				childrenToRemove = new List<UIBase>();
			}
			childrenToRemove.Add(child);
		}
		else
		{
			children.Remove(child);
		}
		if (child is IInputUpdate item)
		{
			manager.inputUpdaters.Remove(item);
		}
		if (child is IFrameUpdate item2)
		{
			manager.frameUpdaters.Remove(item2);
		}
		if (child is ILateUpdate item3)
		{
			manager.lateUpdaters.Remove(item3);
		}
		child._InternalDispose();
		child.parent = null;
	}

	public abstract void SetDirty();

	protected virtual void OnConfigContext()
	{
	}

	internal void _InternalHandleChildren()
	{
		if (childrenTemp != null && childrenTemp.Count > 0)
		{
			foreach (UIBase item in childrenTemp)
			{
				children.Add(item);
			}
			childrenTemp = null;
		}
		if (childrenToRemove == null || childrenToRemove.Count <= 0)
		{
			return;
		}
		foreach (UIBase item2 in childrenToRemove)
		{
			children.Remove(item2);
		}
		childrenToRemove = null;
	}

	public CellUIPrefabPool<T, TSetupData> GetPrefabPool<T, TSetupData>(T prefab, Transform defaultParent = null) where T : CellUIBase<TSetupData>
	{
		if (defaultParent == null)
		{
			defaultParent = prefab.transform.parent;
		}
		if (defaultParent == null)
		{
			defaultParent = GetDefaultParent();
		}
		CellUIPrefabPool<T, TSetupData> cellUIPrefabPool = new CellUIPrefabPool<T, TSetupData>(prefab, this, defaultParent);
		pools.Add(cellUIPrefabPool);
		return cellUIPrefabPool;
		RectTransform GetDefaultParent()
		{
			GameObject obj = new GameObject();
			obj.transform.parent = base.transform;
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.name = prefab.name + " Pool";
			obj.transform.SetAsLastSibling();
			return obj.AddComponent<RectTransform>();
		}
	}

	public void GetPrefabPool<T, TSetupData>(ref CellUIPrefabPool<T, TSetupData> pool, T prefab, Transform defaultParent = null, ICommonInfoPanel infoPanel = null) where T : CellUIBase<TSetupData>
	{
		if (prefab == null)
		{
			Debug.LogError("Prefab is null in " + base.gameObject.name);
		}
		pool = GetPrefabPool<T, TSetupData>(prefab, defaultParent);
		if (infoPanel != null)
		{
			pool.ConfigInfoPanel(infoPanel);
		}
	}

	public void Init<T, TSetupData>(CellUIPrefabPoolConfig<T, TSetupData> config) where T : CellUIBase<TSetupData>
	{
		GetPrefabPool(ref config.pool, config.prefab, config.parent);
	}

	public object GetAppendData(object key)
	{
		(object, object)[] appendData = _appendData;
		for (int i = 0; i < appendData.Length; i++)
		{
			var (obj, result) = appendData[i];
			if (obj == key)
			{
				return result;
			}
		}
		return null;
	}

	public T GetAppendData<T>() where T : class
	{
		object appendData = GetAppendData(typeof(T));
		if (appendData == null)
		{
			return null;
		}
		return appendData as T;
	}
}
