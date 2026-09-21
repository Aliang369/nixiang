using System;
using System.Collections;
using Common.UI;
using TMPro;
using UnityEngine;

public class Skill_GemPanel : UIBase, IStackPanel, IShowAsync, IHideAsync
{
	[SerializeField]
	private SwitchPanel switchPanel;

	[SerializeField]
	private SwitchTag skillTag;

	[SerializeField]
	private SwitchTag gemTag;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton hintButton;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	public SkillUpgradeBasePanel skillUpgradeBasePanel;

	[SerializeField]
	public GemCombinePanel gemCombinePanel;

	[SerializeField]
	public SkillUpgradePanel skillUpgradePanel;

	private bool enableGemCombine => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillGemCombine);

	public RectTransform BackButtonRect
	{
		get
		{
			if (!(backButton != null))
			{
				return null;
			}
			return backButton.transform as RectTransform;
		}
	}

	public void ShowSkillTitle()
	{
		title.text = base.textData.term_skill;
	}

	public void ShowSkillCraftTitle()
	{
		title.text = base.textData.term_skill_craft;
	}

	protected override void Init()
	{
		InitUI(switchPanel);
		InitUI(skillUpgradeBasePanel);
		InitUI(gemCombinePanel);
		InitUI(skillUpgradePanel);
		skillTag.SetText(base.textData.term_skill);
		gemTag.SetText(base.textData.term_gem);
		gemTag.canSelectByClick = (SwitchTag _) => enableGemCombine;
		gemTag.onSelectBlockedByClick = delegate
		{
			ShowGemLockedHint();
		};
		switchPanel.Select(skillTag);
		skillUpgradeBasePanel.gameObject.SetActive(value: true);
		SwitchPanel obj = switchPanel;
		obj.onTagSwitched = (Action)Delegate.Combine(obj.onTagSwitched, new Action(_OnTagSwitched));
		backButton.onClick.AddListener(((UIBase)this).HideUI);
		gemTag.AddNotInteractableClickListener(delegate
		{
			ShowGemLockedHint();
		});
	}

	private void _OnTagSwitched()
	{
		SetDirty();
		UIHelper.GetDefaultCommonInfoPanel().HideAll();
		gemCombinePanel.gameObject.SetActive(value: false);
		skillUpgradeBasePanel.gameObject.SetActive(value: false);
		skillUpgradePanel.gameObject.SetActive(value: false);
		if (switchPanel.targetShowTag == skillTag)
		{
			ShowSkillTitle();
			skillUpgradeBasePanel.ShowUI();
		}
		else if (switchPanel.targetShowTag == gemTag)
		{
			if (!enableGemCombine)
			{
				ShowGemLockedHint();
				switchPanel.Select(skillTag);
			}
			else
			{
				title.text = base.textData.term_gem;
				gemCombinePanel.ShowUI();
			}
		}
	}

	protected override void UpdateDisplay()
	{
		gemTag.SetInteractable(enableGemCombine);
	}

	private void ShowGemLockedHint()
	{
		if (!enableGemCombine)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillGemCombine));
		}
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		base.gameObject.SetActive(value: true);
		ResetToSkillList();
		SetDirty();
		yield return null;
	}

	IEnumerator IHideAsync.HideUIAsync()
	{
		ClearSkillCraftState();
		base.gameObject.SetActive(value: false);
		yield return null;
	}

	private void ResetToSkillList()
	{
		gemCombinePanel.gameObject.SetActive(value: false);
		skillUpgradePanel.gameObject.SetActive(value: false);
		switchPanel.Select(skillTag);
		ShowSkillTitle();
		skillUpgradeBasePanel.ShowUI();
	}

	private void ClearSkillCraftState()
	{
		skillUpgradePanel.selectingSkill = null;
		skillUpgradeBasePanel.ClearSelection();
	}
}
