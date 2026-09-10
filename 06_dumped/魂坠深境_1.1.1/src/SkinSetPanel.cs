using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

public class SkinSetPanel : UIBase
{
	[SerializeField]
	private SkinSetDisplay prefab;

	[NonSerialized]
	private CellUIPrefabPool<SkinSetDisplay, ItemSkinSetDef> pool;

	protected override void Init()
	{
		pool = GetPrefabPool<SkinSetDisplay, ItemSkinSetDef>(prefab);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(SetDirty);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		List<ItemSkinSetDef> validAssets = UIBase.fortressState.applicationState.assetCatalog.skinSetCatalog.ValidAssets;
		validAssets.Sort((ItemSkinSetDef a, ItemSkinSetDef b) => a.SOindex.CompareTo(b.SOindex));
		pool.Sync(validAssets);
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(SetDirty);
	}
}
