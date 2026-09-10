using System.Text;
using ChillyRoom.Services.Accounts.Sdk;
using Generated.ChillyRoomSdkClient;

public class DefaultWatermarkContentProvider : IWatermarkContentProvider
{
	public string GetText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		try
		{
			AccountStateManager instance = AccountStateManager.Instance;
			if (instance != null && instance.HasLoggedIn)
			{
				stringBuilder.Append($"PID{ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId}  ");
			}
			string text = GlobalConfigManager.instance?.CurrentDistroConfigInfo?.distroName;
			if (!string.IsNullOrEmpty(text))
			{
				stringBuilder.Append(text + "  ");
			}
			stringBuilder.Append(AppVersionDisplay.GetDisplayVersion());
		}
		catch
		{
		}
		return stringBuilder.ToString();
	}
}
