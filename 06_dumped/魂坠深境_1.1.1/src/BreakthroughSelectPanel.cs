using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using UnityEngine;

public class BreakthroughSelectPanel : UIBase, ISelectablePanel
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
	private SkillDisplay skillDisplayPrefab;

	[SerializeField]
	private SkillInfoPanel skillInfoPanel;

	private Action<GameSkill> onConfirm;

	private ProfileSkill currentSelectedSkill;

	private CellUIPrefabPool<SkillDisplay, GameSkill> skillDisplayPool;

	private SkillBreakthroughPanel breakthroughPanel => GetComponentInParent<SkillBreakthroughPanel>(includeInactive: true);

	protected override void Init()
	{
		InitUI(skillDisplay);
		InitUI(skillInfoPanel);
		GetPrefabPool(ref skillDisplayPool, skillDisplayPrefab, null, skillInfoPanel);
		confirmButton.onClick.AddListener(OnConfirm);
		backButton.onClick.AddListener(OnBack);
		unselectButton.onClick.AddListener(delegate
		{
			skillDisplay.Unset();
			SetDirty();
		});
		SetDirty();
	}

	public void Setup()
	{
		base.gameObject.SetActive(value: true);
		base.transform.SetAsLastSibling();
		List<ProfileSkill> source = UIBase.applicationState.globalSkillManager.allSkills.ToList();
		source = source.OrderBy((ProfileSkill s) => (!breakthroughPanel.CanSelect(s)) ? 1 : 0).ToList();
		skillDisplayPool.Sync(source.Select((ProfileSkill s) => new GameSkill(s)).ToList());
		if (breakthroughPanel.selecting != null && breakthroughPanel.selecting.setupData != null && breakthroughPanel.CanSelect(breakthroughPanel.selecting.setupData.profileSkill))
		{
			skillDisplay.Setup(breakthroughPanel.selecting.setupData);
		}
		else
		{
			skillDisplay.Unset();
		}
		SetDirty();
	}

	private void OnConfirm()
	{
		if (skillDisplay.setupData != null)
		{
			breakthroughPanel.selecting.Setup(skillDisplay.setupData);
		}
		else
		{
			breakthroughPanel.selecting.Unset();
		}
		base.gameObject.SetActive(value: false);
		breakthroughPanel.selecting = null;
		breakthroughPanel.SetDirty();
	}

	private void OnBack()
	{
		base.gameObject.SetActive(value: false);
		breakthroughPanel.selecting = null;
		breakthroughPanel.SetDirty();
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
		if (skillDisplay?.setupData != null && breakthroughPanel != null)
		{
			return breakthroughPanel.CanSelect(skillDisplay.setupData.profileSkill);
		}
		return false;
	}
}
