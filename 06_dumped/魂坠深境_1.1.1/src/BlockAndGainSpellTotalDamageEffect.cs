using System.Collections.Generic;

public sealed class BlockAndGainSpellTotalDamageEffect : Effect
{
	public CheckNode upgradedCheck;

	public ItemTargetSelector targetSelector;

	public ItemSpellTotalDamageValueGenerator totalDamageGenerator;

	public ChangeEnchant blockEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (targetSelector?.GetSingleItem(eve) == null)
		{
			return NodeCastResult.Failure;
		}
		int valueOrDefault = (totalDamageGenerator?.GenerateValue(eve)).GetValueOrDefault();
		blockEffect?.Cast(eve);
		if (valueOrDefault > 0)
		{
			Effect.EnqueueAction(eve, new ChangeTempHealthAction(valueOrDefault, eve.GetEventBFT(GameEvent.EventBFTType.Owner)));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, totalDamageGenerator, blockEffect, upgradedCheck };
	}
}
