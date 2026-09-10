using Generated.ChillyRoomSdkClient;
using UnityEngine;

public static class AccountUtil
{
	public enum AccountCreateResult
	{
		Success,
		illegal,
		Exist
	}

	public enum LoginResult
	{
		Success,
		illegal,
		NotExist,
		WrongPassword
	}

	public static AccountCreateResult CreateAccount(string playerID, string nickName, string password)
	{
		if (Bootstrap.instance.globalSaveConfigData.accounts.Find((AccountData t) => t.playerID == playerID) != null)
		{
			return AccountCreateResult.Exist;
		}
		AccountData accountData = new AccountData();
		accountData.playerID = playerID;
		accountData.nickName = nickName;
		accountData.password = password;
		Bootstrap.instance.globalSaveConfigData.accounts.Add(accountData);
		SavingUtil.SaveGlobalSaveConfigData(Bootstrap.instance.globalSaveConfigData);
		return AccountCreateResult.Success;
	}

	public static void DeleteAccount(string accountID)
	{
		AccountData accountData = GetAccountData(accountID);
		if (accountData != null)
		{
			Bootstrap.instance.globalSaveConfigData.accounts.Remove(accountData);
			SavingUtil.DeleteProfileData(accountID);
			SavingUtil.SaveGlobalSaveConfigData(Bootstrap.instance.globalSaveConfigData);
		}
	}

	public static bool IsAccountExist(string playerID)
	{
		return GetAccountData(playerID) != null;
	}

	public static void Logout()
	{
		ChillyRoomService.GetSdk().Accounts.Logout(null, isNeedWarning: false);
	}

	public static AccountData GetAccountData(string playerID)
	{
		return Bootstrap.instance.globalSaveConfigData.accounts.Find((AccountData t) => t.playerID == playerID);
	}

	public static void SetupPredefinedProfiles()
	{
		foreach (GlobalConfigManager.PredefinedProfile predefinedProfile in GlobalConfigManager.instance.predefinedProfiles)
		{
			if (!IsAccountExist(predefinedProfile.accountID))
			{
				Debug.Log("创建预制账号-" + predefinedProfile.accountID);
				CreateAccount(predefinedProfile.accountID, predefinedProfile.nickName, predefinedProfile.password);
				if (predefinedProfile.profileData != null)
				{
					SavingUtil.WriteToPersistent(SavingUtil.GetAccountProfileSavePath(predefinedProfile.accountID), predefinedProfile.profileData.text);
				}
				else
				{
					SavingUtil.CreateProfileData(predefinedProfile.accountID);
				}
			}
		}
	}
}
