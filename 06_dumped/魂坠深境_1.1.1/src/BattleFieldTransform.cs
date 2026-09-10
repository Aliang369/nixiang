using System.Collections.Generic;

public class BattleFieldTransform : IBftOwnable
{
	public BattleFieldTransformDisplay attachDisplay;

	public CharacterAI characterAI;

	[SaveField(null, null)]
	public ItemContainer companionContainer;

	[SaveField(null, null)]
	public bool crystalFreeze;

	[SaveField(null, null)]
	public ItemContainer deckContainer;

	[SaveField(null, null)]
	public ItemContainer drawPileItemContainer;

	[SaveField(null, null)]
	public ItemContainer exilePileItemContainer;

	[SaveField(null, null)]
	public ProtectedInt extraHealthPoint = 0;

	[SaveField(null, null)]
	public GameStateInfo gameState;

	[SaveField(null, null)]
	public ProtectedInt health = 0;

	[SaveField(null, null)]
	public bool isPlayer;

	[SaveField(null, null)]
	public ProtectedInt maxHealth = 0;

	[SaveField(null, null)]
	public bool skipNextTurn;

	[SaveField(null, null)]
	public ItemContainer tableContainer;

	[SaveField(null, null)]
	public ProtectedInt tempHealth = 0;

	[SaveField(null, null)]
	public ProtectedInt turnDrawItemCount = 0;

	[SaveField(null, null)]
	public ProtectedInt handLimitBonus = 0;

	[SaveField(null, null)]
	public ItemContainer wastePileItemContainer;

	[SaveField(null, null)]
	public List<Influence> influences;

	public int FirstTurnDrawItemCount => (int)turnDrawItemCount + 2;

	public bool isEnemy => !isPlayer;

	public BftUIDisplay uiDisplay
	{
		get
		{
			if (!isPlayer)
			{
				return gameState?.UIManager?.enemyBftUIDisplay;
			}
			return gameState?.UIManager?.playerBftUIDisplay;
		}
	}

	BattleFieldTransform IBftOwnable.OwnerBft => this;

	public void Attach(CharacterAI ai)
	{
		characterAI = ai;
		attachDisplay = ai.component.BftDisplay;
		attachDisplay.attachBft = this;
		ai.bft = this;
	}

	private BattleFieldTransform()
	{
	}

	public BattleFieldTransform(GameStateInfo gameState, bool isPlayer)
	{
		this.gameState = gameState;
		this.isPlayer = isPlayer;
		EnsureRuntimeContainers(gameState);
	}

	public void EnsureRuntimeContainers(GameStateInfo runtimeGameState = null)
	{
		if (runtimeGameState != null)
		{
			gameState = runtimeGameState;
		}
		if (deckContainer == null)
		{
			deckContainer = new ItemContainer(this, ItemContainer.ContainerType.Deck);
		}
		if (tableContainer == null)
		{
			tableContainer = new ItemContainer(this, ItemContainer.ContainerType.Table);
		}
		if (drawPileItemContainer == null)
		{
			drawPileItemContainer = new ItemContainer(this, ItemContainer.ContainerType.DrawPile);
		}
		if (wastePileItemContainer == null)
		{
			wastePileItemContainer = new ItemContainer(this, ItemContainer.ContainerType.WastePile);
		}
		if (companionContainer == null)
		{
			companionContainer = new ItemContainer(this, ItemContainer.ContainerType.Companion);
		}
		if (exilePileItemContainer == null)
		{
			exilePileItemContainer = new ItemContainer(this, ItemContainer.ContainerType.ExilePile);
		}
		if (influences == null)
		{
			influences = new List<Influence>();
		}
	}

	public int GetTotalHealth()
	{
		if (!isEnemy)
		{
			return health;
		}
		return (int)health + (int)extraHealthPoint * (int)maxHealth;
	}

	public List<Item> GetAllRealItems()
	{
		List<Item> list = new List<Item>();
		list.AddRange(tableContainer.GetAllItems());
		list.AddRange(companionContainer.GetAllItems());
		list.AddRange(drawPileItemContainer.GetAllItems());
		list.AddRange(wastePileItemContainer.GetAllItems());
		return list;
	}

	public List<Item> GetAllCoolDownItems()
	{
		List<Item> allRealItems = GetAllRealItems();
		if (exilePileItemContainer != null)
		{
			allRealItems.AddRange(exilePileItemContainer.GetAllItems());
		}
		return allRealItems;
	}

	public void ClearExilePile()
	{
		EnsureRuntimeContainers(gameState);
		exilePileItemContainer.ClearItemsSilently();
	}

	public ConvertablePivot GetUIPivot(GameStateInfo gameState)
	{
		if (isPlayer)
		{
			return gameState.UIManager.playerBftUIDisplay.heartPivot;
		}
		return gameState.UIManager.enemyBftUIDisplay.pivot;
	}

	public ConvertablePivot Get3DNFXPivot(NFXSetup nfx)
	{
		return nfx.pivotType switch
		{
			NFXSetup.BFTPivotType.Lower => attachDisplay.effectLowerPivot, 
			NFXSetup.BFTPivotType.Upper => attachDisplay.effectUpperPivot, 
			_ => attachDisplay.effectUpperPivot, 
		};
	}

	public ConvertablePivot GetHitPivot()
	{
		return attachDisplay.effectHitPivot;
	}
}
