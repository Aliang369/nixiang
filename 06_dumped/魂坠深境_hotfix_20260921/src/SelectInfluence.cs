using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectInfluence : Effect
{
	public GameEvent.EventBFTType bftType;

	[LocalizeText]
	public string titleText;

	public Effect afterEffect;

	public InfluenceDefGenerator influenceDefGenerator;

	[SerializeField]
	private CheckNode confirmable;

	[SerializeField]
	private bool isSingleSelect;

	[SerializeField]
	private bool requireSelection;

	[SerializeField]
	private CheckNode selectable;

	[SerializeField]
	private CheckNode shouldCancel;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(bftType);
		if (!eventBFT.isPlayer)
		{
			return NodeCastResult.Failure;
		}
		List<InfluenceDef> source = influenceDefGenerator?.GenerateInfluenceDefs(eve);
		if (source.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		List<Influence> pool = source.Select((InfluenceDef i) => new Influence(i)).ToList();
		ElementSelectPanel<Influence>.SelectInfo info = ElementSelectPanel<Influence>.SelectInfo.GenerateNodeSelectInfo(eve, pool, shouldCancel, selectable, confirmable, null, null, () => false, isSingleSelect);
		if (requireSelection)
		{
			ElementSelectPanel<Influence>.SelectInfo selectInfo = info;
			selectInfo.canConfirm = (Func<bool>)Delegate.Combine(selectInfo.canConfirm, (Func<bool>)(() => info.output.Count > 0));
		}
		info.panelHintGetter = () => SelectPanelHelper.FormatSelectPanelHint(eve, titleText, out info.generator);
		info.selectorBft = eventBFT;
		info.onConfirm = delegate
		{
			List<Influence> selectedValues = eve.GetSelectedValues<Influence>();
			eve.SetSelectedValues(info.output);
			afterEffect?.Cast(eve);
			eve.SetSelectedValues(selectedValues);
		};
		eve.gameState.actionManager.EnqueueAction(new SelectAction<Influence>(info));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { afterEffect, influenceDefGenerator, confirmable, selectable, shouldCancel };
	}
}
