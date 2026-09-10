using System.Collections.Generic;
using UnityEngine;

public class SelectRune : Effect
{
	private class RuneInfo
	{
		public RuneDef runeDef;

		public ValueGenerator deltaValueGenerator;
	}

	public GameEvent.EventBFTType bftType;

	public string titleText;

	public Effect afterEffect;

	[SerializeField]
	private List<RuneInfo> runeInfos;

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
		List<Rune> list = new List<Rune>();
		foreach (RuneInfo runeInfo in runeInfos)
		{
			list.Add(GenerateRune(eve, runeInfo));
		}
		ElementSelectPanel<Rune>.SelectInfo info = ElementSelectPanel<Rune>.SelectInfo.GenerateNodeSelectInfo(eve, list, shouldCancel, selectable, confirmable, null, null, () => false, isSingleSelect);
		info.panelHintGetter = () => SelectPanelHelper.FormatSelectPanelHint(eve, titleText, out info.generator);
		info.selectorBft = eventBFT;
		info.onConfirm = delegate
		{
			List<Rune> selectedValues = eve.GetSelectedValues<Rune>();
			eve.SetSelectedValues(info.output);
			afterEffect?.Cast(eve);
			eve.SetSelectedValues(selectedValues);
		};
		eve.gameState.actionManager.EnqueueAction(new SelectAction<Rune>(info));
		return NodeCastResult.Success;
	}

	private Rune GenerateRune(GameEvent eve, RuneInfo runeInfo)
	{
		if (runeInfo.runeDef == null)
		{
			return null;
		}
		int value = ((runeInfo.deltaValueGenerator == null) ? 1 : runeInfo.deltaValueGenerator.GenerateValue(eve).Value);
		return new Rune(runeInfo.runeDef, value);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { afterEffect, confirmable, selectable, shouldCancel };
	}
}
