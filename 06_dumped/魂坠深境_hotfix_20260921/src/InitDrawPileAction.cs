using GameRandom;

public class InitDrawPileAction : ExecuteAction
{
	public BattleFieldTransform bft;

	public InitDrawPileAction(BattleFieldTransform bft)
	{
		this.bft = bft;
	}

	protected override void Update()
	{
		ItemContainer drawPileItemContainer = bft.drawPileItemContainer;
		ItemContainer wastePileItemContainer = bft.wastePileItemContainer;
		ItemContainer deckContainer = bft.deckContainer;
		foreach (Item allItem in drawPileItemContainer.GetAllItems())
		{
			EnqueueAction(new ExileItemAction(allItem, allItem));
		}
		foreach (Item item in ShouldSkipInitialShuffle() ? deckContainer.GetAllItems() : deckContainer.GetAllItems().Shuffle(gameState.randomManager.NextDouble))
		{
			Item target = item.Copy(spawnFromDeck: true);
			if (item.blackboard.InitWastePile)
			{
				EnqueueAction(new SpawnItemAction(target, wastePileItemContainer));
			}
			else
			{
				EnqueueAction(new SpawnItemAction(target, drawPileItemContainer));
			}
		}
	}

	private bool ShouldSkipInitialShuffle()
	{
		if (bft.isPlayer)
		{
			return gameState.IsCharacterTrialGame;
		}
		return false;
	}
}
