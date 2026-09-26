public class zbDungeonDefine
{
	private static zbDungeonDefine sInstance;

	public const int zombiesOrder = 6;

	public const int heroBaseOrder = 5;

	public const float ROOM_WIDTH_GAP = 0.2f;

	public const float ROOM_WIDTH = 3f;

	public const float FLOOR_HEIGHT = 2.2f;

	public const int FLOOR_MAX_CELL = 25;

	public static zbDungeonDefine Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbDungeonDefine();
			}
			return sInstance;
		}
	}
}
