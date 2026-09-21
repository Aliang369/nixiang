using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.IndexDisplay;

public class SkillGemIndexPanel : IndexPagePanel
{
	[SerializeField]
	private SkillGemDisplay skillGemDisplayPrefab;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private SkillGemInfoPanel skillGemInfoPanel;

	[NonSerialized]
	private CellUIPrefabPool<SkillGemDisplay, SkillGem> skillGemDisplayPool;

	public override string TASubPanelTagName => "档案馆-图鉴-护符宝石";

	public override IndexBookPage GetIndexPageType()
	{
		return IndexBookPage.SkillGem;
	}

	public override void ClearTransientSelection()
	{
		skillGemInfoPanel?.HideAllCommonInfo();
	}

	public override bool TryFocusFirstClaimableSingleReward()
	{
		return TryFocusFirstClaimableSingleReward(layoutGroup, (object data) => (data as SkillGem)?.def);
	}

	protected override void Init()
	{
		InitUI(skillGemInfoPanel);
		GetPrefabPool(ref skillGemDisplayPool, skillGemDisplayPrefab, null, skillGemInfoPanel);
	}

	protected override void UpdateDisplay()
	{
		List<SkillGem> list = (from def in GetIndexPageAllContent<SkillGemDef>()
			select new SkillGem(def)).ToList();
		list.Sort((SkillGem a, SkillGem b) => SkillGemDef.SkillGemDefIndexComparer(a.def, b.def, UIBase.applicationState));
		ScrollRectGridLayoutGroupExtends.UpdateSequenceDisplay(list, skillGemDisplayPool, layoutGroup);
		layoutGroup.RefreshLayout();
	}
}
