using System;
using CoreKit.Config;
using Generated.ChillyRoomSdkClient;
using Sirenix.OdinInspector;

public class DebugIntergrateTool : SerializedScriptableObject
{
	[Serializable]
	public class SdkDebugMenu
	{
		public async void Login(UniversalSdkType type)
		{
			await ChillyRoomService.GetSdk().Accounts.Login();
		}

		public async void Logout()
		{
			ChillyRoomService.GetSdk().Accounts.Logout();
		}
	}

	public SdkDebugMenu sdk = new SdkDebugMenu();
}
