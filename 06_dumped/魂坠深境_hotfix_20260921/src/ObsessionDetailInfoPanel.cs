using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ObsessionDetailInfoPanel : CellUI<CharacterObsessionLevelConfig>
{
	[SerializeField]
	private TextMeshProUGUI obsessionTitle;

	[SerializeField]
	private TextMeshProUGUI obsessionEffectText;

	[SerializeField]
	private TextMeshProUGUI obsessionStoryText;

	[SerializeField]
	private TextMeshProUGUI unlockSoulRewardText;

	[SerializeField]
	private Image obsessionLevelIcon;

	[SerializeField]
	private TextMeshProUGUI obsessionLevelText;

	[SerializeField]
	private GameObject unFinishLevel_Pass;

	[SerializeField]
	private HintRedDot hintRedDot;

	[SerializeField]
	private GameObject unFinishLevel_block;

	[SerializeField]
	private GameObject finishedLevel;

	[SerializeField]
	private GameObject storyTextPart;

	[SerializeField]
	private GameButton upgradeButton;

	[SerializeField]
	private FeatureDisplay featureDisplay;

	[NonSerialized]
	private CharacterObsessionPanel characterObsessionPanel;

	[NonSerialized]
	private CellUIPrefabPool<FeatureDisplay, FeatureDisplay.DisplayInfo> pool;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private CharacterObsessionLevelConfig levelConfig => base.setupData;

	private PlayerCharacterState character => characterObsessionPanel?.character;

	private int ObsessionLevelIndex => character.def.obsessionLevelDefs.IndexOf(levelConfig);

	protected override void Init()
	{
		characterObsessionPanel = GetUIInParents<CharacterObsessionPanel>();
		upgradeButton.onClick.AddListener(delegate
		{
			characterObsessionPanel.upgradeConfigPanel.Setup(characterObsessionPanel.character);
		});
		GetPrefabPool(ref pool, featureDisplay);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<CharacterObsessionLevelConfig>.applicationState, obsessionEffectText);
	}

	protected override void OnSetup(CharacterObsessionLevelConfig setupData)
	{
		base.gameObject.SetActive(value: true);
		obsessionLevelIcon.sprite = setupData.icon;
		obsessionLevelText.text = (ObsessionLevelIndex + 1).ToString();
		obsessionTitle.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_obsession_level_arg0_title_arg1.FormatWith(ObsessionLevelIndex + 1, levelConfig.title);
		obsessionEffectText.text = levelConfig.GetEffectDescription(character, out var generator);
		if (unlockSoulRewardText != null)
		{
			unlockSoulRewardText.text = CellUI<CharacterObsessionLevelConfig>.textData.term_unlock_reward_arg0_count_arg1.FormatWith(TextDisplayHelper.SoulText(character.def.attachClass.mClass), 1);
		}
		textLinkObjectDisplayer.SetGenerator(generator);
		pool.Sync(levelConfig.effect?.GenerateFeatureInfoData(character)?.ConvertAll(FeatureDisplay.GetDisplayInfo));
		foreach (FeatureDisplay activeElement in pool.GetActiveElements())
		{
			activeElement.clickShowInfoOverride = FeatureDisplayClickOverrideForMastery;
		}
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
	}

	private bool FeatureDisplayClickOverrideForMastery(FeatureDisplay.DisplayInfo info)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (info.target is CharacterMastery mastery)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_general);
			CellUI<CharacterObsessionLevelConfig>.fortressState.uiManager.characterInfoBasePanel.characterInfoPanel.SelectMastery(mastery);
			return true;
		}
		return false;
	}

	protected override void UpdateDisplay()
	{
		unFinishLevel_Pass.gameObject.SetActive(value: false);
		upgradeButton.gameObject.SetActive(value: false);
		hintRedDot.gameObject.SetActive(value: false);
		finishedLevel.gameObject.SetActive(value: false);
		storyTextPart.gameObject.SetActive(value: false);
		unFinishLevel_block.gameObject.SetActive(value: false);
		if (character.IsUnlocked(levelConfig))
		{
			finishedLevel.gameObject.SetActive(value: true);
			storyTextPart.gameObject.SetActive(value: true);
			obsessionStoryText.text = levelConfig.GetStoryText();
			return;
		}
		unFinishLevel_Pass.gameObject.SetActive(value: true);
		if (character.CanUnlock(levelConfig))
		{
			upgradeButton.gameObject.SetActive(value: true);
			if (levelConfig.GetAttachedCharacter(CellUI<CharacterObsessionLevelConfig>.applicationState).CheckCanUpgrade())
			{
				hintRedDot.gameObject.SetActive(value: true);
			}
		}
		else
		{
			unFinishLevel_block.gameObject.SetActive(value: true);
		}
	}
}
