using System.Collections;
using System.Collections.Generic;

public class ExitRoomTransitAnimAction : AbstractEnumeratorAction
{
	protected override IEnumerator Run()
	{
		Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_transition);
		UIHelper.DisableAllButton();
		List<WrapperEnumerator> anims = new List<WrapperEnumerator>
		{
			gameState.StartCoroutine(gameState.UIManager.showMapFogPanel.ShowFog),
			gameState.StartCoroutine(gameState.mainStateManager.HideCurrentRoomPanel)
		};
		yield return WrapperEnumerator.WaitAll(anims);
		UIHelper.EnableAllButton();
		anims.Clear();
	}
}
