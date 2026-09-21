using System.Collections;
using System.Collections.Generic;

public class EnterMapTransitAnimAction : AbstractEnumeratorAction
{
	private readonly bool scrollMap;

	public EnterMapTransitAnimAction(bool scrollMap)
	{
		this.scrollMap = scrollMap;
	}

	protected override IEnumerator Run()
	{
		gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.updateLocations = false;
		UIHelper.DisableAllButton();
		gameState.UIManager.dungeonMapPanel.ShowPanel();
		List<WrapperEnumerator> list = new List<WrapperEnumerator>();
		gameState.StartCoroutine(gameState.UIManager.showMapFogPanel.HideFog);
		gameState.UIManager.gameVirtualCamera.SetState(VirtualCamera.State.Map);
		if (scrollMap)
		{
			list.Add(gameState.StartCoroutine(() => gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.PlayTween_ScrollMap()));
		}
		else if (gameState.dungeonMapManager.currentLocation != null)
		{
			gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.SetViewRectToLocation(gameState.dungeonMapManager.currentLocation);
		}
		yield return WrapperEnumerator.WaitAll(list);
		gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.updateLocations = true;
		gameState.UIManager.dungeonMapPanel.dungeonMapDisplay.SetDirty();
		UIHelper.EnableAllButton();
	}
}
