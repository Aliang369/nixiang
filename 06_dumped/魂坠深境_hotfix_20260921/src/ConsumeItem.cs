using System.Collections.Generic;

public class ConsumeItem : Effect
{
	public ItemTargetSelector targetSelector;

	public bool consumeRightToLeft;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		if (consumeRightToLeft)
		{
			list.Reverse();
		}
		foreach (Item item in list)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect, null, hitEffect));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
