using System.Collections;

public class NotifyBannerHintAction : GameAction
{
	private readonly BannerHintDisplay.SetupInfo info;

	private readonly bool wait;

	private bool started;

	public NotifyBannerHintAction(BannerHintDisplay.SetupInfo info, bool wait = true)
	{
		this.info = info;
		this.wait = wait;
	}

	protected override void Update()
	{
		if (started)
		{
			return;
		}
		started = true;
		if (wait)
		{
			gameState.StartCoroutine(WaitAnim);
			return;
		}
		gameState.StartCoroutine(() => gameState.UIManager.bannerHintDisplay.ShowBannerHint(info, base.actionTimeScale));
		FinishAction();
	}

	private IEnumerator WaitAnim()
	{
		yield return gameState.UIManager.bannerHintDisplay.ShowBannerHint(info, base.actionTimeScale);
		FinishAction();
	}
}
