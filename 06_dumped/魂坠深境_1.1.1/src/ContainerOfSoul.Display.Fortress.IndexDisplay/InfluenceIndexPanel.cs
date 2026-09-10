using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class InfluenceIndexPanel : IndexPagePanel
{
	[SerializeField]
	private Transform influenceCellParent;

	[SerializeField]
	private InfluenceDisplay influenceDisplayPrefab;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> influenceCellPool;

	public override string TASubPanelTagName => "档案馆-图鉴-祝福";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.Influence;
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data as Influence)?.def);
	}

	protected override void Init()
	{
		influenceCellPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceDisplayPrefab, influenceCellParent);
	}

	protected override void UpdateDisplay()
	{
		List<Influence> list = (from def in GetIndexPageAllContent<InfluenceDef>()
			select new Influence(def)).ToList();
		list.Sort((Influence a, Influence b) => InfluenceDef.InfluenceDefIndexComparer(a.def, b.def, UIBase.applicationState));
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, influenceCellPool, layoutGroup);
		layoutGroup.RefreshLayout();
	}

	public List<ICTAsset> GetIndexContent()
	{
		return new List<ICTAsset>(GetIndexPageAllContent<InfluenceDef>());
	}
}
