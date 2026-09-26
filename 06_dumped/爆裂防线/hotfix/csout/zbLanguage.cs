using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Proto.Enums;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLanguage
{
	private static zbLanguage sInstance;

	private ELanguageType m_eLanguage = ELanguageType.Zhcn;

	private Dictionary<string, string> m_stringTable;

	private StringBuilder m_stringBuilderCache = new StringBuilder();

	public static zbLanguage Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbLanguage();
			}
			return sInstance;
		}
	}

	public void Init()
	{
		LoadLanguageDataConfig();
	}

	public void LoadLanguageDataConfig()
	{
		m_stringTable = new Dictionary<string, string>();
		string resName = "language/chinese/chinese.json";
		TextAsset val = zbAssetLoader.Instance.LoadResouce<TextAsset>(resName);
		if ((Object)(object)val == (Object)null)
		{
			val = zbAssetLoader.Instance.LoadResouce<TextAsset>("language/chinese/chinese.json");
		}
		if ((Object)(object)val != (Object)null)
		{
			Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(val.text);
			m_stringTable = new Dictionary<string, string>();
			foreach (KeyValuePair<string, string> item in dictionary)
			{
				string value = Regex.Unescape(item.Value);
				m_stringTable[item.Key] = value;
			}
		}
		else
		{
			m_stringTable = new Dictionary<string, string>();
		}
		zbLanguageConfig.Instance.Init();
	}

	public string GetStringTable(string key)
	{
		if (m_stringTable == null || !m_stringTable.TryGetValue(key, out var value))
		{
			return key;
		}
		int num = 10;
		while (num-- > 0)
		{
			int num2 = value.IndexOf('{');
			if (num2 == -1)
			{
				break;
			}
			m_stringBuilderCache.Clear();
			int num3 = 0;
			while (num2 != -1)
			{
				int num4 = value.IndexOf('}', num2 + 1);
				if (num4 == -1)
				{
					break;
				}
				m_stringBuilderCache.Append(value, num3, num2 - num3);
				string text = value.Substring(num2 + 1, num4 - num2 - 1);
				if (m_stringTable.TryGetValue(text, out var value2))
				{
					m_stringBuilderCache.Append(value2);
				}
				else
				{
					m_stringBuilderCache.Append('{').Append(text).Append('}');
				}
				num3 = num4 + 1;
				num2 = value.IndexOf('{', num3);
			}
			m_stringBuilderCache.Append(value, num3, value.Length - num3);
			string text2 = m_stringBuilderCache.ToString();
			if (text2 == value)
			{
				break;
			}
			value = text2;
		}
		return value;
	}

	public ELanguageType GetLanguageType()
	{
		return m_eLanguage;
	}

	public string GetRegionName()
	{
		return CultureInfo.CurrentCulture.Name;
	}

	public void SetButtonTextVisible(Button[] buttons)
	{
	}
}
