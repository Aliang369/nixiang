public static class PlatformHelper
{
	public enum Type
	{
		None = -1,
		PC = 1,
		IOS = 2,
		Android = 3
	}

	public static Type GetPlatform(bool debug = true)
	{
		return Type.Android;
	}

	public static bool IsMobile(bool debug = true)
	{
		if (GetPlatform(debug) != Type.Android)
		{
			return GetPlatform(debug) == Type.IOS;
		}
		return true;
	}

	public static bool IsEditor()
	{
		return false;
	}
}
