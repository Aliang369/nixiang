using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeImage : MonoBehaviour
{
	[Serializable]
	public class Config
	{
		[SerializeField]
		public LocalizationManager.LanguageCode code;

		[SerializeField]
		public Sprite sprite;
	}

	[SerializeField]
	private List<Config> sprites;

	[SerializeField]
	private Sprite fallback;

	[SerializeField]
	private Image image;

	private void Awake()
	{
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Combine(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
		SetLanguageText(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
	}

	private void OnDestroy()
	{
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Remove(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
	}

	private void OnLanguageChanged(LocalizationManager.LanguageCode lang)
	{
		SetLanguageText(lang);
	}

	private void SetLanguageText(LocalizationManager.LanguageCode lang)
	{
		Sprite sprite = sprites.Find((Config c) => c.code == lang)?.sprite;
		if (sprite == null)
		{
			image.sprite = fallback;
		}
		else
		{
			image.sprite = sprite;
		}
	}
}
