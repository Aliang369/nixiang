using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Game.MainItemInfoDisplayModule;

public static class ItemQualityNameEcho
{
	private const string EchoObjectName = "QualityNameEcho";

	private const string EchoMaterialResourcePath = "Material/ItemQualityNameEcho";

	private const float OutlineWidth = 0.035f;

	private const float OutlineAlpha = 0.34f;

	private const float MinimumEffectWidth = 260f;

	private const float MaximumEffectWidth = 360f;

	private const float EffectHeight = 58f;

	private const float HorizontalEffectInset = 32f;

	private static Material echoMaterial;

	public static void Setup(TextMeshProUGUI itemName, QualityLevel qualityLevel, Color qualityColor, bool showEffect)
	{
		if (itemName == null)
		{
			return;
		}
		itemName.fontMaterial.DisableKeyword(ShaderUtilities.Keyword_Glow);
		if (!showEffect || qualityLevel < QualityLevel.Base || qualityLevel > QualityLevel.Epic)
		{
			SetEchoActive(itemName, active: false);
			itemName.outlineWidth = 0f;
			return;
		}
		Material material = GetEchoMaterial();
		if (material == null)
		{
			SetEchoActive(itemName, active: false);
			itemName.outlineWidth = 0f;
			return;
		}
		itemName.ForceMeshUpdate();
		float num = Mathf.Clamp(itemName.GetRenderedValues(onlyVisibleCharacters: false).x * 1.45f + 150f, 260f, 360f);
		RectTransform rectTransform = itemName.rectTransform.parent as RectTransform;
		float x = ((rectTransform != null && rectTransform.rect.width > 0f) ? Mathf.Max(0f, rectTransform.rect.width - 32f) : num);
		Image image = EnsureEchoImage(itemName);
		if (!(image == null))
		{
			Color echoColor = GetEchoColor(qualityColor);
			image.material = material;
			image.color = new Color(echoColor.r, echoColor.g, echoColor.b, GetEchoAlpha(qualityLevel));
			image.gameObject.SetActive(value: true);
			RectTransform rectTransform2 = image.rectTransform;
			rectTransform2.sizeDelta = new Vector2(x, 58f);
			rectTransform2.anchoredPosition = itemName.rectTransform.anchoredPosition;
			PlaceImmediatelyBehind(rectTransform2, itemName.rectTransform);
			itemName.outlineColor = new Color32((byte)Mathf.RoundToInt(qualityColor.r * 255f), (byte)Mathf.RoundToInt(qualityColor.g * 255f), (byte)Mathf.RoundToInt(qualityColor.b * 255f), (byte)Mathf.RoundToInt(86.700005f));
			itemName.outlineWidth = 0.035f;
			itemName.UpdateMeshPadding();
			itemName.SetMaterialDirty();
		}
	}

	private static void PlaceImmediatelyBehind(RectTransform echoTransform, RectTransform itemNameTransform)
	{
		int siblingIndex = echoTransform.GetSiblingIndex();
		int siblingIndex2 = itemNameTransform.GetSiblingIndex();
		int siblingIndex3 = ((siblingIndex < siblingIndex2) ? (siblingIndex2 - 1) : siblingIndex2);
		echoTransform.SetSiblingIndex(siblingIndex3);
	}

	private static Material GetEchoMaterial()
	{
		if (echoMaterial == null)
		{
			echoMaterial = Resources.Load<Material>("Material/ItemQualityNameEcho");
		}
		return echoMaterial;
	}

	private static Image EnsureEchoImage(TextMeshProUGUI itemName)
	{
		RectTransform rectTransform = itemName.rectTransform.parent as RectTransform;
		if (rectTransform == null)
		{
			return null;
		}
		RectTransform rectTransform2 = rectTransform.Find("QualityNameEcho") as RectTransform;
		Image image;
		if (rectTransform2 == null)
		{
			GameObject obj = new GameObject("QualityNameEcho", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image))
			{
				layer = itemName.gameObject.layer
			};
			rectTransform2 = obj.GetComponent<RectTransform>();
			rectTransform2.SetParent(rectTransform, worldPositionStays: false);
			image = obj.GetComponent<Image>();
		}
		else
		{
			image = rectTransform2.GetComponent<Image>() ?? rectTransform2.gameObject.AddComponent<Image>();
		}
		for (int i = 0; i < rectTransform2.childCount; i++)
		{
			rectTransform2.GetChild(i).gameObject.SetActive(value: false);
		}
		image.sprite = null;
		image.type = Image.Type.Simple;
		image.preserveAspect = false;
		image.raycastTarget = false;
		rectTransform2.anchorMin = new Vector2(0.5f, 0.5f);
		rectTransform2.anchorMax = new Vector2(0.5f, 0.5f);
		rectTransform2.pivot = new Vector2(0.5f, 0.5f);
		rectTransform2.localRotation = Quaternion.identity;
		rectTransform2.localScale = Vector3.one;
		return image;
	}

	private static void SetEchoActive(TextMeshProUGUI itemName, bool active)
	{
		Transform transform = itemName.rectTransform.parent?.Find("QualityNameEcho");
		if (transform != null)
		{
			transform.gameObject.SetActive(active);
		}
	}

	private static Color GetEchoColor(Color qualityColor)
	{
		float num = qualityColor.r * 0.2126f + qualityColor.g * 0.7152f + qualityColor.b * 0.0722f;
		return Color.Lerp(b: new Color(num, num, num), a: qualityColor, t: 0.1f);
	}

	private static float GetEchoAlpha(QualityLevel qualityLevel)
	{
		return qualityLevel switch
		{
			QualityLevel.Base => 0.42f, 
			QualityLevel.Common => 0.5f, 
			QualityLevel.Uncommon => 0.56f, 
			QualityLevel.Rare => 0.62f, 
			QualityLevel.Epic => 0.68f, 
			_ => 0f, 
		};
	}
}
