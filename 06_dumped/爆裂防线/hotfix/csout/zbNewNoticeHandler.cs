using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Types;

public static class zbNewNoticeHandler
{
	private const string KeyNewNotices = "newNotices";

	private static Dictionary<string, int> m_newNotices = new Dictionary<string, int>();

	public static void Init()
	{
		m_newNotices = LoadStringIntDict("newNotices");
	}

	public static bool IsNewNotice(string noticeId)
	{
		if (m_newNotices.TryGetValue(noticeId, out var value))
		{
			return value == 0;
		}
		return false;
	}

	public static void AddNewNotice(string noticeId)
	{
		if (!m_newNotices.ContainsKey(noticeId))
		{
			m_newNotices[noticeId] = 0;
			SaveStringIntDict("newNotices", m_newNotices);
		}
	}

	public static void SetNoticeViewed(string noticeId)
	{
		if (m_newNotices.TryGetValue(noticeId, out var value) && value == 0)
		{
			m_newNotices[noticeId] = 1;
			SaveStringIntDict("newNotices", m_newNotices);
		}
	}

	private static void SaveStringIntDict(string key, Dictionary<string, int> dict, string splitSymbol = ":")
	{
		string jsonData = string.Join(",", dict.Select((KeyValuePair<string, int> kv) => $"{kv.Key}{splitSymbol}{kv.Value}"));
		zbGameSave.Instance.SaveDataToFile(key, jsonData);
	}

	private static Dictionary<string, int> LoadStringIntDict(string key, string splitSymbol = ":")
	{
		string dataFromFile = zbGameSave.Instance.GetDataFromFile(key);
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		if (string.IsNullOrEmpty(dataFromFile))
		{
			return dictionary;
		}
		string[] array = dataFromFile.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(splitSymbol);
			if (array2.Length == 2 && int.TryParse(array2[1], out var result))
			{
				dictionary[array2[0]] = result;
			}
		}
		return dictionary;
	}

	public static bool HasNewPoint()
	{
		if (zbLoginUtil.noticeListAckData != null)
		{
			foreach (NoticeDetail notice in zbLoginUtil.noticeListAckData.NoticeList)
			{
				if (IsNewNotice(notice.Id))
				{
					return true;
				}
			}
		}
		return false;
	}
}
