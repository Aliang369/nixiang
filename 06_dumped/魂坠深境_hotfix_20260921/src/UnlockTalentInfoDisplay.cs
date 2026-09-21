using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockTalentInfoDisplay : CellUI<TalentDef>, IDisplayModulesController
{
	public class DisplayModule : UIDisplayModule
	{
		protected UnlockTalentInfoDisplay unlockTalentInfoDisplay => parent as UnlockTalentInfoDisplay;

		protected TalentDef talent => unlockTalentInfoDisplay.setupData;
	}

	[SerializeField]
	private TextMeshProUGUI talentNameText;

	[SerializeField]
	private Image talentIconImage;

	[SerializeField]
	private TextMeshProUGUI talentDescriptionText;

	[SerializeField]
	private TextMeshProUGUI talentTypeNameText;

	[SerializeField]
	private TextMeshProUGUI talentTypeDescriptionText;

	private CharacterTalentUnlockPanel characterTalentUnlockPanelParent;

	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private TalentDef talent => base.setupData;

	protected override void Init()
	{
		characterTalentUnlockPanelParent = GetComponentInParent<CharacterTalentUnlockPanel>(includeInactive: true);
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<TalentDef>.applicationState, talentDescriptionText, base.rectTransform);
	}

	protected override void OnSetup(TalentDef talentCell)
	{
		base.gameObject.SetActive(value: true);
		SetDirty();
	}

	protected override void OnUnset()
	{
		base.gameObject.SetActive(value: false);
		base.OnUnset();
	}

	protected override void UpdateDisplay()
	{
		if (!(talent == null))
		{
			PlayerCharacterState character = characterTalentUnlockPanelParent?.character;
			talentNameText.text = talent.GetDisplayName(CellUI<TalentDef>.applicationState, character);
			talentIconImage.sprite = talent.GetDisplayIcon(CellUI<TalentDef>.applicationState, character);
			talentDescriptionText.text = talent.GetDisplayDescription(CellUI<TalentDef>.applicationState, character, out var generator);
			textLinkObjectDisplayer.SetGenerator(generator);
			talentTypeNameText.text = talent.TalentTypeName;
			talentTypeDescriptionText.text = talent.TalentTypeDescription;
		}
	}
}
