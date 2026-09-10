using System.Collections.Generic;

public class SetItemSpellIndex : Effect
{
	public ValueGenerator indexGenerator;

	public ItemTargetSelector targetSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int index = ((indexGenerator != null) ? indexGenerator.GenerateValue(eve).Value : 0);
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new SetSpellIndexAction(item, index));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, indexGenerator };
	}
}
