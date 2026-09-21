using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MasteryInfoPanel : CellUI<MasteryInfoDisplay.DisplayInfo>
{
	[SerializeField]
	private GameButton closeButton;

	[SerializeField]
	private GameButton installButton;

	[SerializeField]
	private GameButton uninstallButton;

	[SerializeField]
	private MasteryInfoDisplay masteryInfoDisplay;

	[SerializeField]
	private Image lockedImage;

	[SerializeField]
	private TextMeshProUGUI lockedInfoText;

	private CharacterInfoPanel characterInfoPanel;

	private TextMeshProUGUI installButtonText;

	private TextMeshProUGUI[] uninstallButtonTexts;

	public RectTransform InstallButtonRect
	{
		get
		{
			if (!(installButton != null))
			{
				return null;
			}
			return installButton.GetComponent<RectTransform>();
		}
	}

	private PlayerCharacterState character => base.setupData.character;

	private bool isPreview => character?.isPreview ?? false;

	private CharacterMastery mastery => base.setupData.mastery;

	protected override void Init()
	{
		InitUI(masteryInfoDisplay);
		characterInfoPanel = GetComponentInParent<CharacterInfoPanel>(includeInactive: true);
		installButtonText = installButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		uninstallButtonTexts = uninstallButton.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		RefreshActionButtonTexts();
		closeButton.onClick.AddListener(base.Unset);
		installButton.onClick.AddListener(delegate
		{
			character.SetMastery(mastery);
			CellUI<MasteryInfoDisplay.DisplayInfo>.applicationState.savingManager.SetProfileDirtyToSave();
			Unset();
		});
		uninstallButton.onClick.AddListener(delegate
		{
			character.SetMastery(null);
			CellUI<MasteryInfoDisplay.DisplayInfo>.applicationState.savingManager.SetProfileDirtyToSave();
			Unset();
		});
	}

	private void RefreshActionButtonTexts()
	{
		TextDataPreset instance = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (installButtonText != null)
		{
			installButtonText.text = instance.term_apply_mastery;
		}
		if (uninstallButtonTexts != null)
		{
			if (uninstallButtonTexts.Length != 0 && uninstallButtonTexts[0] != null)
			{
				uninstallButtonTexts[0].text = instance.term_unequip_mastery;
			}
			if (uninstallButtonTexts.Length > 1 && uninstallButtonTexts[1] != null)
			{
				uninstallButtonTexts[1].text = instance.term_mastery_applied;
			}
		}
	}

	protected override void OnSetup(MasteryInfoDisplay.DisplayInfo _)
	{
		base.gameObject.SetActive(value: true);
		masteryInfoDisplay.Setup(base.setupData);
		UpdateDisplay();
	}

	protected override void OnUnset()
	{
		masteryInfoDisplay.Unset();
		base.gameObject.SetActive(value: false);
		parent.SetDirty();
	}

	protected override void UpdateDisplay()
	{
		RefreshActionButtonTexts();
		lockedImage.gameObject.SetActive(value: false);
		installButton.gameObject.SetActive(value: false);
		uninstallButton.gameObject.SetActive(value: false);
		if (isPreview)
		{
			lockedImage.gameObject.SetActive(value: true);
			lockedInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_unlocked;
		}
		else if (!character.IsMasteryUnlocked(mastery))
		{
			lockedImage.gameObject.SetActive(value: true);
			int num = character.def.obsessionLevelDefs.FindIndex((CharacterObsessionLevelConfig config) => (config.effect is UnlockCharacterMastery unlockCharacterMastery && unlockCharacterMastery.mastery == mastery) || config.effect is UnlockAllMastery);
			if (num >= 0)
			{
				string text = character.def.obsessionLevelDefs.IndexOrNull(num)?.title ?? "";
				lockedInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_unlock_by_obsession_level_arg0_title_arg1.FormatWith(num + 1, text);
			}
			else
			{
				lockedInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_not_unlocked;
			}
		}
		else if (character.currentMastery != mastery)
		{
			installButton.gameObject.SetActive(value: true);
		}
		else
		{
			uninstallButton.gameObject.SetActive(value: true);
		}
	}
}
