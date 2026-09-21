using System.Collections;

public class ExitMapTransitAnimAction : AbstractEnumeratorAction
{
	private bool instantTransit;

	public ExitMapTransitAnimAction(bool instantTransit = false)
	{
		this.instantTransit = instantTransit;
	}

	protected override IEnumerator Run()
	{
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_transition);
		UIHelper.DisableAllButton();
		if (!instantTransit)
		{
			yield return gameState.UIManager.showMapFogPanel.ShowFog();
		}
		gameState.UIManager.dungeonMapPanel.HidePanel();
		UIHelper.EnableAllButton();
	}
}
