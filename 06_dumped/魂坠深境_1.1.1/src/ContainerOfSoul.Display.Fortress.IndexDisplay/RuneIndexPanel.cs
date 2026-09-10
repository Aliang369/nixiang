using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class RuneIndexPanel : IndexPagePanel
{
	[SerializeField]
	private RuneDisplay runeDisplayPrefab;

	[SerializeField]
	private Transform runeCellParent;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	public CellUIPrefabPool<RuneDisplay, Rune> runeDisplayPool;

	public override string TASubPanelTagName => "档案馆-图鉴-印记";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.Rune;
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data as Rune)?.def);
	}

	protected override void UpdateDisplay()
	{
		List<Rune> list = (from def in GetIndexPageAllContent<RuneDef>()
			select new Rune(def, def.attachGenerationInfo?.stack ?? 1)).ToList();
		list.Sort((Rune a, Rune b) => RuneDef.RuneDefIndexComparer(a.def, b.def, UIBase.applicationState));
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, runeDisplayPool, layoutGroup);
		layoutGroup.RefreshLayout();
	}

	protected override void Init()
	{
		runeDisplayPool = GetPrefabPool<RuneDisplay, Rune>(runeDisplayPrefab, runeCellParent);
	}
}
