using System;
using Common.UI;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SkillUpgradePanel : UIBase, IShowAsync
{
	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton hintButton;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	private SwitchTag addSlotTag;

	[SerializeField]
	private SwitchTag slotTag;

	[SerializeField]
	private SwitchTag breakthroughTag;

	[SerializeField]
	private SwitchTag deleteTag;

	[SerializeField]
	public SkillAddSlotPanel skillAddSlotPanel;

	[SerializeField]
	public DeleteSkillPanel deleteSkillPanel;

	[SerializeField]
	public GemMountingPanel gemMountingPanel;

	[SerializeField]
	public SkillBreakthroughPanel breakthroughPanel;

	[SerializeField]
	public SkillUpgradeSelectPanel skillUpgradeSelectPanel;

	[NonSerialized]
	private bool suppressOperationSwitchCallback;

	[NonSerialized]
	private Sequence operationTransitionSequence;

	[NonSerialized]
	public ProfileSkill selectingSkill;

	private Skill_GemPanel skillGemPanel => (parent as Skill_GemPanel) ?? UIBase.fortressState.uiManager.skill_GemPanel;

	private bool enableAddSlot => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillAddSlot);

	private bool enableMountGem => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillMountGem);

	private bool enableDelete => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillDelete);

	private bool hasGemSlot
	{
		get
		{
			if (selectingSkill != null)
			{
				return selectingSkill.HexShapeSlotCount > 0;
			}
			return false;
		}
	}

	private bool canOpenBreakthrough => selectingSkill != null;

	private bool canOpenAddSlot
	{
		get
		{
			if (enableAddSlot)
			{
				return selectingSkill != null;
			}
			return false;
		}
	}

	private bool canOpenMountGem
	{
		get
		{
			if (enableMountGem && selectingSkill != null)
			{
				return hasGemSlot;
			}
			return false;
		}
	}

	private bool canOpenDelete
	{
		get
		{
			if (enableDelete && selectingSkill != null)
			{
				return selectingSkill.IsDeleteAble;
			}
			return false;
		}
	}

	protected override void Init()
	{
		InitUI(switchPanel);
		InitUI(skillAddSlotPanel);
		InitUI(deleteSkillPanel);
		InitUI(gemMountingPanel);
		InitUI(breakthroughPanel);
		InitUI(skillUpgradeSelectPanel);
		addSlotTag?.SetText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_add_slot_tab);
		slotTag?.SetText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_mount_gem_tab);
		breakthroughTag?.SetText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_breakthrough_tab);
		deleteTag?.SetText(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_delete_tab);
		SetupOperationTagClickGuard();
		UpdateTitle();
		backButton.onClick.AddListener(delegate
		{
			skillGemPanel.skillUpgradeBasePanel.ShowUI();
			skillGemPanel.skillUpgradeBasePanel.SetDirty();
			skillGemPanel.ShowSkillTitle();
			this.HideUI();
		});
		addSlotTag?.AddNotInteractableClickListener(delegate
		{
			if (!enableAddSlot)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillAddSlot));
			}
			else if (selectingSkill == null)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			}
		});
		slotTag?.AddNotInteractableClickListener(delegate
		{
			if (!enableMountGem)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillMountGem));
			}
			else if (selectingSkill == null)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			}
			else if (!hasGemSlot)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(base.textData.term_no_slot);
			}
		});
		breakthroughTag?.AddNotInteractableClickListener(delegate
		{
			if (selectingSkill == null)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			}
		});
		hintButton.onClick.AddListener(delegate
		{
			UIBase.fortressState.uiManager.fortressGuidePanel.upgradeSkillGuideHint.Show();
		});
		deleteTag.AddNotInteractableClickListener(delegate
		{
			if (!enableDelete)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillDelete));
			}
			else if (selectingSkill == null)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
			}
			else if (!selectingSkill.IsDeleteAble)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_contain_skills_not_deletable);
			}
		});
		switchPanel.Select(breakthroughTag);
		breakthroughPanel.gameObject.SetActive(value: true);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
	}

	public void ShowSelectPanel()
	{
		skillUpgradeSelectPanel.ShowForUpgradeTarget();
	}

	public void _OnTagSwitched()
	{
		SetDirty();
		skillAddSlotPanel.gameObject.SetActive(value: false);
		deleteSkillPanel.gameObject.SetActive(value: false);
		gemMountingPanel.gameObject.SetActive(value: false);
		breakthroughPanel.gameObject.SetActive(value: false);
		gemMountingPanel.skillDisplay.gameObject.SetActive(value: false);
		UIBase.fortressState.uiManager.universalCommonInfoPanel.HideAll();
		if (switchPanel.targetShowTag == addSlotTag)
		{
			if (!canOpenAddSlot)
			{
				ShowAddSlotLockedHint();
				SelectDefaultOperation();
			}
			else
			{
				skillAddSlotPanel.ShowUI();
			}
		}
		else if (switchPanel.targetShowTag == deleteTag)
		{
			if (!canOpenDelete)
			{
				ShowDeleteLockedHint();
				SelectDefaultOperation();
			}
			else
			{
				deleteSkillPanel.ShowUI();
			}
		}
		else if (switchPanel.targetShowTag == slotTag)
		{
			if (!canOpenMountGem)
			{
				ShowMountGemLockedHint();
				SelectDefaultOperation();
			}
			else
			{
				gemMountingPanel.ShowUI();
			}
		}
		else if (switchPanel.targetShowTag == breakthroughTag)
		{
			if (!canOpenBreakthrough)
			{
				ShowBreakthroughLockedHint();
			}
			else
			{
				breakthroughPanel.ShowUI();
			}
		}
	}

	protected override void UpdateDisplay()
	{
		breakthroughTag?.SetInteractable(canOpenBreakthrough);
		slotTag?.SetInteractable(canOpenMountGem);
		addSlotTag?.SetInteractable(canOpenAddSlot);
		deleteTag?.SetInteractable(canOpenDelete);
	}

	private void SetupOperationTagClickGuard()
	{
		SetupTagClickGuard(addSlotTag, () => canOpenAddSlot, ShowAddSlotLockedHint);
		SetupTagClickGuard(slotTag, () => canOpenMountGem, ShowMountGemLockedHint);
		SetupTagClickGuard(breakthroughTag, () => canOpenBreakthrough, ShowBreakthroughLockedHint);
		SetupTagClickGuard(deleteTag, () => canOpenDelete, ShowDeleteLockedHint);
	}

	private void SetupTagClickGuard(SwitchTag tag, Func<bool> canSelect, Action showBlockedHint)
	{
		if (!(tag == null))
		{
			tag.canSelectByClick = (SwitchTag _) => canSelect?.Invoke() ?? true;
			tag.onSelectBlockedByClick = delegate
			{
				showBlockedHint?.Invoke();
			};
		}
	}

	private void ShowAddSlotLockedHint()
	{
		if (!enableAddSlot)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillAddSlot));
		}
		else if (selectingSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
		}
	}

	private void ShowMountGemLockedHint()
	{
		if (!enableMountGem)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillMountGem));
		}
		else if (selectingSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
		}
		else if (!hasGemSlot)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_no_slot);
		}
	}

	private void ShowBreakthroughLockedHint()
	{
		if (selectingSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
		}
	}

	private void ShowDeleteLockedHint()
	{
		if (!enableDelete)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillDelete));
		}
		else if (selectingSkill == null)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_skill);
		}
		else if (!selectingSkill.IsDeleteAble)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_contain_skills_not_deletable);
		}
	}

	private void SelectDefaultOperation()
	{
		if (switchPanel.targetShowTag != breakthroughTag)
		{
			switchPanel.Select(breakthroughTag);
		}
	}

	void IShowAsync.OnShowStart()
	{
		if (selectingSkill == null)
		{
			selectingSkill = skillGemPanel.skillUpgradeBasePanel.selectedSkill;
		}
		if (selectingSkill == null)
		{
			skillGemPanel.skillUpgradeBasePanel.ShowUI();
			skillGemPanel.ShowSkillTitle();
			base.gameObject.SetActive(value: false);
			SetDirty();
		}
		else
		{
			base.gameObject.SetActive(value: true);
			switchPanel.Select(breakthroughTag);
			skillGemPanel.ShowSkillCraftTitle();
			UpdateTitle();
			_OnTagSwitched();
		}
	}

	private void UpdateTitle()
	{
		if (title == null)
		{
			title = base.transform.Find("Title/-h-物品名字背景/Title")?.GetComponent<TextMeshProUGUI>();
		}
		if (title != null)
		{
			title.text = ((base.textData != null) ? base.textData.term_skill_craft : ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_craft);
		}
	}
}
