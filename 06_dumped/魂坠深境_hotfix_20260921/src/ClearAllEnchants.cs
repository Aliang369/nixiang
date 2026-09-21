using System.Collections.Generic;

public class ClearAllEnchants : Effect
{
	public ItemTargetSelector targetSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		object source = ((object)eve.CasterItem) ?? ((object)eve.CasterSkill);
		foreach (Item item in list)
		{
			if (item != null)
			{
				Effect.EnqueueAction(eve, new ClearAllEnchantsAction(item, source));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return targetSelector?.GetNodeDisplayInfo(eve);
	}
}
