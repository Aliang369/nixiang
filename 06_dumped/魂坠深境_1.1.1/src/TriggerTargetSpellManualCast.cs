using System.Collections.Generic;

public class TriggerTargetSpellManualCast : Effect
{
	public enum TargetingType
	{
		Default,
		Random
	}

	public ItemTargetSelector toTriggerItemSelector;

	public TargetingType targetingType;

	public bool withAnim = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		PlayerControlInfo playerControlInfo = eve.gameState?.UIManager?.playerItemArea?.controlInfo;
		if (playerControlInfo == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list = toTriggerItemSelector?.GetTargetItems(eve);
		if (list == null || list.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			if (!item.HasSpell)
			{
				continue;
			}
			Item targetItem = null;
			if (item.spellNeedsTarget)
			{
				ItemSpell currentSpell = item.CurrentSpell;
				switch (targetingType)
				{
				case TargetingType.Default:
					targetItem = currentSpell.requirement?.GetDefaultTargetItem(item, eve.gameState);
					break;
				case TargetingType.Random:
					targetItem = currentSpell.requirement?.GetRandomTargetItem(item, eve.gameState);
					break;
				}
			}
			PlayerControlInfo.SpellRequest request = new PlayerControlInfo.SpellRequest
			{
				spellItem = item,
				targetItem = targetItem,
				withAnim = withAnim
			};
			playerControlInfo.EnqueueSpellRequest(request);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { toTriggerItemSelector };
	}
}
