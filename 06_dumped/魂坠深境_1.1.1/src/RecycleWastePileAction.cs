internal class RecycleWastePileAction : GameAction
{
	public BattleFieldTransform bft;

	private RecycleWastePileVFX vfx;

	public RecycleWastePileAction(BattleFieldTransform bft)
	{
		this.bft = bft;
	}

	protected override void Update()
	{
		if (bft == null)
		{
			FinishAction();
			return;
		}
		vfx = new RecycleWastePileVFX(gameState);
		BattleFieldTransform battleFieldTransform = bft;
		if (battleFieldTransform != null && battleFieldTransform.isPlayer)
		{
			vfx.PrepareVFX();
		}
		foreach (Item allItem in bft.wastePileItemContainer.GetAllItems())
		{
			EnqueueAction(new TransferItemAction(allItem, bft.drawPileItemContainer, null, TransferItemAction.TransferAnimType.None));
			BattleFieldTransform battleFieldTransform2 = bft;
			if (battleFieldTransform2 != null && battleFieldTransform2.isPlayer)
			{
				EnqueueAction(delegate
				{
					vfx.PlayTransferItemAnim();
				});
				EnqueueAction(new WaitAction(0.03f / base.actionTimeScale));
			}
		}
		BattleFieldTransform battleFieldTransform3 = bft;
		if (battleFieldTransform3 != null && battleFieldTransform3.isPlayer)
		{
			EnqueueAction(new WaitUntilAction(() => vfx.IsVFXEnd()));
			EnqueueAction(delegate
			{
				vfx.Finish();
			});
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerDrawCardRecycle(bft);
		});
		FinishAction();
	}
}
