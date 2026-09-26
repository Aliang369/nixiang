using Proto.Enums;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class zbFontReplacer
{
	private static zbFontReplacer sInstance;

	private ELanguageType lastLanguage;

	private Font targetFont;

	private FontAsset targetFontAsset;

	private FontDefinition targetFontDefinition;

	public static zbFontReplacer Instance
	{
		get
		{
			if (sInstance == null)
			{
				sInstance = new zbFontReplacer();
			}
			return sInstance;
		}
	}

	private string GetFontPathByLanguage(ELanguageType eLanguageType)
	{
		return "font/zbft.ttf";
	}

	public void Init()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		string fontPathByLanguage = GetFontPathByLanguage(zbLanguage.Instance.GetLanguageType());
		targetFont = zbAssetLoader.Instance.LoadResouce<Font>(fontPathByLanguage);
		if ((Object)(object)targetFont != (Object)null)
		{
			targetFontAsset = FontAsset.CreateFontAsset(targetFont);
			targetFontDefinition = FontDefinition.FromSDFFont(targetFontAsset);
		}
	}

	public void ReplaceFontInUI(VisualElement rootElement)
	{
		ELanguageType languageType = zbLanguage.Instance.GetLanguageType();
		if (lastLanguage != languageType)
		{
			lastLanguage = languageType;
			Init();
		}
		if (!((Object)(object)targetFont == (Object)null))
		{
			TraverseAndReplaceFont(rootElement);
		}
	}

	private void TraverseAndReplaceFont(VisualElement element, bool bNeedTransText = true)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Label val = (Label)(object)((element is Label) ? element : null);
		if (val != null)
		{
			((VisualElement)val).style.unityFontDefinition = StyleFontDefinition.op_Implicit(targetFontDefinition);
			if (bNeedTransText)
			{
				((TextElement)val).text = zbUtil.Language(((TextElement)val).text);
			}
		}
		else
		{
			Button val2 = (Button)(object)((element is Button) ? element : null);
			if (val2 != null)
			{
				((VisualElement)val2).style.unityFontDefinition = StyleFontDefinition.op_Implicit(targetFontDefinition);
				if (bNeedTransText)
				{
					((TextElement)val2).text = zbUtil.Language(((TextElement)val2).text);
				}
			}
			else
			{
				TextElement val3 = (TextElement)(object)((element is TextElement) ? element : null);
				if (val3 != null)
				{
					((VisualElement)val3).style.unityFontDefinition = StyleFontDefinition.op_Implicit(targetFontDefinition);
					if (bNeedTransText)
					{
						val3.text = zbUtil.Language(val3.text);
					}
				}
			}
		}
		foreach (VisualElement item in element.Children())
		{
			TraverseAndReplaceFont(item, bNeedTransText);
		}
	}
}
