using System;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInfoDisplay : CellUI<GameSkill>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected SkillInfoDisplay display => parent as SkillInfoDisplay;

		protected GameSkill gameSkill => display.setupData;

		protected ProfileSkill profileSkill => gameSkill.profileSkill;

		protected PresetSkillSlot presetSkillSlot => display.presetSkillSlot;
	}

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private TextMeshProUGUI basicInfoText;

	[SerializeField]
	private TextMeshProUGUI castDescriptionText;

	[SerializeField]
	private TextMeshProUGUI passiveDescriptionText;

	[SerializeField]
	private GameObject spellBackground;

	[SerializeField]
	private bool showStory;

	[SerializeField]
	private Image storyPanel;

	[SerializeField]
	private TextMeshProUGUI storyText;

	[NonSerialized]
	private TextLinkObjectDisplayer castTextLinkObjectDisplayer;

	[NonSerialized]
	private TextLinkObjectDisplayer passiveTextLinkObjectDisplayer;

	[NonSerialized]
	public PresetSkillSlot presetSkillSlot;

	protected override void Init()
	{
		castTextLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<GameSkill>.applicationState, castDescriptionText, base.rectTransform);
		passiveTextLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<GameSkill>.applicationState, passiveDescriptionText, base.rectTransform);
		RankModule module = GetModule<RankModule>();
		if (module != null)
		{
			module.fQuality = () => base.setupData.QualityLevel;
			module.fMaxVirutalQuality = () => base.setupData.skillDef.MaxQualityLevel;
		}
		base.gameObject.SetActive(value: false);
	}

	protected override void OnSetup(GameSkill skill)
	{
		SetActiveAndDirty(active: true);
		if (showStory && !skill.skillDef.storyText.IsNullOrEmpty())
		{
			storyPanel.gameObject.SetActive(value: true);
			storyText.text = skill.skillDef.storyText;
		}
		else
		{
			storyPanel.gameObject.SetActive(value: false);
		}
	}

	protected override void OnUnset()
	{
		presetSkillSlot = null;
		base.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		if (base.setupData == null)
		{
			icon.sprite = null;
			nameText.text = string.Empty;
			basicInfoText.text = string.Empty;
			castDescriptionText.text = string.Empty;
			passiveDescriptionText.text = string.Empty;
			castDescriptionText.gameObject.SetActive(value: false);
			passiveDescriptionText.gameObject.SetActive(value: false);
			spellBackground.gameObject.SetActive(value: false);
			if (storyPanel != null)
			{
				storyPanel.gameObject.SetActive(value: false);
			}
		}
		else
		{
			icon.sprite = base.setupData.skillDef.Icon;
			nameText.text = base.setupData.NameWithQualityAndUpgrade;
			basicInfoText.text = base.setupData.GetBasicInfoDescription();
			castDescriptionText.text = base.setupData.GetCastDescription(CellUI<GameSkill>.applicationState, out var generator);
			castTextLinkObjectDisplayer.SetGenerator(generator);
			passiveDescriptionText.text = base.setupData.GetPassiveDescription(CellUI<GameSkill>.applicationState, out var generator2);
			passiveTextLinkObjectDisplayer.SetGenerator(generator2);
			castDescriptionText.gameObject.SetActive(!castDescriptionText.text.IsNullOrEmpty());
			passiveDescriptionText.gameObject.SetActive(!passiveDescriptionText.text.IsNullOrEmpty());
			spellBackground.gameObject.SetActive(!castDescriptionText.text.IsNullOrEmpty());
		}
	}
}
