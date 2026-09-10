using System.Collections.Generic;
using UnityEngine;

public class SelectSkill : Effect
{
	public GameEvent.EventBFTType bftType;

	public string titleText;

	public Effect afterEffect;

	[SerializeField]
	private SkillSelector skillSelector;

	[SerializeField]
	private CheckNode confirmable;

	[SerializeField]
	private bool isSingleSelect;

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
		List<GameSkill> pool = skillSelector?.GetTargetSkills(eve);
		ElementSelectPanel<GameSkill>.SelectInfo info = ElementSelectPanel<GameSkill>.SelectInfo.GenerateNodeSelectInfo(eve, pool, shouldCancel, selectable, confirmable, null, null, () => false, isSingleSelect);
		info.selectorBft = eventBFT;
		info.panelHintGetter = () => SelectPanelHelper.FormatSelectPanelHint(eve, titleText, out info.generator);
		info.onConfirm = delegate
		{
			List<GameSkill> selectedValues = eve.GetSelectedValues<GameSkill>();
			eve.SetSelectedValues(info.output);
			afterEffect?.Cast(eve);
			eve.SetSelectedValues(selectedValues);
		};
		eve.gameState.actionManager.EnqueueAction(new SelectAction<GameSkill>(info));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { skillSelector, afterEffect, confirmable, selectable, shouldCancel };
	}
}
