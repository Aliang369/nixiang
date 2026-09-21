using System.Collections.Generic;

public class ChangeMaxCounter : Effect
{
	public ItemTargetSelector target;

	public ValueGenerator counterGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	public bool baseAdd = true;

	public bool keepPositive = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Success;
		}
		int value = counterGenerator.GenerateValue(eve).Value;
		foreach (Item item in targetItems)
		{
			bool flag = (int?)item?.maxCounter > 0;
			if ((item != null) & flag)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				int num = (item.UsesDefenceCounterLimit ? CombatValueLimit.Clamp((long)(int)item.maxCounter + (long)value) : ((int)item.maxCounter + value));
				if (keepPositive && num <= 0)
				{
					num = 1;
				}
				Effect.EnqueueAction(eve, new SetMaxCounterAction(num, item, eve.Caster));
				if (baseAdd && value > 0)
				{
					Effect.EnqueueAction(eve, new ChangeCounterAction(value, item, eve.CasterItem));
				}
				hitEffect?.Play(eve, item);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, counterGenerator };
	}
}
