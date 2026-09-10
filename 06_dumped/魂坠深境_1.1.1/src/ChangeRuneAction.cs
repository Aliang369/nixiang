public class ChangeRuneAction : ExecuteAction
{
	public int delta;

	public RuneDef runeDef;

	public Item source;

	public bool sourceDeckChange;

	public Item target;

	public ChangeRuneAction(Item target, RuneDef runeDef, int delta, Item source, bool sourceDeckChange = false)
	{
		this.delta = delta;
		this.runeDef = runeDef;
		this.target = target;
		this.source = source;
		this.sourceDeckChange = sourceDeckChange;
	}

	public ChangeRuneAction(Item target, Rune rune, Item source, bool sourceDeckChange = false)
	{
		delta = rune.value;
		runeDef = rune.def;
		this.target = target;
		this.source = source;
		this.sourceDeckChange = sourceDeckChange;
	}

	protected override void Update()
	{
		if (!(runeDef == null) && target != null)
		{
			Rune rune = target.Runes.Find((Rune e) => e.def == runeDef);
			if (rune == null && delta >= 0)
			{
				AddRune();
			}
			else if (rune != null)
			{
				ChangeRuneValue(rune);
			}
			if (sourceDeckChange && target.sourceDeckItem != null)
			{
				EnqueueAction(new ChangeRuneAction(target.sourceDeckItem, runeDef, delta, source));
			}
		}
	}

	private void AddRune()
	{
		Rune rune = new Rune(runeDef, delta);
		target.AddRune(rune);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAddRune(target, rune);
		});
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerRuneValueChange(target, rune, delta);
		});
		ItemContainer owner = target.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.Deck)
		{
			BattleFieldTransform ownerBft = target.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer)
			{
				gameState.recordManager.TrackJournalRecord(new MintageRuneRecord(target, rune));
			}
		}
	}

	private void ChangeRuneValue(Rune existed)
	{
		if (existed.value + delta <= 0)
		{
			target.RemoveRune(existed);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerRuneValueChange(target, existed, delta);
			});
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerRemoveRune(target, existed);
			});
		}
		else if (existed.def.stacking)
		{
			target.ChangeRuneValue(existed, existed.value + delta);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerRuneValueChange(target, existed, delta);
			});
		}
	}
}
