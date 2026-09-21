public class AddInfluenceAction : ExecuteAction
{
	public Influence influece;

	public BattleFieldTransform bft;

	public AddInfluenceAction(Influence influece, BattleFieldTransform bft)
	{
		this.influece = influece;
		this.bft = bft;
	}

	protected override void Update()
	{
		bft.influences.Add(influece);
		influece.owner = bft;
		EnqueueAction(new WaitFrameAction(1));
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAddInfluence(influece);
			gameState.recordManager.TrackJournalRecord(new GetBlessRecord(influece));
			if (bft == gameState.playerState.bft && PlayerGameStateInfo.ShouldSyncMissingWillPowerPenalty(influece))
			{
				gameState.playerState.SyncMissingWillPowerPenaltyInfluences();
			}
		}));
	}
}
