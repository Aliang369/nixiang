using System.Collections.Generic;
using UnityEngine;

public class LocalizeTerm
{
	[SerializeField]
	public string key;

	[SerializeField]
	public string description;

	[SerializeField]
	public string category;

	[SerializeField]
	public bool dirty;

	[SerializeField]
	public Dictionary<LocalizationManager.LanguageCode, string> texts = new Dictionary<LocalizationManager.LanguageCode, string>();

	public string BaseText
	{
		get
		{
			return GetText(LocalizationManager.LanguageCode.ChineseSimplified);
		}
		set
		{
			SetText(LocalizationManager.LanguageCode.ChineseSimplified, value);
		}
	}

	public string GetText(LocalizationManager.LanguageCode languageCode)
	{
		if (texts == null)
		{
			texts = new Dictionary<LocalizationManager.LanguageCode, string>();
		}
		return texts.GetValueOrDefault(languageCode, "");
	}

	public void SetText(LocalizationManager.LanguageCode languageCode, string text)
	{
		if (texts == null)
		{
			texts = new Dictionary<LocalizationManager.LanguageCode, string>();
		}
		texts[languageCode] = text;
	}

	public bool EqualsTo(LocalizeTerm other)
	{
		if (key == other.key && description == other.description && category == other.category)
		{
			return texts[LocalizationManager.LanguageCode.ChineseSimplified] == other.texts[LocalizationManager.LanguageCode.ChineseSimplified];
		}
		return false;
	}
}
