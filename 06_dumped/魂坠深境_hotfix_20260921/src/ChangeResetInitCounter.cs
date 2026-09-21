using System.Collections.Generic;

public class ChangeResetInitCounter : Effect
{
	public ItemTargetSelector target;

	public ValueGenerator counterGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		int? value = counterGenerator.GetValue(eve, 0);
		if (targetItems == null || !value.HasValue)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in targetItems)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			int counter = (item.UsesDefenceCounterLimit ? CombatValueLimit.Clamp((long)(int)item.resetInitCounter + (long)value.Value) : ((int)item.resetInitCounter + value.Value));
			Effect.EnqueueAction(eve, new ChangeResetInitCounterAction(counter, item));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(counterGenerator.GenerateValue(eve))
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, counterGenerator };
	}
}
