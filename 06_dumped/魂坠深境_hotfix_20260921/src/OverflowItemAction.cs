public class OverflowItemAction : ExecuteAction
{
	private readonly Item item;

	private readonly bool playDetailedDisplay;

	private readonly bool waitForNextFrame;

	public OverflowItemAction(Item item, bool playDetailedDisplay, bool waitForNextFrame)
	{
		this.item = item;
		this.playDetailedDisplay = playDetailedDisplay;
		this.waitForNextFrame = waitForNextFrame;
		Ignore(ActionLock.Type.RemoveItem);
	}

	protected override void Update()
	{
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerOverflow(item);
		});
		if (playDetailedDisplay)
		{
			EnqueueAction(delegate
			{
				gameState.StartCoroutine(() => gameState.UIManager.playerItemArea.playerOverloadDisplay.PlayIndexEffect(item));
			});
			EnqueueAction(new WaitAction(0.4f / base.actionTimeScale));
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ExileOverflowItemInsteadConsume))
		{
			EnqueueAction(new ExileItemAction(item, null));
		}
		else
		{
			EnqueueAction(new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Overflow));
		}
		if (waitForNextFrame)
		{
			EnqueueAction(new WaitFrameAction(1));
		}
	}
}
