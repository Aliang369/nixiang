using UnityEngine;

public class OverlayWorldEventAction : ExecuteAction
{
	private WorldEvent worldEventTemplate;

	private bool withTransitAnim;

	public OverlayWorldEventAction(WorldEvent worldEventTemplate, bool withTransitAnim = true)
	{
		this.worldEventTemplate = worldEventTemplate;
		this.withTransitAnim = withTransitAnim;
	}

	protected override void Update()
	{
		if (withTransitAnim)
		{
			EnqueueAction(new ExitRoomTransitAnimAction());
		}
		WorldEvent worldEvent = Object.Instantiate(worldEventTemplate);
		gameState.currentEncounter.triggerEvent = worldEvent;
		worldEvent.SetupAndShow(gameState);
		if (withTransitAnim)
		{
			EnqueueAction(new EnterRoomTransitAnimAction());
		}
	}
}
