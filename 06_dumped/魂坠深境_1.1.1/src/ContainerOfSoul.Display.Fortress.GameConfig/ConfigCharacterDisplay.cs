using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ContainerOfSoul.Display.Fortress.GameConfig;

public class ConfigCharacterDisplay : UIBase, IPointerClickHandler, IEventSystemHandler
{
	public CharacterSpineDisplay characterSpineDisplay;

	public CharacterSpineDisplay characterSpineDisplay2;

	public TextMeshProUGUI characterNameText;

	public TextMeshProUGUI skinAndMasteryNameText;

	public GameButton openCharacterSelectionButton;

	public GameButton openTalentSelectionButton;

	public GameObject characterStrengthenInfo;

	[NonSerialized]
	private GameConfigPanel gameConfigPanel;

	private PlayerCharacterState CharacterState => gameConfigPanel.config.GetCharacterState(UIBase.applicationState);

	private PlayerCharacterState DisplayCharacterState => gameConfigPanel.config.GetDisplayCharacterState(UIBase.applicationState);

	protected override bool shouldUpdate => parent.gameObject.activeInHierarchy;

	protected override void Init()
	{
		InitUI(characterSpineDisplay);
		if (characterSpineDisplay2 != null)
		{
			InitUI(characterSpineDisplay2);
		}
		InitCharacterPortraitClickTarget(characterSpineDisplay);
		InitCharacterPortraitClickTarget(characterSpineDisplay2);
		gameConfigPanel = (GameConfigPanel)parent;
		openCharacterSelectionButton.gameObject.SetActive(value: false);
		openTalentSelectionButton.onClick.AddListener(_OnOpenTalentSelectionButtonClicked);
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.AddListener(_OnGameConfigUpdate);
		UIHelper.RegisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
		characterStrengthenInfo.gameObject.SetActive(value: false);
	}

	protected override void Dispose()
	{
		openTalentSelectionButton.onClick.RemoveListener(_OnOpenTalentSelectionButtonClicked);
		UIBase.applicationState.eventManager.SetNewGameConfigEvent.RemoveListener(_OnGameConfigUpdate);
		UIHelper.UnregisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		PlayerCharacterState displayCharacterState = DisplayCharacterState;
		if (displayCharacterState != null)
		{
			CharacterMastery mastery = gameConfigPanel.config.GetMastery(UIBase.applicationState);
			characterSpineDisplay.Setup(displayCharacterState);
			characterSpineDisplay2?.Setup(displayCharacterState);
			characterNameText.text = displayCharacterState.def.characterName;
			skinAndMasteryNameText.text = displayCharacterState.def.characterName + ((mastery != null) ? (" " + mastery.masteryName) : "");
		}
	}

	private void _OnGameConfigUpdate(NewGameConfigData config)
	{
		SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		characterStrengthenInfo.gameObject.SetActive(value: false);
	}

	private void _OnOpenTalentSelectionButtonClicked()
	{
		UIBase.fortressState.uiManager.characterInfoBasePanel.ShowUI(CharacterState);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}

	private static void InitCharacterPortraitClickTarget(CharacterSpineDisplay display)
	{
		if (!(display == null))
		{
			Image image = display.GetComponent<Image>();
			if (image == null)
			{
				image = display.gameObject.AddComponent<Image>();
				image.color = Color.clear;
			}
			image.raycastTarget = true;
		}
	}

	private static bool IsCharacterPortraitClicked(CharacterSpineDisplay display, PointerEventData eventData)
	{
		if (display == null)
		{
			return false;
		}
		return RectTransformUtility.RectangleContainsScreenPoint(display.transform as RectTransform, eventData.position, eventData.pressEventCamera);
	}
}
