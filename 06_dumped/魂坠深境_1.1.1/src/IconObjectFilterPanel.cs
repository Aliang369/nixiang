using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;
using UnityEngine.Serialization;

public class IconObjectFilterPanel : UIBase
{
	public struct IconFilterData(object target, Sprite icon, string title, Func<bool?> shouldShowRedDot = null)
	{
		public object target = target;

		public Sprite icon = icon;

		public string title = title;

		public Func<bool?> shouldShowRedDot = shouldShowRedDot;
	}

	[FormerlySerializedAs("characterClassFilter")]
	[SerializeField]
	private IconObjectFilter filterPrefab;

	[SerializeField]
	public bool canUnselect = true;

	[NonSerialized]
	private CellUIPrefabPool<IconObjectFilter, IconFilterData> filterPool;

	[NonSerialized]
	public IconObjectFilter selected;

	[NonSerialized]
	public Action onChangeSelected;

	protected override void Init()
	{
		GetPrefabPool(ref filterPool, filterPrefab);
	}

	public void Sync(List<IconFilterData> characterClassDefs)
	{
		filterPool.Sync(characterClassDefs);
	}

	public IconObjectFilter GetFilter(object target)
	{
		return filterPool.GetActiveElements().FirstOrDefault((IconObjectFilter d) => d.setupData.target == target);
	}

	public void OnChangeSelected()
	{
		onChangeSelected?.Invoke();
		parent.SetDirty();
	}
}
