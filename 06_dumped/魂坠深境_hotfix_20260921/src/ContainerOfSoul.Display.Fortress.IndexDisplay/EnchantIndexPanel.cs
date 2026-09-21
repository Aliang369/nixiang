using System;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class EnchantIndexPanel : IndexPagePanel
{
	[SerializeField]
	private EnchantDisplay enchantDisplayPrefab;

	[SerializeField]
	private Transform enchantCellParent;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	public CellUIPrefabPool<EnchantDisplay, Enchant> enchantCellPool;

	public override string TASubPanelTagName => "档案馆-图鉴-附魔";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.Enchant;
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data as Enchant)?.def);
	}

	protected override void Init()
	{
		enchantCellPool = GetPrefabPool<EnchantDisplay, Enchant>(enchantDisplayPrefab, enchantCellParent);
	}

	protected override void UpdateDisplay()
	{
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay((from def in GetIndexPageAllContent<EnchantDef>()
			select new Enchant(def)).ToList(), enchantCellPool, layoutGroup);
		layoutGroup.RefreshLayout();
	}
}
