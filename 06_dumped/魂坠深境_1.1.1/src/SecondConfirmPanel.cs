using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecondConfirmPanel : UIBase
{
	private const int MaxPenaltyDisplayCount = 5;

	private const string WillPowerWarningFontName = "NotoSerifSC-SemiBold";

	private const string WillPowerWarningMaterialName = "NotoSerifSC-SemiBold Atlas Material_Subtitle";

	private const string WillPowerButtonFontName = "NotoSerifSC-Bold";

	private const string WillPowerButtonMaterialName = "NotoSerifSC-Bold Atlas Material-buttonB";

	private static readonly Vector2 InfluenceDisplaySize = new Vector2(555.2f, 264f);

	private static readonly Vector2 WillPowerButtonIconSize = new Vector2(50f, 50f);

	private static readonly Color WarningTextColor = new Color(0.7f, 0.69f, 0.64f, 1f);

	[SerializeField]
	private TextMeshProUGUI refreshDescriptionText;

	[SerializeField]
	private TextMeshProUGUI confirmButtonText;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private TextMeshProUGUI cancelButtonText;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private Sprite willPowerCancelIconSprite;

	[SerializeField]
	private Sprite willPowerConfirmIconSprite;

	[NonSerialized]
	private SecondConfirmInfo confirmInfo;

	[NonSerialized]
	private WillPowerOverLimitConfirmInfo willPowerConfirmInfo;

	[NonSerialized]
	private RectTransform contentRoot;

	[NonSerialized]
	private GameObject normalDescriptionRoot;

	[NonSerialized]
	private RectTransform willPowerLayoutRoot;

	[NonSerialized]
	private RectTransform penaltyDisplayParent;

	[NonSerialized]
	private HorizontalLayoutGroup penaltyDisplayLayout;

	[NonSerialized]
	private TextMeshProUGUI warningText;

	[NonSerialized]
	private CellUIPrefabPool<InfluenceDisplay, Influence> penaltyDisplayPool;

	[NonSerialized]
	private Image confirmButtonImage;

	[NonSerialized]
	private Image cancelButtonImage;

	[NonSerialized]
	private Image willPowerConfirmButtonIcon;

	[NonSerialized]
	private Image willPowerCancelButtonIcon;

	[NonSerialized]
	private Sprite defaultConfirmButtonSprite;

	[NonSerialized]
	private Sprite defaultCancelButtonSprite;

	[NonSerialized]
	private Color defaultConfirmButtonColor;

	[NonSerialized]
	private Color defaultCancelButtonColor;

	[NonSerialized]
	private Vector2 defaultConfirmButtonTextPosition;

	[NonSerialized]
	private Vector2 defaultCancelButtonTextPosition;

	[NonSerialized]
	private TMP_FontAsset defaultConfirmButtonFont;

	[NonSerialized]
	private TMP_FontAsset defaultCancelButtonFont;

	[NonSerialized]
	private Material defaultConfirmButtonMaterial;

	[NonSerialized]
	private Material defaultCancelButtonMaterial;

	[NonSerialized]
	private TMP_FontAsset willPowerWarningFont;

	[NonSerialized]
	private TMP_FontAsset willPowerButtonFont;

	[NonSerialized]
	private Material willPowerWarningMaterial;

	[NonSerialized]
	private Material willPowerButtonMaterial;

	protected override void Init()
	{
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		contentRoot = refreshDescriptionText.transform.parent?.parent as RectTransform;
		normalDescriptionRoot = refreshDescriptionText.transform.parent?.gameObject;
		CacheButtonStyle();
	}

	public void Setup(SecondConfirmInfo confirmInfo)
	{
		this.confirmInfo = confirmInfo;
		willPowerConfirmInfo = null;
		base.gameObject.SetActive(value: true);
		SetWillPowerLayoutActive(active: false);
		refreshDescriptionText.text = confirmInfo.description;
		confirmButtonText.text = confirmInfo.confirmChoiceText;
		cancelButtonText.text = confirmInfo.cancelChoiceText;
		confirmButton.interactable = confirmInfo.confirmInteractable;
	}

	public void SetupWillPowerOverLimit(WillPowerOverLimitConfirmInfo info)
	{
		confirmInfo = null;
		willPowerConfirmInfo = info;
		base.gameObject.SetActive(value: true);
		EnsureWillPowerLayout();
		SetWillPowerLayoutActive(active: true);
		warningText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_will_power_over_limit_confirm_arg0.FormatWith(info.overLimitWillPower);
		cancelButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_cancel;
		confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_will_power_over_limit_confirm;
		confirmButton.interactable = true;
		SpawnPenaltyDisplays(info.penaltyInfluences);
	}

	private void CacheButtonStyle()
	{
		confirmButtonImage = confirmButton.targetGraphic as Image;
		cancelButtonImage = cancelButton.targetGraphic as Image;
		if (confirmButtonImage != null)
		{
			defaultConfirmButtonSprite = confirmButtonImage.sprite;
			defaultConfirmButtonColor = confirmButtonImage.color;
		}
		if (cancelButtonImage != null)
		{
			defaultCancelButtonSprite = cancelButtonImage.sprite;
			defaultCancelButtonColor = cancelButtonImage.color;
		}
		if (confirmButtonText.transform is RectTransform rectTransform)
		{
			defaultConfirmButtonTextPosition = rectTransform.anchoredPosition;
		}
		if (cancelButtonText.transform is RectTransform rectTransform2)
		{
			defaultCancelButtonTextPosition = rectTransform2.anchoredPosition;
		}
		defaultConfirmButtonFont = confirmButtonText.font;
		defaultCancelButtonFont = cancelButtonText.font;
		defaultConfirmButtonMaterial = confirmButtonText.fontSharedMaterial;
		defaultCancelButtonMaterial = cancelButtonText.fontSharedMaterial;
	}

	private void EnsureWillPowerLayout()
	{
		if (!(willPowerLayoutRoot != null))
		{
			if (contentRoot == null)
			{
				contentRoot = refreshDescriptionText.transform.parent?.parent as RectTransform;
			}
			willPowerLayoutRoot = CreateRect("WillPowerOverLimitLayout", contentRoot);
			willPowerLayoutRoot.anchorMin = Vector2.zero;
			willPowerLayoutRoot.anchorMax = Vector2.one;
			willPowerLayoutRoot.offsetMin = Vector2.zero;
			willPowerLayoutRoot.offsetMax = Vector2.zero;
			penaltyDisplayParent = CreateRect("PenaltyDisplayParent", willPowerLayoutRoot);
			penaltyDisplayParent.anchorMin = new Vector2(0.5f, 0.5f);
			penaltyDisplayParent.anchorMax = new Vector2(0.5f, 0.5f);
			penaltyDisplayParent.pivot = new Vector2(0.5f, 0.5f);
			penaltyDisplayParent.anchoredPosition = new Vector2(0f, 92f);
			penaltyDisplayParent.sizeDelta = new Vector2(1720f, 330f);
			penaltyDisplayLayout = penaltyDisplayParent.gameObject.AddComponent<HorizontalLayoutGroup>();
			penaltyDisplayLayout.childAlignment = (TextAnchor)4;
			penaltyDisplayLayout.spacing = 24f;
			penaltyDisplayLayout.childControlWidth = false;
			penaltyDisplayLayout.childControlHeight = false;
			penaltyDisplayLayout.childScaleWidth = true;
			penaltyDisplayLayout.childScaleHeight = true;
			penaltyDisplayLayout.childForceExpandWidth = false;
			penaltyDisplayLayout.childForceExpandHeight = false;
			warningText = CreateText("WarningText", willPowerLayoutRoot, refreshDescriptionText);
			RectTransform obj = warningText.rectTransform;
			obj.anchorMin = new Vector2(0.5f, 0.5f);
			obj.anchorMax = new Vector2(0.5f, 0.5f);
			obj.pivot = new Vector2(0.5f, 0.5f);
			obj.anchoredPosition = new Vector2(0f, -54f);
			obj.sizeDelta = new Vector2(1180f, 70f);
			warningText.enableAutoSizing = false;
			warningText.fontSize = 32f;
			warningText.alignment = TextAlignmentOptions.Center;
			warningText.color = WarningTextColor;
			ApplyWillPowerTextStyle(warningText, GetWillPowerWarningFont(), GetWillPowerWarningMaterial());
			InfluenceDisplay influenceInfoDisplayPrefab = UIBase.gameState.UIManager.multiInfoDisplayPanel.InfluenceInfoDisplayPrefab;
			penaltyDisplayPool = GetPrefabPool<InfluenceDisplay, Influence>(influenceInfoDisplayPrefab, penaltyDisplayParent);
			willPowerLayoutRoot.gameObject.SetActive(value: false);
		}
	}

	private void SetWillPowerLayoutActive(bool active)
	{
		if (willPowerLayoutRoot != null)
		{
			willPowerLayoutRoot.gameObject.SetActive(active);
		}
		if (normalDescriptionRoot != null)
		{
			normalDescriptionRoot.SetActive(!active);
		}
		if (active)
		{
			SetupWillPowerButtonStyle();
			return;
		}
		ClearPenaltyDisplays();
		RestoreButtonStyle();
	}

	private void SetupWillPowerButtonStyle()
	{
		if (confirmButtonImage != null && cancelButtonImage != null)
		{
			confirmButtonImage.color = defaultCancelButtonColor;
			cancelButtonImage.color = defaultConfirmButtonColor;
		}
		if (confirmButtonText.transform is RectTransform rectTransform)
		{
			rectTransform.anchoredPosition = defaultConfirmButtonTextPosition;
		}
		if (cancelButtonText.transform is RectTransform rectTransform2)
		{
			rectTransform2.anchoredPosition = defaultCancelButtonTextPosition;
		}
		EnsureWillPowerButtonIcons();
		SetWillPowerButtonIconsActive(active: true);
		ApplyWillPowerTextStyle(confirmButtonText, GetWillPowerButtonFont(), GetWillPowerButtonMaterial());
		ApplyWillPowerTextStyle(cancelButtonText, GetWillPowerButtonFont(), GetWillPowerButtonMaterial());
	}

	private void RestoreButtonStyle()
	{
		if (confirmButtonImage != null)
		{
			confirmButtonImage.sprite = defaultConfirmButtonSprite;
			confirmButtonImage.color = defaultConfirmButtonColor;
		}
		if (cancelButtonImage != null)
		{
			cancelButtonImage.sprite = defaultCancelButtonSprite;
			cancelButtonImage.color = defaultCancelButtonColor;
		}
		if (confirmButtonText.transform is RectTransform rectTransform)
		{
			rectTransform.anchoredPosition = defaultConfirmButtonTextPosition;
		}
		if (cancelButtonText.transform is RectTransform rectTransform2)
		{
			rectTransform2.anchoredPosition = defaultCancelButtonTextPosition;
		}
		RestoreTextStyle(confirmButtonText, defaultConfirmButtonFont, defaultConfirmButtonMaterial);
		RestoreTextStyle(cancelButtonText, defaultCancelButtonFont, defaultCancelButtonMaterial);
		SetWillPowerButtonIconsActive(active: false);
	}

	private void EnsureWillPowerButtonIcons()
	{
		if (willPowerConfirmButtonIcon == null)
		{
			willPowerConfirmButtonIcon = CreateWillPowerButtonIcon(confirmButton.transform, "WillPowerConfirmIcon", willPowerConfirmIconSprite, new Vector2(-165f, 8.5f));
		}
		if (willPowerCancelButtonIcon == null)
		{
			willPowerCancelButtonIcon = CreateWillPowerButtonIcon(cancelButton.transform, "WillPowerCancelIcon", willPowerCancelIconSprite, new Vector2(165f, 8.5f));
		}
	}

	private Image CreateWillPowerButtonIcon(Transform parent, string objectName, Sprite sprite, Vector2 anchoredPosition)
	{
		GameObject obj = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		RectTransform component = obj.GetComponent<RectTransform>();
		component.SetParent(parent, worldPositionStays: false);
		component.anchorMin = new Vector2(0.5f, 0.5f);
		component.anchorMax = new Vector2(0.5f, 0.5f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.anchoredPosition = anchoredPosition;
		component.sizeDelta = WillPowerButtonIconSize;
		Image component2 = obj.GetComponent<Image>();
		component2.sprite = sprite;
		component2.preserveAspect = true;
		component2.raycastTarget = false;
		component2.color = new Color(1f, 1f, 1f, 0.95f);
		component2.transform.SetAsLastSibling();
		return component2;
	}

	private void SetWillPowerButtonIconsActive(bool active)
	{
		if (willPowerConfirmButtonIcon != null)
		{
			willPowerConfirmButtonIcon.gameObject.SetActive(active);
		}
		if (willPowerCancelButtonIcon != null)
		{
			willPowerCancelButtonIcon.gameObject.SetActive(active);
		}
	}

	private void SpawnPenaltyDisplays(List<Influence> penaltyInfluences)
	{
		ClearPenaltyDisplays();
		if (penaltyDisplayPool == null || penaltyInfluences.IsNullOrEmpty())
		{
			return;
		}
		List<Influence> list = penaltyInfluences.Take(5).ToList();
		float penaltyDisplayScale = GetPenaltyDisplayScale(list.Count);
		penaltyDisplayLayout.spacing = ((list.Count <= 1) ? 0f : 24f);
		foreach (Influence item in list)
		{
			InfluenceDisplay influenceDisplay = penaltyDisplayPool.Spawn(item);
			if (!(influenceDisplay == null))
			{
				influenceDisplay.transform.localScale = Vector3.one * penaltyDisplayScale;
				if (influenceDisplay.transform is RectTransform rectTransform)
				{
					rectTransform.sizeDelta = InfluenceDisplaySize;
				}
				LayoutElement obj = influenceDisplay.gameObject.GetComponent<LayoutElement>() ?? influenceDisplay.gameObject.AddComponent<LayoutElement>();
				obj.preferredWidth = InfluenceDisplaySize.x * penaltyDisplayScale;
				obj.preferredHeight = InfluenceDisplaySize.y * penaltyDisplayScale;
				obj.flexibleWidth = 0f;
				obj.flexibleHeight = 0f;
			}
		}
	}

	private float GetPenaltyDisplayScale(int count)
	{
		if (count > 1)
		{
			return count switch
			{
				2 => 0.95f, 
				3 => 0.8f, 
				4 => 0.68f, 
				_ => 0.6f, 
			};
		}
		return 1f;
	}

	private TMP_FontAsset GetWillPowerWarningFont()
	{
		return willPowerWarningFont ?? (willPowerWarningFont = GetFontAsset("NotoSerifSC-SemiBold"));
	}

	private TMP_FontAsset GetWillPowerButtonFont()
	{
		return willPowerButtonFont ?? (willPowerButtonFont = GetFontAsset("NotoSerifSC-Bold"));
	}

	private Material GetWillPowerWarningMaterial()
	{
		return willPowerWarningMaterial ?? (willPowerWarningMaterial = GetLoadedMaterial("NotoSerifSC-SemiBold Atlas Material_Subtitle"));
	}

	private Material GetWillPowerButtonMaterial()
	{
		return willPowerButtonMaterial ?? (willPowerButtonMaterial = GetLoadedMaterial("NotoSerifSC-Bold Atlas Material-buttonB"));
	}

	private TMP_FontAsset GetFontAsset(string fontName)
	{
		TMP_FontAsset tMP_FontAsset = ScriptableObjectSingleton<GameDataManager>.instance?.allFontAssets?.FirstOrDefault((TMP_FontAsset font) => font != null && font.name == fontName);
		if (tMP_FontAsset != null)
		{
			return tMP_FontAsset;
		}
		return Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault((TMP_FontAsset font) => font != null && font.name == fontName);
	}

	private Material GetLoadedMaterial(string materialName)
	{
		return Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault((Material material) => material != null && material.name == materialName);
	}

	private void ApplyWillPowerTextStyle(TextMeshProUGUI text, TMP_FontAsset font, Material material)
	{
		if (!(text == null) && !(font == null))
		{
			text.font = font;
			text.fontSharedMaterial = ((material != null) ? material : font.material);
		}
	}

	private void RestoreTextStyle(TextMeshProUGUI text, TMP_FontAsset font, Material material)
	{
		if (!(text == null))
		{
			if (font != null)
			{
				text.font = font;
			}
			if (material != null)
			{
				text.fontSharedMaterial = material;
			}
		}
	}

	private void ClearPenaltyDisplays()
	{
		penaltyDisplayPool?.DespawnAll();
	}

	private RectTransform CreateRect(string name, Transform parent)
	{
		RectTransform component = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
		component.SetParent(parent, worldPositionStays: false);
		component.localScale = Vector3.one;
		return component;
	}

	private TextMeshProUGUI CreateText(string name, Transform parent, TextMeshProUGUI template)
	{
		TextMeshProUGUI component = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
		component.transform.SetParent(parent, worldPositionStays: false);
		component.transform.localScale = Vector3.one;
		component.font = template.font;
		component.fontSharedMaterial = template.fontSharedMaterial;
		component.spriteAsset = template.spriteAsset;
		component.color = template.color;
		component.fontSize = template.fontSize;
		component.fontSizeMin = template.fontSizeMin;
		component.fontSizeMax = template.fontSizeMax;
		component.enableAutoSizing = template.enableAutoSizing;
		component.fontStyle = template.fontStyle;
		component.alignment = template.alignment;
		component.enableWordWrapping = template.enableWordWrapping;
		component.overflowMode = template.overflowMode;
		component.richText = true;
		component.raycastTarget = false;
		return component;
	}

	private void OnDisable()
	{
		ClearPenaltyDisplays();
		if (willPowerLayoutRoot != null)
		{
			willPowerLayoutRoot.gameObject.SetActive(value: false);
		}
		if (normalDescriptionRoot != null)
		{
			normalDescriptionRoot.SetActive(value: true);
		}
		RestoreButtonStyle();
		willPowerConfirmInfo = null;
	}

	private void _OnConfirmButtonClicked()
	{
		if (willPowerConfirmInfo != null)
		{
			willPowerConfirmInfo.confirmCallback?.Invoke();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			confirmInfo.confirmCallback?.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}

	private void _OnCancelButtonClicked()
	{
		if (willPowerConfirmInfo != null)
		{
			willPowerConfirmInfo.cancelCallback?.Invoke();
			base.gameObject.SetActive(value: false);
		}
		else
		{
			confirmInfo.cancelCallback?.Invoke();
			base.gameObject.SetActive(value: false);
		}
	}
}
