public class EnterLocationAction : ExecuteAction
{
	private readonly PathLocation targetLocation;

	private readonly bool finishLoadingBeforeRoomReveal;

	public EnterLocationAction(PathLocation targetLocation, bool finishLoadingBeforeRoomReveal = false)
	{
		this.targetLocation = targetLocation;
		this.finishLoadingBeforeRoomReveal = finishLoadingBeforeRoomReveal;
	}

	protected override void Update()
	{
		if (gameState.dungeonMapManager.inMapMode)
		{
			EnqueueAction(new ExitMapTransitAnimAction());
		}
		else
		{
			EnqueueAction(new EnumeratorAction(gameState.UIManager.showMapFogPanel.ShowFog));
		}
		EnqueueAction(new EnterLocationImplAction(targetLocation));
		if (finishLoadingBeforeRoomReveal)
		{
			EnqueueAction(delegate
			{
				BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
			});
			EnqueueAction(new WaitUntilAction(() => !BootstrapExtra.instance.loadingPanel.showing));
		}
		EnqueueAction(new EnterRoomTransitAnimAction());
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAfterEnterLocationScreenFade(targetLocation);
		}));
	}
}
