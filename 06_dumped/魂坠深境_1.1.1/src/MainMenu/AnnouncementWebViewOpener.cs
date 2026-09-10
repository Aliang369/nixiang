using Generated.ChillyRoomSdkClient;

namespace MainMenu;

public static class AnnouncementWebViewOpener
{
	public static void Open(string url)
	{
		if (!url.IsNullOrEmpty())
		{
			ChillyRoomService.GetSdk().Core.WebViewClient.ShowOrCreateWebview(url);
		}
	}
}
