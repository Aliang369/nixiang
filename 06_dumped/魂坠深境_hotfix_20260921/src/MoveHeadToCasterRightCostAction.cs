public class MoveHeadToCasterRightCostAction : ExecuteAction
{
	private readonly SpellCastInfo castInfo;

	private readonly Effect costNode;

	private readonly Item targetItem;

	public MoveHeadToCasterRightCostAction(Item targetItem, SpellCastInfo castInfo, Effect costNode)
	{
		this.targetItem = targetItem;
		this.castInfo = castInfo;
		this.costNode = costNode;
	}

	protected override void Update()
	{
		if (targetItem == null || castInfo == null)
		{
			return;
		}
		gameState.gameEventManager.TriggerBeforeCost(targetItem, castInfo);
		if (!gameState.gameEventManager.TriggerResolveSpellCostConsumeInfo(targetItem, castInfo.spellItem, GameEvent.TriggerType.Real).reject)
		{
			if (costNode != null)
			{
				costNode.Cast(castInfo.GenerateSpellCastEvent(GameEvent.TriggerType.Real));
			}
			else
			{
				EnqueueAction(new MoveHeadToCasterRightAction(targetItem));
			}
		}
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAfterCost(targetItem, castInfo);
		}));
	}
}
