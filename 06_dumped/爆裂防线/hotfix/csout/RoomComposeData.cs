public class RoomComposeData
{
	public int[] roomList;

	public int x;

	public int y;

	public RoomComposeData(int roomId0, int roomId1, int newX, int newY)
	{
		roomList = new int[2] { roomId0, roomId1 };
		x = newX;
		y = newY;
	}
}
