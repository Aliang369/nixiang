using System.Collections;
using System.Collections.Generic;

public class EnterRoomTransitAnimAction : AbstractEnumeratorAction
{
	protected override IEnumerator Run()
	{
		List<WrapperEnumerator> anims = new List<WrapperEnumerator>();
		UIHelper.DisableAllButton();
		anims.Add(gameState.StartCoroutine(gameState.UIManager.showMapFogPanel.HideFog));
		anims.Add(gameState.StartCoroutine(gameState.mainStateManager.ShowCurrentRoomPanel));
		yield return WrapperEnumerator.WaitAll(anims);
		gameState.gameEventManager.TriggerEnterRoom();
		UIHelper.EnableAllButton();
		anims.Clear();
	}
}
