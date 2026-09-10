public class RemoveInfluenceAction : ExecuteAction
{
	public Influence influence;

	public bool preserveWillPowerCost;

	public RemoveInfluenceAction(Influence influence, bool preserveWillPowerCost = false)
	{
		this.influence = influence;
		this.preserveWillPowerCost = preserveWillPowerCost;
	}

	protected override void Update()
	{
		if (influence.owner != null)
		{
			BattleFieldTransform owner = influence.owner;
			if (owner.influences.Remove(influence) && preserveWillPowerCost && owner == gameState?.playerState?.bft && influence.willPowerCost > 0)
			{
				PlayerGameStateInfo playerState = gameState.playerState;
				playerState.additionalWillPowerCost = (int)playerState.additionalWillPowerCost + influence.willPowerCost;
			}
			influence.owner = null;
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerRemoveInfluence(influence);
			});
		}
	}
}
