using System.Collections.Generic;

public class ChangeTreasureValue : Effect
{
	public ItemTargetSelector target;

	public ValueGenerator valueGenerator;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = target?.GetTargetItems(eve);
		int? delta = valueGenerator.GetValue(eve);
		if (!delta.HasValue || list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				ChangeValue(item, delta.Value);
			}));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, valueGenerator };
	}

	private void ChangeValue(Item item, int value)
	{
		item.treasureValue += value;
	}
}
