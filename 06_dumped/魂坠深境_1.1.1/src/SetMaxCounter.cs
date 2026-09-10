using System.Collections.Generic;

public class SetMaxCounter : Effect
{
	public ItemTargetSelector target;

	public ValueGenerator counterGenerator;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		int? num = counterGenerator?.GenerateValue(eve);
		if (targetItems == null || !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in targetItems)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new SetMaxCounterAction(num.Value, item, eve.Caster));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, counterGenerator };
	}
}
