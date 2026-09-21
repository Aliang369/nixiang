using System;
using System.Collections.Generic;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class SkinSetIndexPanel : IndexPagePanel
{
	[SerializeField]
	private SkinSetDisplay prefab;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	public CellUIPrefabPool<SkinSetDisplay, ItemSkinSetDef> pool;

	public override string TASubPanelTagName => "档案馆-图鉴-卡牌形态";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.SkinSet;
	}

	public override bool WithIndexBookRewardToGet()
	{
		return false;
	}

	protected override void Init()
	{
		pool = GetPrefabPool<SkinSetDisplay, ItemSkinSetDef>(prefab, prefab.transform.parent);
	}

	protected override void UpdateDisplay()
	{
		List<ItemSkinSetDef> validAssets = UIBase.fortressState.applicationState.assetCatalog.skinSetCatalog.ValidAssets;
		validAssets.Sort((ItemSkinSetDef a, ItemSkinSetDef b) => a.SOindex.CompareTo(b.SOindex));
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(validAssets, pool, layoutGroup);
		layoutGroup.RefreshLayout();
	}
}
