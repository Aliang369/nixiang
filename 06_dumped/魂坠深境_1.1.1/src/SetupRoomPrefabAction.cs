public class SetupRoomPrefabAction : ExecuteAction
{
	private readonly DungeonRoom roomPrefab;

	public SetupRoomPrefabAction(DungeonRoom roomPrefab)
	{
		this.roomPrefab = roomPrefab;
	}

	protected override void Update()
	{
		if (roomPrefab != null)
		{
			gameState.currentEncounter.SetupRoom(roomPrefab);
		}
	}
}
