using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class zbAccountMgr
{
	public static Dictionary<string, string> accounts = new Dictionary<string, string>();

	public static List<string> accountOrder = new List<string>();

	public static string account_zb;

	public static string userid_zb;

	public static string pwd_zb;

	public static string source_zb = "";

	public static string device_zb = "";

	public static string os_zb = "Win32";

	public static string os_vers_zb = "";

	private static string accountSaveName = "accountData.json";

	private static string orderSaveName = "accountOrder.json";

	private static string lastLoginAccountKey = "lastLoginAccountkey";

	public static void Init()
	{
		LoadAccounts();
		if (accounts.Count > 0)
		{
			account_zb = accountOrder[0];
		}
		os_zb = (Application.isMobilePlatform ? "mobile" : "win");
	}

	public static void SaveAccount(string username, string password)
	{
		accounts[username] = password;
		accountOrder.Remove(username);
		accountOrder.Insert(0, username);
		while (accountOrder.Count > 3)
		{
			string key = accountOrder[accountOrder.Count - 1];
			accountOrder.RemoveAt(accountOrder.Count - 1);
			accounts.Remove(key);
		}
		string jsonData = JsonConvert.SerializeObject((object)accounts);
		string jsonData2 = JsonConvert.SerializeObject((object)accountOrder.ToList());
		zbGameSave.Instance.SaveDataToFile(accountSaveName, jsonData);
		zbGameSave.Instance.SaveDataToFile(orderSaveName, jsonData2);
	}

	private static void LoadAccounts()
	{
		accounts.Clear();
		accountOrder.Clear();
		string dataFromFile = zbGameSave.Instance.GetDataFromFile(accountSaveName);
		if (!string.IsNullOrEmpty(dataFromFile))
		{
			accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataFromFile) ?? new Dictionary<string, string>();
		}
		string dataFromFile2 = zbGameSave.Instance.GetDataFromFile(orderSaveName);
		if (string.IsNullOrEmpty(dataFromFile2))
		{
			return;
		}
		List<string> list = JsonConvert.DeserializeObject<List<string>>(dataFromFile2);
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			if (accounts.ContainsKey(item))
			{
				accountOrder.Add(item);
			}
		}
	}

	public static void SaveLastLoginAccount()
	{
		zbGameSave.Instance.SetString(lastLoginAccountKey, account_zb);
	}

	public static string GetLastLoginAccount()
	{
		return zbGameSave.Instance.GetString(lastLoginAccountKey);
	}

	public static string GetPassword(string username)
	{
		if (!accounts.TryGetValue(username, out var value))
		{
			return "";
		}
		return value;
	}

	public static List<string> GetRecentUsernames()
	{
		return accountOrder.ToList();
	}
}
