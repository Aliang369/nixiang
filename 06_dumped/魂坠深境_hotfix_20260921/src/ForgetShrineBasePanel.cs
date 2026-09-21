using System.Collections;
using System.Collections.Generic;
using Common.UI;
using TMPro;
using UnityEngine;

public class ForgetShrineBasePanel : CellUI<CharacterClassState>, IStackPanel, IShowAsync
{
	[SerializeField]
	public ForgetShrinePanel forgetShrinePanel;

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
		InitUI(forgetShrinePanel);
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.canUnselect = false;
		characterClassFilterPanel.onChangeSelected = delegate
		{
			ShowCharacterClass(selected?.state, syncFilter: false);
		};
		hintButton.onClick.AddListener(delegate
		{
			CellUI<CharacterClassState>.fortressState.uiManager.fortressGuidePanel.forgetShrineGuideHint.Show();
		});
		backButton.onClick.AddListener(((CellUI<CharacterClassState>)this).HideUI<CharacterClassState>);
	}

	protected override void OnSetup(CharacterClassState setupData)
	{
		RefreshCharacterClassFilter();
		ShowCharacterClass(setupData ?? GetDefaultCharacterClassState(), syncFilter: true);
		SetDirty();
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		base.gameObject.SetActive(value: true);
		yield break;
	}

	private void RefreshCharacterClassFilter()
	{
		List<IconObjectFilterPanel.IconFilterData> allCharacterClassIconFilterDatas = CellUI<CharacterClassState>.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: false);
		for (int i = 0; i < allCharacterClassIconFilterDatas.Count; i++)
		{
			IconObjectFilterPanel.IconFilterData value = allCharacterClassIconFilterDatas[i];
			CharacterClassDef characterClassDef = value.target as CharacterClassDef;
			value.shouldShowRedDot = () => CellUI<CharacterClassState>.applicationState.playerInfoManager.HasUnviewedForgetShrinePoint(characterClassDef?.state);
			allCharacterClassIconFilterDatas[i] = value;
		}
		characterClassFilterPanel.Sync(allCharacterClassIconFilterDatas);
		characterClassFilterPanel.gameObject.SetActive(value: true);
	}

	private void ShowCharacterClass(CharacterClassState character, bool syncFilter)
	{
		if (character != null)
		{
			titleText.text = CellUI<CharacterClassState>.textData.term_forget_shrine_title;
			if (syncFilter)
			{
				characterClassFilterPanel.selected = characterClassFilterPanel.GetFilter(character.classDef);
			}
			forgetShrinePanel.ShowUI(character);
			if (CellUI<CharacterClassState>.applicationState.playerInfoManager.MarkForgetShrinePointViewed(character))
			{
				characterClassFilterPanel.SetDirty();
				CellUI<CharacterClassState>.fortressState.uiManager.mainUIPanel.SetDirty();
			}
		}
	}

	private CharacterClassState GetDefaultCharacterClassState()
	{
		List<CharacterClassState> characterClassStates = CellUI<CharacterClassState>.applicationState.characterManager.characterClassStates;
		if (characterClassStates == null || characterClassStates.Count == 0)
		{
			return null;
		}
		return characterClassStates[0];
	}
}
