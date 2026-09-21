using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChillyRoom.Censor.V1;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForerunnerLeaveNotePanel : UIBase
{
	public TMP_InputField inputField;

	public GameButton presetSelectorButton;

	public TextMeshProUGUI presetSelectorText;

	public RectTransform presetSelectorArrow;

	public GameObject presetPopup;

	public ScrollRect presetScrollRect;

	public RectTransform presetOptionRoot;

	public GameButton presetOptionTemplate;

	public TextMeshProUGUI signatureText;

	public GameButton confirmButton;

	public GameButton backButton;

	public ItemDisplayBase itemDisplay;

	public SkillDisplay skillDisplay;

	public ItemInfoDisplayBase itemInfoDisplay;

	public SkillInfoDisplay skillInfoDisplay;

	public string checkingText;

	public string pendingText;

	public string validatedText;

	public TextMeshProUGUI hint;

	[NonSerialized]
	private ForerunnerLegacy forerunnerLegacy;

	[NonSerialized]
	private ForerunnerPanel parentPanel;

	[NonSerialized]
	private List<TextDataPreset.ForerunnerPresetMessage> presetMessages = new List<TextDataPreset.ForerunnerPresetMessage>();

	[NonSerialized]
	private readonly List<GameButton> presetOptionButtons = new List<GameButton>();

	[NonSerialized]
	private Color activeInputTextColor;

	[NonSerialized]
	private bool presetUiEnabled;

	[NonSerialized]
	private bool inputVisualActivated;

	[NonSerialized]
	private string selectedPresetPreviewText;

	private const int MaxNoteLength = 120;

	private const int VisiblePresetOptionCount = 5;

	private static readonly Color SelectedOptionColor = new Color(0.58f, 0.43f, 0.23f, 0.26f);

	private static readonly Color HighlightedOptionColor = new Color(0.58f, 0.43f, 0.23f, 0.16f);

	private static readonly Color PressedOptionColor = new Color(0.49f, 0.33f, 0.17f, 0.28f);

	protected override void Init()
	{
		InitUI(itemDisplay);
		InitUI(skillDisplay);
		InitUI(itemInfoDisplay);
		InitUI(skillInfoDisplay);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		skillDisplay.GetComponent<GameButton>().onClick.AddListener(delegate
		{
			skillInfoDisplay.gameObject.SetActive(value: true);
			skillInfoDisplay.Setup(skillDisplay.setupData);
		});
		InputModule module = itemDisplay.GetModule<InputModule>();
		module.onPointerClick = (Action<InputModuleBase>)Delegate.Combine(module.onPointerClick, (Action<InputModuleBase>)delegate
		{
			itemInfoDisplay.gameObject.SetActive(value: true);
			itemInfoDisplay.Setup(itemDisplay.item);
		});
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		inputField.characterLimit = 120;
		inputField.onValueChanged.AddListener(_OnInputValueChanged);
		inputField.onSelect.AddListener(_OnInputSelected);
		presetSelectorButton.onClick.AddListener(_OnPresetSelectorButtonClicked);
		presetOptionTemplate.gameObject.SetActive(value: false);
		presetPopup.SetActive(value: false);
		activeInputTextColor = inputField.textComponent.color;
		ConfigureSingleLineEllipsis(presetSelectorText);
		ConfigureSingleLineEllipsis(presetOptionTemplate.transform.Find("Label")?.GetComponent<TextMeshProUGUI>());
		parentPanel = GetUIInParents<ForerunnerPanel>();
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
		inputField.onValueChanged.RemoveListener(_OnInputValueChanged);
		inputField.onSelect.RemoveListener(_OnInputSelected);
		presetSelectorButton.onClick.RemoveListener(_OnPresetSelectorButtonClicked);
	}

	protected override void UpdateDisplay()
	{
		if (forerunnerLegacy != null)
		{
			skillDisplay.gameObject.SetActive(forerunnerLegacy.legacySkill != null);
			if (forerunnerLegacy.legacySkill != null)
			{
				skillDisplay.Setup(forerunnerLegacy.legacySkill.DisplaySkill);
			}
			itemDisplay.gameObject.SetActive(forerunnerLegacy.legacyItem != null);
			if (forerunnerLegacy.legacyItem != null)
			{
				itemDisplay.Setup(forerunnerLegacy.legacyItem.DisplayItem);
			}
			signatureText.text = forerunnerLegacy.signature;
		}
	}

	private async Task CenserCheck()
	{
		if (checkingText != null || pendingText == null)
		{
			return;
		}
		checkingText = pendingText;
		validatedText = null;
		pendingText = null;
		hint.text = "......";
		try
		{
			switch (await ValidationUtil.Validate(checkingText, TextUsageHint.Chat))
			{
			case TextValidationResult.Pass:
				hint.text = "";
				validatedText = checkingText;
				break;
			case TextValidationResult.ContainsSensitivePhrase:
				hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_input_text_not_allow;
				validatedText = null;
				break;
			default:
				hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkError;
				validatedText = null;
				break;
			}
		}
		catch
		{
			hint.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_network_error_general_networkError;
			validatedText = null;
		}
		checkingText = null;
		SetDirty();
	}

	public void Setup(ForerunnerLegacy t)
	{
		forerunnerLegacy = t;
		checkingText = null;
		validatedText = null;
		selectedPresetPreviewText = null;
		hint.text = "";
		presetUiEnabled = ForerunnerPresetMessageResolver.GetEnabledMessages().Count > 0;
		presetSelectorButton.gameObject.SetActive(presetUiEnabled);
		SetPresetPopupVisible(visible: false);
		if (!presetUiEnabled)
		{
			pendingText = null;
			inputVisualActivated = true;
			RefreshInputTextColor();
			SetDirty();
			return;
		}
		RefreshPresetOptions();
		if (forerunnerLegacy.note.IsNullOrEmpty() && forerunnerLegacy.presetMessageId.IsNullOrEmpty())
		{
			ApplyPreset(ForerunnerPresetMessageResolver.PickRandomEnabled(), activateInput: false);
		}
		else
		{
			SetInputTextWithoutNotify(forerunnerLegacy.note ?? string.Empty);
			TextDataPreset.ForerunnerPresetMessage message = null;
			bool flag = !forerunnerLegacy.presetMessageId.IsNullOrEmpty() && ForerunnerPresetMessageResolver.TryGet(forerunnerLegacy.presetMessageId, out message);
			selectedPresetPreviewText = (flag ? message.text : null);
			inputVisualActivated = !flag;
			RefreshPresetVisuals();
		}
		SetDirty();
	}

	private void RefreshPresetOptions()
	{
		presetMessages = ForerunnerPresetMessageResolver.GetEnabledMessages();
		foreach (GameButton presetOptionButton in presetOptionButtons)
		{
			if (!(presetOptionButton == null))
			{
				presetOptionButton.gameObject.SetActive(value: false);
				UnityEngine.Object.Destroy(presetOptionButton.gameObject);
			}
		}
		presetOptionButtons.Clear();
		foreach (TextDataPreset.ForerunnerPresetMessage presetMessage in presetMessages)
		{
			GameButton gameButton = UnityEngine.Object.Instantiate(presetOptionTemplate, presetOptionRoot);
			gameButton.gameObject.name = "PresetOption_" + presetMessage.id;
			TextMeshProUGUI textMeshProUGUI = gameButton.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
			if (textMeshProUGUI != null)
			{
				ConfigureSingleLineEllipsis(textMeshProUGUI);
				textMeshProUGUI.text = presetMessage.text;
			}
			TextDataPreset.ForerunnerPresetMessage capturedMessage = presetMessage;
			gameButton.onClick.AddListener(delegate
			{
				_OnPresetSelected(capturedMessage);
			});
			gameButton.gameObject.SetActive(value: true);
			presetOptionButtons.Add(gameButton);
		}
		presetSelectorButton.interactable = presetMessages.Count > 0;
		LayoutRebuilder.ForceRebuildLayoutImmediate(presetOptionRoot);
		RefreshPresetVisuals();
	}

	private void ApplyPreset(TextDataPreset.ForerunnerPresetMessage presetMessage, bool activateInput)
	{
		if (presetMessage != null)
		{
			forerunnerLegacy.presetMessageId = presetMessage.id;
			selectedPresetPreviewText = presetMessage.text;
			SetInputTextWithoutNotify(presetMessage.text);
			inputVisualActivated = activateInput;
			checkingText = null;
			validatedText = null;
			hint.text = "";
			RefreshPresetVisuals();
			SetDirty();
		}
	}

	private void SetInputTextWithoutNotify(string text)
	{
		string text2 = text ?? string.Empty;
		inputField.SetTextWithoutNotify(text2);
		pendingText = text2;
		forerunnerLegacy.note = text2;
	}

	private void _OnInputValueChanged(string text)
	{
		pendingText = text;
		validatedText = null;
		inputVisualActivated = true;
		if (forerunnerLegacy != null && presetUiEnabled)
		{
			forerunnerLegacy.note = text;
			forerunnerLegacy.presetMessageId = null;
		}
		SetPresetPopupVisible(visible: false);
		RefreshPresetVisuals();
		SetDirty();
	}

	private void _OnInputSelected(string _)
	{
		inputVisualActivated = true;
		SetPresetPopupVisible(visible: false);
		RefreshInputTextColor();
	}

	private void _OnPresetSelectorButtonClicked()
	{
		if (presetUiEnabled && presetMessages.Count != 0)
		{
			SetPresetPopupVisible(!presetPopup.activeSelf);
		}
	}

	private void _OnPresetSelected(TextDataPreset.ForerunnerPresetMessage presetMessage)
	{
		if (presetUiEnabled && presetMessage != null)
		{
			ApplyPreset(presetMessage, activateInput: true);
			SetPresetPopupVisible(visible: false);
		}
	}

	private void SetPresetPopupVisible(bool visible)
	{
		bool flag = visible && presetUiEnabled;
		presetPopup.SetActive(flag);
		signatureText.enabled = !flag;
		if (flag)
		{
			presetPopup.transform.SetAsLastSibling();
			Canvas.ForceUpdateCanvases();
			LayoutRebuilder.ForceRebuildLayoutImmediate(presetOptionRoot);
			ScrollToSelectedPreset();
		}
	}

	private void ScrollToSelectedPreset()
	{
		int num = presetMessages.FindIndex((TextDataPreset.ForerunnerPresetMessage message) => message.id == forerunnerLegacy.presetMessageId);
		if (num < 0 || presetMessages.Count <= 5)
		{
			presetScrollRect.verticalNormalizedPosition = 1f;
			return;
		}
		int num2 = presetMessages.Count - 5;
		int num3 = Mathf.Clamp(num - 2, 0, num2);
		presetScrollRect.verticalNormalizedPosition = 1f - (float)num3 / (float)num2;
	}

	private void RefreshPresetVisuals()
	{
		RefreshInputTextColor();
		if (!presetUiEnabled || forerunnerLegacy == null)
		{
			return;
		}
		presetSelectorText.text = selectedPresetPreviewText ?? string.Empty;
		for (int i = 0; i < presetOptionButtons.Count; i++)
		{
			GameButton gameButton = presetOptionButtons[i];
			if (!(gameButton == null))
			{
				bool flag = i < presetMessages.Count && presetMessages[i].id == forerunnerLegacy.presetMessageId;
				Transform transform = gameButton.transform.Find("Checkmark");
				if (transform != null)
				{
					transform.gameObject.SetActive(flag);
				}
				ColorBlock colors = gameButton.colors;
				colors.normalColor = (flag ? SelectedOptionColor : Color.clear);
				colors.highlightedColor = HighlightedOptionColor;
				colors.pressedColor = PressedOptionColor;
				colors.selectedColor = HighlightedOptionColor;
				colors.disabledColor = Color.clear;
				colors.fadeDuration = 0.08f;
				gameButton.colors = colors;
				if (gameButton.targetGraphic != null)
				{
					gameButton.targetGraphic.color = colors.normalColor;
				}
				TextMeshProUGUI textMeshProUGUI = gameButton.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
				if (textMeshProUGUI != null)
				{
					textMeshProUGUI.color = activeInputTextColor;
				}
			}
		}
	}

	private void RefreshInputTextColor()
	{
		if (!(inputField?.textComponent == null))
		{
			Color color = activeInputTextColor;
			if (presetUiEnabled && !inputVisualActivated)
			{
				color.a = 0.42f;
			}
			inputField.textComponent.color = color;
		}
	}

	private static void ConfigureSingleLineEllipsis(TMP_Text text)
	{
		if (!(text == null))
		{
			text.enableWordWrapping = false;
			text.overflowMode = TextOverflowModes.Ellipsis;
		}
	}

	private async void _OnConfirmButtonClicked()
	{
		SetPresetPopupVisible(visible: false);
		if (GlobalConfigManager.instance.EnableBlockWordsCheck)
		{
			await CenserCheck();
		}
		else
		{
			validatedText = pendingText;
		}
		if (!validatedText.IsNullOrEmpty())
		{
			confirmButton.interactable = false;
			forerunnerLegacy.signature = signatureText.text ?? "";
			forerunnerLegacy.note = validatedText ?? "";
			if (!forerunnerLegacy.presetMessageId.IsNullOrEmpty() && !ForerunnerPresetMessageResolver.IsEnabledForUpload(forerunnerLegacy.presetMessageId))
			{
				forerunnerLegacy.presetMessageId = null;
			}
			parentPanel.forerunnerHintPanel.TriggerAfterGive();
		}
	}

	private void _OnBackButtonClicked()
	{
		SetPresetPopupVisible(visible: false);
		base.gameObject.SetActive(value: false);
	}

	private void _OnEmptyAreaClicked()
	{
		SetPresetPopupVisible(visible: false);
		itemInfoDisplay.SetActiveAndDirty(active: false);
		skillInfoDisplay.SetActiveAndDirty(active: false);
	}
}
