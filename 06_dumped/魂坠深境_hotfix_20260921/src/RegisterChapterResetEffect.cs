using System.Collections.Generic;

public class RegisterChapterResetEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public ValueGenerator maxCounterDeltaGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int valueOrDefault = (maxCounterDeltaGenerator?.GenerateValue(eve)).GetValueOrDefault();
		foreach (Item item in list)
		{
			if (item != null)
			{
				Effect.EnqueueAction(eve, new RegisterChapterResetEffectAction(item, valueOrDefault));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, maxCounterDeltaGenerator };
	}
}
