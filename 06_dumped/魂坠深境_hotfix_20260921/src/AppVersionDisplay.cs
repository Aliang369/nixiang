using ContainerOfSoul.Launcher;
using UnityEngine;

public static class AppVersionDisplay
{
	public static string GetDisplayVersion(string hotfixVersion = null)
	{
		if (hotfixVersion == null)
		{
			hotfixVersion = LauncherModel.BundleVersion;
		}
		return Application.version + "." + NormalizeHotfixVersion(hotfixVersion);
	}

	private static string NormalizeHotfixVersion(string hotfixVersion)
	{
		if (!string.IsNullOrWhiteSpace(hotfixVersion) && !(hotfixVersion == "-1"))
		{
			return hotfixVersion;
		}
		return "0";
	}
}
