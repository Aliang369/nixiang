public class ClearRoomAction : ExecuteAction
{
	protected override void Update()
	{
		gameState.currentEncounter?.ClearRoom();
	}
}
