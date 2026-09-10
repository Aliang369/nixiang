using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class IdolBasePanel : CellUI<CharacterClassState>, IStackPanel
{
	[SerializeField]
	public IdolPanel idolPanel;

	[SerializeField]
	private TextMeshProUGUI titleText;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton hintButton;

	private CharacterClassDef selected => characterClassFilterPanel?.selected?.setupData.target as CharacterClassDef;

	protected override void Init()
	{
		InitUI(idolPanel);
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.canUnselect = false;
		characterClassFilterPanel.onChangeSelected = delegate
		{
			if (selected != null)
			{
				ShowCharacterClass(selected.state, syncFilter: false);
			}
		};
		hintButton.onClick.AddListener(delegate
		{
			CellUI<CharacterClassState>.fortressState.uiManager.fortressGuidePanel.idolGuideHint.Show();
		});
		backButton.onClick.AddListener(((CellUI<CharacterClassState>)this).HideUI<CharacterClassState>);
	}

	protected override void OnSetup(CharacterClassState setupData)
	{
		base.gameObject.SetActive(value: true);
		RefreshCharacterClassFilter();
		CharacterClassState characterClassState = (IsAvailableIdolCharacterClass(setupData) ? setupData : GetDefaultCharacterClassState());
		if (characterClassState != null)
		{
			ShowCharacterClass(characterClassState, syncFilter: true);
		}
		SetDirty();
	}

	private void RefreshCharacterClassFilter()
	{
		List<IconObjectFilterPanel.IconFilterData> characterClassDefs = (from d in CellUI<CharacterClassState>.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false)
			where IsAvailableIdolCharacterClass(d.target as CharacterClassDef)
			select d).ToList();
		characterClassFilterPanel.Sync(characterClassDefs);
		characterClassFilterPanel.gameObject.SetActive(value: true);
	}

	private void ShowCharacterClass(CharacterClassState character, bool syncFilter)
	{
		titleText.text = CellUI<CharacterClassState>.textData.term_idol_title_format.FormatWith(character.classDef.characterName);
		if (syncFilter)
		{
			characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(character.classDef);
		}
		idolPanel.ShowUI(character);
	}

	private CharacterClassState GetDefaultCharacterClassState()
	{
		List<CharacterClassState> characterClassStates = CellUI<CharacterClassState>.applicationState.characterManager.characterClassStates;
		if (characterClassStates == null || characterClassStates.Count == 0)
		{
			return null;
		}
		return characterClassStates.FirstOrDefault(IsAvailableIdolCharacterClass);
	}

	private bool IsAvailableIdolCharacterClass(CharacterClassState characterClassState)
	{
		return IsAvailableIdolCharacterClass(characterClassState?.classDef);
	}

	private bool IsAvailableIdolCharacterClass(CharacterClassDef characterClassDef)
	{
		IdolDef idolDef = characterClassDef?.state.GetIdolDef();
		if (idolDef?.levels != null)
		{
			return idolDef.levels.Any((IdolLevel level) => level?.reward != null && level.reward.Count > 0);
		}
		return false;
	}
}
