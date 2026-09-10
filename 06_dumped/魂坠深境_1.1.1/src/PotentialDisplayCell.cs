using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PotentialDisplayCell : CellUI<TalentDef>, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private TextMeshProUGUI nameText;

	[SerializeField]
	private Image icon;

	[SerializeField]
	private Image lockMark;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	private TalentDef talent => base.setupData;

	private PlayerCharacterState characterState => (parent as CharacterInfoArea)?.characterState;

	private bool isPreview => (parent as CharacterInfoArea)?.isPreview ?? false;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<TalentDef>.applicationState, descriptionText, base.rectTransform);
	}

	protected override void OnSetup(TalentDef setupData)
	{
		UpdateDisplay();
	}

	protected override void UpdateDisplay()
	{
		if (!(talent == null) && characterState != null)
		{
			lockMark.gameObject.SetActive(!isPreview && !characterState.IsTalentUnlocked(talent));
			nameText.text = talent.talentDisplayName;
			icon.sprite = talent.icon;
			descriptionText.text = TextDataPreset.FormatTalentDefEffectDescription(talent, CellUI<TalentDef>.applicationState, out var generator);
			textLinkObjectDisplayer.SetGenerator(generator);
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (characterState.IsTalentUnlocked(talent))
		{
			CellUI<TalentDef>.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_potential_is_unlock_need_active_on_healfire);
		}
		else
		{
			CellUI<TalentDef>.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_potential_need_unlock_on_talent_tree);
		}
	}
}
