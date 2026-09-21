using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLocalize : MonoBehaviour
{
	[Serializable]
	public class LocalFontSizeConfig
	{
		[Serializable]
		public class Config
		{
			[SerializeField]
			public LocalizationManager.LanguageCode code;

			[SerializeField]
			public float size;
		}

		[SerializeField]
		public List<Config> configs;
	}

	private static readonly Dictionary<string, Dictionary<LocalizationManager.LanguageCode, string>> MissingTermFallbacks = new Dictionary<string, Dictionary<LocalizationManager.LanguageCode, string>>();

	public string id;

	public string path;

	public string translationDescription;

	public bool ignore;

	public float initSize;

	public LocalFontSizeConfig localFontSizeConfig;

	private TextMeshProUGUI Text => GetComponent<TextMeshProUGUI>();

	private void Awake()
	{
		initSize = (Text.enableAutoSizing ? Text.fontSizeMax : Text.fontSize);
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Combine(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
		if (ScriptableObjectSingleton<LocalizationManager>.instance != null && ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage != LocalizationManager.LanguageCode.ChineseSimplified)
		{
			SetLanguageText(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
			SetLanguageSize(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
		}
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Remove(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
	}

	private void OnLanguageChanged(LocalizationManager.LanguageCode lang)
	{
		SetLanguageText(lang);
		SetLanguageSize(lang);
	}

	public void SetLanguageText(LocalizationManager.LanguageCode lang)
	{
		if (ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization && !ignore)
		{
			LocalizeTerm localizeTerm = LocalizationManager.GetLocalizeTerm(id);
			string text;
			if (localizeTerm != null)
			{
				Text.text = localizeTerm.GetText(lang);
			}
			else if (TryGetMissingTermFallback(id, lang, out text))
			{
				Text.text = text;
			}
			else
			{
				Debug.LogError("[TMPLocalize] - SetLanguageText() - localize not found - [" + id + "] - [" + base.transform.FullPath() + "]");
				Text.text = "----";
			}
			if (GetComponent<TextEmergeEffect>() != null)
			{
				GetComponent<TextEmergeEffect>().StartEmergeText();
			}
		}
	}

	private static bool TryGetMissingTermFallback(string key, LocalizationManager.LanguageCode lang, out string text)
	{
		text = "";
		if (!MissingTermFallbacks.TryGetValue(key, out var value))
		{
			return false;
		}
		if (value.TryGetValue(lang, out text))
		{
			return true;
		}
		if (value.TryGetValue(LocalizationManager.LanguageCode.English, out text))
		{
			return true;
		}
		return false;
	}

	public void SetLanguageSize(LocalizationManager.LanguageCode lang)
	{
		if (localFontSizeConfig == null || !ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization || ignore)
		{
			return;
		}
		LocalFontSizeConfig.Config config = localFontSizeConfig.configs.Find((LocalFontSizeConfig.Config c) => c.code == lang);
		if (config == null)
		{
			if (Text.enableAutoSizing)
			{
				Text.fontSizeMax = initSize;
			}
			else
			{
				Text.fontSize = initSize;
			}
		}
		else if (Text.enableAutoSizing)
		{
			Text.fontSizeMax = config.size;
		}
		else
		{
			Text.fontSize = config.size;
		}
	}
}
