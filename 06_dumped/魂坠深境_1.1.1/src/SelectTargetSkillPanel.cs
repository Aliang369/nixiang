using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class SelectTargetSkillPanel : CellUI<SelectTargetSkillPanel.DisplayInfo>, ISelectablePanel, IShowAsync, IHideAsync
{
	public class DisplayInfo
	{
		public Action<GameSkill> onConfirm;

		public Func<List<ProfileSkill>, List<ProfileSkill>> onSort;

		public Func<ProfileSkill, bool> poolFilter;

		public Func<ProfileSkill, bool> canSelect;

		public Func<DisplayInfo, bool> canConfirm;

		public Action onBack;

		public bool confirmOnSelect;

		public bool closeOnConfirm = true;

		public bool closeOnBack = true;
	}

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton dropButton;

	[SerializeField]
	private SkillDisplay skillCellPrefab;

	[SerializeField]
	private SkillInfoPanel skillInfoPanel;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	public SkillDisplay skillDisplay;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> skillCellPool;

	private ProfileSkill targetSkill => skillDisplay.setupData?.profileSkill;

	private DisplayInfo displayInfo => base.setupData;

	protected override void Init()
	{
		InitUI(skillInfoPanel);
		GetPrefabPool(ref skillCellPool, skillCellPrefab, null, skillInfoPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		dropButton.onClick.AddListener(_OnEmptyAreaClicked);
		sortDropDown.options = (from c in ProfileSkill.GetAllProfileSkillSortMethod()
			select new TMP_Dropdown.OptionData(c.Item1)).ToList();
		sortDropDown.onValueChanged.AddListener(_OnSortDropdownValueChanged);
		UIHelper.RegisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.Sync(CellUI<DisplayInfo>.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
		characterClassFilterPanel.gameObject.SetActive(value: true);
	}

	protected override void Dispose()
	{
		backButton.onClick.RemoveListener(_OnBackButtonClicked);
		confirmButton.onClick.RemoveListener(_OnConfirmButtonClicked);
		UIHelper.UnregisterEmptyAreaClickedEvent(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		RefreshSkills();
		if (targetSkill != null)
		{
			dropButton.gameObject.SetActive(value: true);
		}
		else
		{
			dropButton.gameObject.SetActive(value: false);
			skillInfoPanel.HideAllCommonInfo();
		}
		if (displayInfo.canConfirm == null)
		{
			confirmButton.interactable = targetSkill != null;
		}
		else
		{
			confirmButton.interactable = displayInfo.canConfirm(displayInfo);
		}
	}

	protected override void OnSetup(DisplayInfo setupData)
	{
		base.gameObject.SetActive(value: true);
		skillDisplay.Unset();
		layoutGroup.ClearAllDisplayElement();
	}

	private void RefreshSkills()
	{
		List<ProfileSkill> list = CellUI<DisplayInfo>.applicationState.globalSkillManager.allSkills.ToList();
		if (displayInfo.poolFilter != null)
		{
			list.RemoveAll((ProfileSkill s) => displayInfo.poolFilter(s));
		}
		if (displayInfo.onSort != null)
		{
			list = displayInfo.onSort(list);
		}
		else
		{
			list.Sort(ProfileSkill.GetAllProfileSkillSortMethod()[sortDropDown.value].Item2);
		}
		skillCellPool.UpdateSequenceDisplay(list.Where(CheckClass).ToList(), layoutGroup, SkillDisplay.profileSkillConvertor);
		bool CheckClass(ProfileSkill skill)
		{
			if (characterClassFilterPanel.selected == null)
			{
				return true;
			}
			return skill.def.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
		}
	}

	public void SetSelected(ProfileSkill profileSkill)
	{
		if (profileSkill == null)
		{
			this.skillDisplay.Unset();
			return;
		}
		SkillDisplay skillDisplay = skillCellPool.GetActiveElements().Find((SkillDisplay c) => c.setupData.profileSkill == profileSkill);
		if (skillDisplay == null)
		{
			this.skillDisplay.Unset();
			return;
		}
		this.skillDisplay.Setup(skillDisplay.setupData);
		SetDirty();
	}

	private void _OnBackButtonClicked()
	{
		skillDisplay.Unset();
		if (displayInfo.onBack != null)
		{
			displayInfo.onBack();
		}
		else if (displayInfo.closeOnBack)
		{
			SetActiveAndDirtyWithFadeAnim(active: false);
		}
		else
		{
			SetDirty();
		}
	}

	private void _OnConfirmButtonClicked()
	{
		displayInfo.onConfirm?.Invoke(skillDisplay.setupData);
		if (displayInfo.closeOnConfirm)
		{
			base.gameObject.SetActive(value: false);
		}
		else
		{
			SetDirty();
		}
	}

	private void _OnEmptyAreaClicked()
	{
		skillDisplay.Unset();
		SetDirty();
	}

	private void _OnSortDropdownValueChanged(int value)
	{
		SetDirty();
	}

	public void Select(SelectableModuleBase module)
	{
		skillDisplay.Setup((module.parent as SkillDisplay).setupData);
		if (displayInfo.confirmOnSelect && (displayInfo.canConfirm == null || displayInfo.canConfirm(displayInfo)))
		{
			displayInfo.onConfirm?.Invoke(skillDisplay.setupData);
		}
		SetDirty();
	}

	public void Unselect(SelectableModuleBase module)
	{
		skillDisplay.Unset();
		SetDirty();
	}

	public bool IsSelected(SelectableModuleBase module)
	{
		return skillDisplay.setupData?.profileSkill == (module.parent as SkillDisplay).setupData.profileSkill;
	}

	public bool CanSelect(SelectableModuleBase module)
	{
		if (displayInfo.canSelect == null)
		{
			return true;
		}
		return displayInfo.canSelect((module.parent as SkillDisplay).setupData.profileSkill);
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		base.gameObject.SetActive(value: true);
		yield return null;
	}

	IEnumerator IHideAsync.HideUIAsync()
	{
		base.gameObject.SetActive(value: false);
		yield return null;
	}
}
