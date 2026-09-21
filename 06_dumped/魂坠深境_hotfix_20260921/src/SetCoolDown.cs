using System.Collections.Generic;

public class SetCoolDown : Effect
{
	public ValueGenerator valueGenerator;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in targetItems)
		{
			if (item != null && item.HasSpell)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				Effect.EnqueueAction(eve, new SetCoolDownAction(item, eve.CasterItem, valueGenerator?.GenerateValue(eve) ?? 1));
				hitEffect?.Play(eve, item);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, valueGenerator };
	}
}
