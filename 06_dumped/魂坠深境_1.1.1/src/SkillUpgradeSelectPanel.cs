using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUpgradeSelectPanel : UIBase, ISelectablePanel, IShowAsync, IHideAsync
{
	[SerializeField]
	private GameButton confirmButton;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton unselectButton;

	[SerializeField]
	private SkillDisplay skillDisplay;

	[SerializeField]
	private CellUIPrefabPoolConfig<SkillDisplay, GameSkill> skillDisplayPool;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private SkillInfoPanel skillInfoPanel;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	private Func<GameSkill> customInitialSkillGetter;

	private Func<ProfileSkill, bool> customCanSelect;

	private Action<GameSkill> customConfirm;

	private Action customBack;

	private ScrollRect skillScrollRect;

	private RectTransform skillScrollContent;

	private float lockedSkillContentX;

	private SkillUpgradePanel skillUpgradePanel => GetComponentInParent<SkillUpgradePanel>(includeInactive: true);

	protected override void Init()
	{
		InitUI(skillDisplay);
		InitUI(skillInfoPanel);
		Init(skillDisplayPool);
		skillDisplayPool.pool.ConfigInfoPanel(skillInfoPanel);
		InitSkillScrollRect();
		confirmButton.onClick.AddListener(OnConfirm);
		backButton.onClick.AddListener(OnBack);
		unselectButton.onClick.AddListener(delegate
		{
			skillDisplay.Unset();
			SetDirty();
		});
		if (sortDropDown != null)
		{
			EnsureDropdownSortsOnTop(sortDropDown);
			sortDropDown.ClearOptions();
			sortDropDown.AddOptions((from c in ProfileSkill.GetAllProfileSkillSortMethod()
				select new TMP_Dropdown.OptionData(c.Item1)).ToList());
			sortDropDown.SetValueWithoutNotify(0);
			sortDropDown.RefreshShownValue();
			sortDropDown.onValueChanged.AddListener(delegate
			{
				RefreshSkills();
			});
		}
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
		characterClassFilterPanel.gameObject.SetActive(value: true);
		IconObjectFilterPanel iconObjectFilterPanel = characterClassFilterPanel;
		iconObjectFilterPanel.onChangeSelected = (Action)Delegate.Combine(iconObjectFilterPanel.onChangeSelected, new Action(RefreshSkills));
		SetDirty();
	}

	private void InitSkillScrollRect()
	{
		skillScrollContent = ((layoutGroup != null) ? layoutGroup.RectTransform() : null);
		skillScrollRect = ((layoutGroup != null) ? layoutGroup.GetComponentInParent<ScrollRect>(includeInactive: true) : null);
		if (!(skillScrollRect == null))
		{
			if (skillScrollContent != null)
			{
				lockedSkillContentX = skillScrollContent.anchoredPosition.x;
			}
			skillScrollRect.horizontal = false;
			skillScrollRect.movementType = ScrollRect.MovementType.Clamped;
			skillScrollRect.onValueChanged.AddListener(delegate
			{
				LockSkillListHorizontalPosition();
			});
			LockSkillListHorizontalPosition();
		}
	}

	private void LateUpdate()
	{
		LockSkillListHorizontalPosition();
	}

	private void LockSkillListHorizontalPosition()
	{
		if (skillScrollRect != null)
		{
			skillScrollRect.horizontal = false;
			skillScrollRect.velocity = new Vector2(0f, skillScrollRect.velocity.y);
			skillScrollRect.horizontalNormalizedPosition = 0f;
		}
		if (!(skillScrollContent == null))
		{
			Vector2 anchoredPosition = skillScrollContent.anchoredPosition;
			if (!Mathf.Approximately(anchoredPosition.x, lockedSkillContentX))
			{
				anchoredPosition.x = lockedSkillContentX;
				skillScrollContent.anchoredPosition = anchoredPosition;
			}
		}
	}

	private static void EnsureDropdownSortsOnTop(TMP_Dropdown dropdown)
	{
		Canvas val = dropdown.GetComponent<Canvas>();
		if ((UnityEngine.Object)(object)val == null)
		{
			val = dropdown.gameObject.AddComponent<Canvas>();
		}
		val.overrideSorting = true;
		val.sortingOrder = 200;
		if (dropdown.GetComponent<GraphicRaycaster>() == null)
		{
			dropdown.gameObject.AddComponent<GraphicRaycaster>();
		}
	}

	public void ShowForUpgradeTarget()
	{
		ClearCustomSelectionMode();
		manager.ShowUI(this);
	}

	public void ShowForBreakthroughMaterial(SkillBreakthroughPanel breakthroughPanel, SkillDisplay targetSkillDisplay)
	{
		customInitialSkillGetter = () => targetSkillDisplay.setupData;
		customCanSelect = breakthroughPanel.CanSelect;
		customConfirm = delegate(GameSkill selectedSkill)
		{
			if (selectedSkill != null)
			{
				targetSkillDisplay.Setup(selectedSkill);
			}
			else
			{
				targetSkillDisplay.Unset();
			}
			breakthroughPanel.selecting = null;
			breakthroughPanel.SetDirty();
		};
		customBack = delegate
		{
			breakthroughPanel.selecting = null;
			breakthroughPanel.SetDirty();
		};
		manager.ShowUI(this);
	}

	void IShowAsync.OnShowStart()
	{
		base.gameObject.SetActive(value: true);
		base.transform.SetAsLastSibling();
		RefreshSkills();
		if (customInitialSkillGetter != null)
		{
			GameSkill gameSkill = customInitialSkillGetter();
			if (gameSkill != null)
			{
				skillDisplay.Setup(gameSkill);
			}
			else
			{
				skillDisplay.Unset();
			}
		}
		else if (skillUpgradePanel.selectingSkill != null)
		{
			skillDisplay.Setup(skillUpgradePanel.selectingSkill.GetGameSkill());
		}
		else
		{
			skillDisplay.Unset();
		}
		SetDirty();
	}

	private void RefreshSkills()
	{
		List<ProfileSkill> list = UIBase.applicationState.globalSkillManager.allSkills.Where(CheckClass).ToList();
		List<(string, IComparer<ProfileSkill>)> allProfileSkillSortMethod = ProfileSkill.GetAllProfileSkillSortMethod();
		if (allProfileSkillSortMethod.Count == 0)
		{
			return;
		}
		int index = ((sortDropDown != null) ? Mathf.Clamp(sortDropDown.value, 0, allProfileSkillSortMethod.Count - 1) : 0);
		list.Sort(allProfileSkillSortMethod[index].Item2);
		if (customCanSelect != null)
		{
			list = list.OrderBy((ProfileSkill s) => (!customCanSelect(s)) ? 1 : 0).ToList();
		}
		skillDisplayPool.pool.UpdateSequenceDisplay(list, layoutGroup, SkillDisplay.profileSkillConvertor);
		LockSkillListHorizontalPosition();
		bool CheckClass(ProfileSkill skill)
		{
			if (characterClassFilterPanel.selected == null)
			{
				return true;
			}
			return skill.def.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
		}
	}

	private void OnConfirm()
	{
		if (customConfirm != null)
		{
			if (skillDisplay.setupData?.profileSkill != null && customCanSelect != null && !customCanSelect(skillDisplay.setupData.profileSkill))
			{
				skillDisplay.Unset();
				SetDirty();
			}
			else
			{
				customConfirm(skillDisplay.setupData);
				manager.HideUI(this);
				ClearCustomSelectionMode();
			}
		}
		else
		{
			if (skillDisplay.setupData != null)
			{
				skillUpgradePanel.selectingSkill = skillDisplay.setupData.profileSkill;
			}
			else
			{
				skillUpgradePanel.selectingSkill = null;
			}
			manager.HideUI(this);
			skillUpgradePanel._OnTagSwitched();
		}
	}

	private void OnBack()
	{
		customBack?.Invoke();
		manager.HideUI(this);
		ClearCustomSelectionMode();
		skillUpgradePanel.SetDirty();
	}

	void IHideAsync.OnHideStart()
	{
		ClearCustomSelectionMode();
	}

	public IEnumerator HideAnim()
	{
		base.gameObject.SetActive(value: false);
		yield break;
	}

	private void ClearCustomSelectionMode()
	{
		customInitialSkillGetter = null;
		customCanSelect = null;
		customConfirm = null;
		customBack = null;
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		SkillDisplay skillDisplay = (SkillDisplay)module.parent;
		this.skillDisplay.Setup(skillDisplay.setupData);
		SetDirty();
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		skillDisplay.Unset();
		SetDirty();
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		SkillDisplay skillDisplay = (SkillDisplay)module.parent;
		return this.skillDisplay.setupData?.profileSkill == skillDisplay?.setupData?.profileSkill;
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		SkillDisplay skillDisplay = (SkillDisplay)module.parent;
		if (skillDisplay?.setupData != null)
		{
			if (customCanSelect != null)
			{
				return customCanSelect(skillDisplay.setupData.profileSkill);
			}
			return true;
		}
		return false;
	}
}
