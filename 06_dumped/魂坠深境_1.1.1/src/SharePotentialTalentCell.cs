using System;
using TMPro;
using UnityEngine.UI;

public class SharePotentialTalentCell : CellUI<PlayerCharacterState>
{
	public Image avatarImage;

	public Image talentImage;

	public TextMeshProUGUI talentNameText;

	public TextMeshProUGUI talentDescriptionText;

	public GameButton confirmButton;

	public TextMeshProUGUI confirmButtonText;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private PlayerCharacterState character;

	[NonSerialized]
	private ActivatePotentialTalentPanel activatePotentialTalentPanelParent;

	private TalentDef SharePotential => character.def.sharePotential;

	private bool EnableSharePotential => character.EnableSharePotential;

	private bool ActivateAble
	{
		get
		{
			if (!character.locked && SharePotential != null)
			{
				return EnableSharePotential;
			}
			return false;
		}
	}

	protected override void Init()
	{
		activatePotentialTalentPanelParent = GetUIInParents<ActivatePotentialTalentPanel>();
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<PlayerCharacterState>.applicationState, talentDescriptionText, base.rectTransform);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
	}

	protected override void OnSetup(PlayerCharacterState t)
	{
		character = t;
		avatarImage.sprite = t.Avatar;
		if (SharePotential != null)
		{
			talentImage.sprite = SharePotential.GetDisplayIcon(CellUI<PlayerCharacterState>.applicationState, t);
			talentNameText.text = SharePotential.GetDisplayName(CellUI<PlayerCharacterState>.applicationState, t);
			if (CellUI<PlayerCharacterState>.gameState != null)
			{
				talentDescriptionText.text = SharePotential.GetDisplayDescription(CellUI<PlayerCharacterState>.applicationState, CellUI<PlayerCharacterState>.gameState.playerState.character, out var generator);
				textLinkObjectDisplayer.SetGenerator(generator);
			}
		}
		confirmButton.interactable = ActivateAble;
		if (character.locked)
		{
			confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_share_potential_character_locked;
		}
		else if (!character.EnableSharePotential)
		{
			confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_share_potential_talent_required;
		}
		else
		{
			confirmButtonText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_active;
		}
	}

	private void _OnConfirmButtonClicked()
	{
		activatePotentialTalentPanelParent.ConfirmActivateTalent(SharePotential);
	}
}
