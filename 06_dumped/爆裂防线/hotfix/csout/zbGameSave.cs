using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class zbGameSave
{
	[Serializable]
	private class IntArrayWrapper
	{
		public int[] array;
	}

	private static zbGameSave sInstance;

	public static zbGameSave Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbGameSave();
			}
			return sInstance;
		}
	}

	public void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
		PlayerPrefs.Save();
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public void SetBool(string key, bool value)
	{
		SetInt(key, value ? 1 : 0);
	}

	public bool GetBool(string key, bool defaultValue = false)
	{
		return GetInt(key, defaultValue ? 1 : 0) == 1;
	}

	public void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
		PlayerPrefs.Save();
	}

	public float GetFloat(string key, float defaultValue = 0f)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public void SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
		PlayerPrefs.Save();
	}

	public string GetString(string key, string defaultValue = "")
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public void SetLong(string key, long value)
	{
		PlayerPrefs.SetString(key, value.ToString());
		PlayerPrefs.Save();
	}

	public long GetLong(string key, long defaultValue = 0L)
	{
		if (PlayerPrefs.HasKey(key) && long.TryParse(PlayerPrefs.GetString(key), out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public void SetIntArray(string key, int[] array)
	{
		string value = JsonConvert.SerializeObject((object)array);
		SetString(key, value);
	}

	public int[] GetIntArray(string key)
	{
		string text = GetString(key);
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				return JsonConvert.DeserializeObject<int[]>(text);
			}
			catch (Exception)
			{
				return new int[0];
			}
		}
		return new int[0];
	}

	public void DelteKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public void SaveDataToFile(string fileName, string jsonData)
	{
		try
		{
			File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), jsonData);
		}
		catch (Exception)
		{
		}
	}

	public string GetDataFromFile(string fileName)
	{
		try
		{
			string path = Path.Combine(Application.persistentDataPath, fileName);
			if (!File.Exists(path))
			{
				return string.Empty;
			}
			return File.ReadAllText(path);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}
}
