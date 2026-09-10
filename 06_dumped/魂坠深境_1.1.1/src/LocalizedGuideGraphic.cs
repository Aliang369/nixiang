using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class LocalizedGuideGraphic : MonoBehaviour
{
	[SerializeField]
	private RawImage rawImage;

	[SerializeField]
	private Image image;

	[SerializeField]
	private Texture englishTexture;

	[SerializeField]
	private Sprite englishSprite;

	private Texture chineseTexture;

	private Sprite chineseSprite;

	private void Awake()
	{
		if (rawImage != null)
		{
			chineseTexture = rawImage.texture;
		}
		if (image != null)
		{
			chineseSprite = image.sprite;
		}
		Apply(ScriptableObjectSingleton<LocalizationManager>.instance.currentLanguage);
	}

	private void Apply(LocalizationManager.LanguageCode language)
	{
		bool flag = UsesChineseGuide(language);
		if (rawImage != null)
		{
			rawImage.texture = ((!flag && englishTexture != null) ? englishTexture : chineseTexture);
		}
		if (image != null)
		{
			image.sprite = ((!flag && englishSprite != null) ? englishSprite : chineseSprite);
		}
	}

	internal static bool UsesChineseGuide(LocalizationManager.LanguageCode language)
	{
		if (language != LocalizationManager.LanguageCode.Unset && language != LocalizationManager.LanguageCode.ChineseSimplified)
		{
			return language == LocalizationManager.LanguageCode.ChineseTraditional;
		}
		return true;
	}
}
