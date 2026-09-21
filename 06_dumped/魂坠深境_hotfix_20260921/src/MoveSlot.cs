using System.Collections.Generic;

public class MoveSlot : Effect
{
	public enum MoveType
	{
		To,
		Delta
	}

	public ItemTargetSelector targetSelector;

	public MoveType type;

	public ValueGenerator valueGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int num = ((valueGenerator != null) ? valueGenerator.GenerateValue(eve).Value : 0);
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			if (type == MoveType.To)
			{
				Effect.EnqueueAction(eve, new MoveToSlotAction(num, item, eve.Caster));
			}
			else
			{
				Effect.EnqueueAction(eve, new MoveDeltaSlotAction(num, item, eve.Caster));
			}
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator, targetSelector };
	}
}
