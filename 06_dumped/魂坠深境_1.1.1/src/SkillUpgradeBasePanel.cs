using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkillUpgradeBasePanel : UIBase, IShowAsync, IHideAsync, ISelectablePanel
{
	[SerializeField]
	private CellUIPrefabPoolConfig<SkillDisplay, GameSkill> skillCellPool;

	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private SkillDisplay selectedSkillDisplay;

	[SerializeField]
	private SkillInfoDisplay fixedSkillInfoDisplay;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	public ProfileSkill selectedSkill;

	public RectTransform ConfirmButtonRect
	{
		get
		{
			if (!(confirmButton != null))
			{
				return null;
			}
			return confirmButton.transform as RectTransform;
		}
	}

	protected override void Init()
	{
		Init(skillCellPool);
		InitUI(selectedSkillDisplay);
		if (fixedSkillInfoDisplay != null)
		{
			InitUI(fixedSkillInfoDisplay);
		}
		else
		{
			skillCellPool.pool.ConfigInfoPanel(UIHelper.GetDefaultCommonInfoPanel());
		}
		confirmButton.onClick.AddListener(_OnConfirmButtonClicked);
		TextMeshProUGUI componentInChildren = confirmButton.GetComponentInChildren<TextMeshProUGUI>(includeInactive: true);
		if (componentInChildren != null)
		{
			componentInChildren.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_craft_button;
		}
		confirmButton.gameObject.SetActive(value: false);
		sortDropDown.options = (from c in ProfileSkill.GetAllProfileSkillSortMethod()
			select new TMP_Dropdown.OptionData(c.Item1)).ToList();
		sortDropDown.onValueChanged.AddListener(delegate
		{
			RefreshSkills();
		});
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
		characterClassFilterPanel.gameObject.SetActive(value: true);
		IconObjectFilterPanel iconObjectFilterPanel = characterClassFilterPanel;
		iconObjectFilterPanel.onChangeSelected = (Action)Delegate.Combine(iconObjectFilterPanel.onChangeSelected, new Action(RefreshSkills));
		RefreshSkills();
	}

	protected override void UpdateDisplay()
	{
		UpdateSelectedSkillDisplay();
		UpdateFixedSkillInfoDisplay();
		confirmButton.gameObject.SetActive(selectedSkill != null);
	}

	public void RefreshSkills()
	{
		List<ProfileSkill> list = UIBase.applicationState.globalSkillManager.allSkills.Where(CheckClass).ToList();
		List<(string, IComparer<ProfileSkill>)> allProfileSkillSortMethod = ProfileSkill.GetAllProfileSkillSortMethod();
		if (allProfileSkillSortMethod.Count != 0)
		{
			int index = ((sortDropDown != null) ? Mathf.Clamp(sortDropDown.value, 0, allProfileSkillSortMethod.Count - 1) : 0);
			list.Sort(allProfileSkillSortMethod[index].Item2);
			skillCellPool.pool.UpdateSequenceDisplay(list, layoutGroup, SkillDisplay.profileSkillConvertor);
		}
		bool CheckClass(ProfileSkill skill)
		{
			if (characterClassFilterPanel.selected == null)
			{
				return true;
			}
			return skill.def.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
		}
	}

	public RectTransform GetSkillCellRect(SkillDef targetSkillDef)
	{
		if (targetSkillDef == null || skillCellPool?.pool == null)
		{
			return null;
		}
		SkillDisplay skillDisplay = skillCellPool.pool.GetActiveElements().FirstOrDefault((SkillDisplay cell) => cell.setupData?.profileSkill?.def == targetSkillDef);
		if (!(skillDisplay != null))
		{
			return null;
		}
		return skillDisplay.transform as RectTransform;
	}

	private void UpdateSelectedSkillDisplay()
	{
		if (selectedSkill != null && !UIBase.applicationState.globalSkillManager.allSkills.Contains(selectedSkill))
		{
			selectedSkill = null;
		}
		if (selectedSkill == null)
		{
			selectedSkillDisplay.Unset();
		}
		else
		{
			selectedSkillDisplay.Setup(selectedSkill.GetGameSkill());
		}
	}

	private void UpdateFixedSkillInfoDisplay()
	{
		if (!(fixedSkillInfoDisplay == null))
		{
			if (selectedSkill == null)
			{
				fixedSkillInfoDisplay.Unset();
			}
			else
			{
				fixedSkillInfoDisplay.Setup(selectedSkill.GetGameSkill());
			}
		}
	}

	private void _OnConfirmButtonClicked()
	{
		UIHelper.GetDefaultCommonInfoPanel()?.HideAll();
		if (selectedSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			ClearSelection();
		}
		else
		{
			UIBase.fortressState.uiManager.skill_GemPanel.skillUpgradePanel.selectingSkill = selectedSkill;
			UIBase.fortressState.uiManager.skill_GemPanel.skillUpgradePanel.ShowUI();
			SetDirty();
		}
	}

	public void ClearSelection()
	{
		selectedSkill = null;
		selectedSkillDisplay.Unset();
		fixedSkillInfoDisplay?.Unset();
		confirmButton.gameObject.SetActive(value: false);
		UIHelper.GetDefaultCommonInfoPanel()?.HideAll();
		SetDirty();
	}

	public void Select(SelectableModuleBase module)
	{
		selectedSkill = (module.parent as SkillDisplay)?.setupData?.profileSkill;
		UIHelper.GetDefaultCommonInfoPanel()?.HideAll();
		SetDirty();
	}

	public void Unselect(SelectableModuleBase module)
	{
		ProfileSkill profileSkill = (module.parent as SkillDisplay)?.setupData?.profileSkill;
		if (selectedSkill == profileSkill)
		{
			selectedSkill = null;
		}
		UIHelper.GetDefaultCommonInfoPanel()?.HideAll();
		SetDirty();
	}

	public bool IsSelected(SelectableModuleBase module)
	{
		if (selectedSkill != null)
		{
			return selectedSkill == (module.parent as SkillDisplay)?.setupData?.profileSkill;
		}
		return false;
	}

	public bool CanSelect(SelectableModuleBase module)
	{
		return (module.parent as SkillDisplay)?.setupData?.profileSkill != null;
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		RefreshSkills();
		SetDirty();
	}

	public IEnumerator ShowAnim()
	{
		if (!((UnityEngine.Object)(object)animator == null))
		{
			((Behaviour)(object)animator).enabled = true;
			animator.SetTrigger(AnimKeywords.show);
			yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		}
	}

	public IEnumerator HideAnim()
	{
		if (!base.gameObject.activeInHierarchy)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		if ((UnityEngine.Object)(object)animator == null)
		{
			base.gameObject.SetActive(value: false);
			yield break;
		}
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}
}
