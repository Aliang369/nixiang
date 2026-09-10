using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkillSelectArea : UIBase, ISelectablePanel
{
	[SerializeField]
	private CellUIPrefabPoolConfig<SkillDisplay, GameSkill> skillPool;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private ScrollRectGridLayoutGroup scrollRectGridLayoutGroup;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[NonSerialized]
	public SkillDisplay selectedSkillDisplay;

	[NonSerialized]
	public ProfileSkill selectedSkill;

	private SkillConfigPresetPanel skillConfigPresetPanel => parent as SkillConfigPresetPanel;

	private PresetSkillSlot focusSlot => skillConfigPresetPanel.selectedCell?.skillSlot;

	private PlayerCharacterState character => skillConfigPresetPanel.character;

	protected override void Init()
	{
		Init(skillPool);
		InitUI(characterClassFilterPanel);
		sortDropDown.options = (from c in ProfileSkill.GetAllProfileSkillSortMethod()
			select new TMP_Dropdown.OptionData(c.Item1)).ToList();
		sortDropDown.onValueChanged.AddListener(delegate
		{
			RefreshSkills();
		});
		characterClassFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
		characterClassFilterPanel.gameObject.SetActive(value: true);
		IconObjectFilterPanel iconObjectFilterPanel = characterClassFilterPanel;
		iconObjectFilterPanel.onChangeSelected = (Action)Delegate.Combine(iconObjectFilterPanel.onChangeSelected, new Action(RefreshSkills));
	}

	public void Setup()
	{
		scrollRectGridLayoutGroup.ClearAllDisplayElement();
		skillPool.DespawnAll();
		RefreshSkills();
	}

	public bool CanConfig(ProfileSkill skill)
	{
		if (focusSlot == null)
		{
			return false;
		}
		return character.CanConfigSkill(focusSlot, skill);
	}

	private bool CheckClass(ProfileSkill skill)
	{
		if (characterClassFilterPanel.selected == null)
		{
			return true;
		}
		return skill.def.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
	}

	public void RefreshSkills()
	{
		List<ProfileSkill> elements = UIBase.applicationState.globalSkillManager.allSkills.Where(CheckClass).OrderByDescending(CanConfig).ThenBy((ProfileSkill skill) => skill, ProfileSkill.GetAllProfileSkillSortMethod()[sortDropDown.value].Item2)
			.ToList();
		skillPool.pool.UpdateSequenceDisplay(elements, scrollRectGridLayoutGroup, SkillDisplay.profileSkillConvertor);
		selectedSkill = null;
		selectedSkillDisplay = null;
		SetDirty();
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		selectedSkillDisplay = module.parent as SkillDisplay;
		selectedSkill = (module.parent as SkillDisplay).setupData.profileSkill;
		skillConfigPresetPanel.SelectSkill(selectedSkill);
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		selectedSkillDisplay = null;
		selectedSkill = null;
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		return selectedSkill == (module.parent as SkillDisplay).setupData.profileSkill;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		ProfileSkill profileSkill = (module.parent as SkillDisplay).setupData.profileSkill;
		return CanConfig(profileSkill);
	}
}
