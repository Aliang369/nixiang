using System.Collections.Generic;

public class MoveSlotToward : Effect
{
	public ValueGenerator deltaGenerator;

	public ItemTargetSelector pivotSelector;

	public ItemTargetSelector targetSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int valueOrDefault = (deltaGenerator?.GenerateValue(eve)).GetValueOrDefault();
		Item item = targetSelector?.GetSingleItem(eve);
		Item item2 = pivotSelector?.GetSingleItem(eve);
		if (item2 == null || item == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, item);
		Effect.EnqueueAction(eve, new MoveSlotTowardAction(item, item2, eve.CasterItem, valueOrDefault));
		hitEffect?.Play(eve, item);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, pivotSelector, deltaGenerator };
	}
}
