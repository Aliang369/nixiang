using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class PotentialTalentCell : CellUI<TalentDef>
{
	public TextMeshProUGUI nameText;

	public Image icon;

	public Image characterIcon;

	public TextMeshProUGUI descriptionText;

	public GameButton confirmButton;

	[NonSerialized]
	private ActivatePotentialTalentPanel activatePotentialTalentPanelParent;

	[NonSerialized]
	private TalentDef talent;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private ActivatePotential ActivatePotentialNode => talent.node as ActivatePotential;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<TalentDef>.applicationState, descriptionText, base.rectTransform);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		activatePotentialTalentPanelParent = GetUIInParents<ActivatePotentialTalentPanel>();
	}

	protected override void OnSetup(TalentDef dailyContract)
	{
		if (dailyContract != null && dailyContract.node is ActivatePotential)
		{
			talent = dailyContract;
			SetDirty();
		}
		characterIcon.gameObject.SetActive(value: false);
	}

	protected override void UpdateDisplay()
	{
		if (!(talent == null))
		{
			nameText.text = talent.GetDisplayName(CellUI<TalentDef>.applicationState, CellUI<TalentDef>.gameState.playerState.character);
			icon.sprite = talent.GetDisplayIcon(CellUI<TalentDef>.applicationState, CellUI<TalentDef>.gameState.playerState.character);
			if (CellUI<TalentDef>.gameState != null)
			{
				descriptionText.text = talent.GetDisplayDescription(CellUI<TalentDef>.applicationState, CellUI<TalentDef>.gameState.playerState.character, out var generator);
				textLinkObjectDisplayer.SetGenerator(generator);
			}
			else
			{
				descriptionText.text = TextDataPreset.FormatTalentDefEffectDescription(talent, CellUI<TalentDef>.applicationState, out var generator2);
				textLinkObjectDisplayer.SetGenerator(generator2);
			}
			confirmButton.interactable = ActivatePotentialNode.TriggerAble(talent, CellUI<TalentDef>.gameState);
			PlayerCharacterState sourceShareCharacter = GetSourceShareCharacter();
			if (sourceShareCharacter != null)
			{
				characterIcon.gameObject.SetActive(value: true);
				characterIcon.sprite = sourceShareCharacter.def.headIcon;
			}
			else
			{
				characterIcon.gameObject.SetActive(value: false);
			}
		}
	}

	private PlayerCharacterState GetSourceShareCharacter()
	{
		return CellUI<TalentDef>.gameState.applicationState.characterManager.characterStates.FirstOrDefault((PlayerCharacterState c) => c.def.sharePotential == talent && c.def != CellUI<TalentDef>.gameState.playerState.character.def);
	}

	private void _OnConfirmButtonClicked()
	{
		activatePotentialTalentPanelParent.ConfirmActivateTalent(talent);
	}
}
