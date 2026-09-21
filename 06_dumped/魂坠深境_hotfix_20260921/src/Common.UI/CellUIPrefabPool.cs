using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Common.UI;

public class CellUIPrefabPool<TCellUI, TSetupData> : PrefabPoolBase<TCellUI> where TCellUI : CellUIBase<TSetupData>
{
	public interface IDataConvertor<T>
	{
		TSetupData ToSetupData(T t);

		T FromSetupData(TSetupData setupData);
	}

	private readonly UINode attachedUI;

	public Action<TCellUI> onSetup;

	public Action<TCellUI> onInit;

	private static List<TCellUI> syncToDespawnBuffer = new List<TCellUI>();

	internal CellUIPrefabPool(TCellUI prefab, UINode attachedUI, Transform defaultParent = null)
	{
		base.prefab = prefab;
		this.attachedUI = attachedUI;
		base.defaultParent = defaultParent;
		activePool = new List<TCellUI>();
		inactivePool = new List<TCellUI>();
		if (prefab?.transform?.parent != null)
		{
			prefab.gameObject.SetActive(value: false);
		}
	}

	public TCellUI Spawn(TSetupData data, Transform parent = null, Action<TCellUI> overrideSetup = null, bool lastSibling = true)
	{
		if (data == null)
		{
			return null;
		}
		if (prefab == null)
		{
			return null;
		}
		if (parent == null)
		{
			parent = defaultParent;
		}
		TCellUI val;
		if (inactivePool.Count > 0)
		{
			val = inactivePool[inactivePool.Count - 1];
			inactivePool.RemoveAt(inactivePool.Count - 1);
		}
		else
		{
			val = UnityEngine.Object.Instantiate(prefab, parent);
			attachedUI.InitUI(val);
			onInit?.Invoke(val);
		}
		val.gameObject.SetActive(value: true);
		val.gameObject.transform.SetParent(parent);
		if (lastSibling)
		{
			val.gameObject.transform.SetAsLastSibling();
		}
		else
		{
			val.gameObject.transform.SetAsFirstSibling();
		}
		activePool.Add(val);
		if (overrideSetup != null)
		{
			overrideSetup(val);
		}
		else
		{
			val.Setup(data);
		}
		onSetup?.Invoke(val);
		return val;
	}

	public override void Despawn(TCellUI obj)
	{
		if (activePool.Contains(obj))
		{
			obj.Unset();
			obj.gameObject.SetActive(value: false);
			activePool.Remove(obj);
			inactivePool.Add(obj);
		}
	}

	public TCellUI GetActiveElement(TSetupData setupData)
	{
		return activePool.Find((TCellUI c) => c.setupData.Equals(setupData));
	}

	internal override void Dispose()
	{
		DespawnAll();
		foreach (TCellUI item in inactivePool)
		{
			attachedUI.DisposeUI(item);
		}
		inactivePool.Clear();
	}

	public void Sync(IList<TSetupData> list)
	{
		if (list == null)
		{
			DespawnAll();
			return;
		}
		foreach (TSetupData setupData in list)
		{
			if (setupData != null && !activePool.Find((TCellUI c) => c.setupData.Equals(setupData)))
			{
				Spawn(setupData).SetDirty();
			}
		}
		syncToDespawnBuffer.Clear();
		foreach (TCellUI item in activePool)
		{
			if (!list.Contains(item.setupData))
			{
				syncToDespawnBuffer.Add(item);
			}
		}
		foreach (TCellUI item2 in syncToDespawnBuffer)
		{
			Despawn(item2);
		}
		syncToDespawnBuffer.Clear();
		Order((List<TCellUI> l) => l.OrderBy((TCellUI c) => list.IndexOf(c.setupData)).ToList());
	}

	public void Sync<T>(IList<T> list, IDataConvertor<T> convertor)
	{
		if (list == null)
		{
			DespawnAll();
			return;
		}
		foreach (T element in list)
		{
			if (element != null && !activePool.Any((TCellUI c) => convertor.FromSetupData(c.setupData).Equals(element)))
			{
				Spawn(convertor.ToSetupData(element)).SetDirty();
			}
		}
		syncToDespawnBuffer.Clear();
		foreach (TCellUI cell in activePool)
		{
			if (!list.Any((T e) => e.Equals(convertor.FromSetupData(cell.setupData))))
			{
				syncToDespawnBuffer.Add(cell);
			}
		}
		foreach (TCellUI item in syncToDespawnBuffer)
		{
			Despawn(item);
		}
		syncToDespawnBuffer.Clear();
		Order((List<TCellUI> l) => l.OrderBy((TCellUI c) => list.IndexOf(convertor.FromSetupData(c.setupData))).ToList());
	}

	public void Order(Func<List<TCellUI>, List<TCellUI>> fun)
	{
		if (fun != null)
		{
			activePool = fun(activePool);
			for (int i = 0; i < activePool.Count; i++)
			{
				activePool[i].transform.SetAsLastSibling();
			}
		}
	}

	public void ConfigInfoPanel(ICommonInfoPanel panel)
	{
		onSetup = (Action<TCellUI>)Delegate.Combine(onSetup, new Action<TCellUI>(SetUp));
		void SetUp(TCellUI display)
		{
			if (display.GetModule<InfoModule>() == null)
			{
				Debug.LogError("no info module " + GetTransformPath(display.gameObject.transform));
			}
			else
			{
				display.GetModule<InfoModule>().infoPanel = panel;
			}
		}
	}

	private static string GetTransformPath(Transform transform)
	{
		string text = transform.name;
		while (transform.parent != null)
		{
			transform = transform.parent;
			text = transform.name + "/" + text;
		}
		return text;
	}

	public void Spawn(IEnumerable<TSetupData> setupDatas)
	{
		if (setupDatas == null)
		{
			return;
		}
		foreach (TSetupData setupData in setupDatas)
		{
			Spawn(setupData);
		}
	}

	public override void DestroyInActive()
	{
		foreach (TCellUI item in inactivePool)
		{
			attachedUI.DisposeUI(item);
			UnityEngine.Object.Destroy(item.gameObject);
		}
		inactivePool.Clear();
	}
}
