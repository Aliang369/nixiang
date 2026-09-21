using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.UI;

[Serializable]
public class CellUIPrefabPoolConfig<TCellUI, TSetupData> where TCellUI : CellUIBase<TSetupData>
{
	[SerializeField]
	public TCellUI prefab;

	[SerializeField]
	public RectTransform parent;

	[NonSerialized]
	public CellUIPrefabPool<TCellUI, TSetupData> pool;

	public TCellUI Spawn(TSetupData data, Transform parent = null, Action<TCellUI> overrideSetup = null, bool lastSibling = true)
	{
		return pool.Spawn(data, parent, overrideSetup, lastSibling);
	}

	public void DespawnAll()
	{
		pool.DespawnAll();
	}

	public void Sync(IList<TSetupData> list)
	{
		pool.Sync(list);
	}
}
