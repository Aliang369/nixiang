using System.Collections.Generic;
using UnityEngine;

public class SelectEnchant : Effect
{
	public GameEvent.EventBFTType bftType;

	public EnchantDefGenerator enchantGenerator;

	public ValueGenerator deltaValueGenerator;

	public string titleText;

	public Effect afterEffect;

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
		List<EnchantDef> obj = enchantGenerator?.GenerateEnchants(eve);
		int value = ((deltaValueGenerator == null) ? 1 : deltaValueGenerator.GenerateValue(eve).Value);
		List<Enchant> list = new List<Enchant>();
		foreach (EnchantDef item in obj)
		{
			list.Add(new Enchant(item, value));
		}
		ElementSelectPanel<Enchant>.SelectInfo info = ElementSelectPanel<Enchant>.SelectInfo.GenerateNodeSelectInfo(eve, list, shouldCancel, selectable, confirmable, null, null, () => false, isSingleSelect);
		info.panelHintGetter = () => SelectPanelHelper.FormatSelectPanelHint(eve, titleText, out info.generator);
		info.selectorBft = eventBFT;
		info.onConfirm = delegate
		{
			List<Enchant> selectedValues = eve.GetSelectedValues<Enchant>();
			eve.SetSelectedValues(info.output);
			afterEffect?.Cast(eve);
			eve.SetSelectedValues(selectedValues);
		};
		eve.gameState.actionManager.EnqueueAction(new SelectAction<Enchant>(info));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { afterEffect, enchantGenerator, deltaValueGenerator, confirmable, selectable, shouldCancel };
	}
}
