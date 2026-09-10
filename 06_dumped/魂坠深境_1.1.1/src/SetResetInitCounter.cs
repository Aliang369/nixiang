using System.Collections.Generic;

public class SetResetInitCounter : Effect
{
	public ValueGenerator counterGenerator;

	public ItemTargetSelector target;

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
			Effect.EnqueueAction(eve, new ChangeResetInitCounterAction(value.Value, item));
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
