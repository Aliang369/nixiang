using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.SkillOverview;

public class SkillStoragePanel : UIBase
{
	[SerializeField]
	private SkillInfoPanel skillInfoPanel;

	[SerializeField]
	private GameButton toSkillUpgradePanelButton;

	[SerializeField]
	private GameButton toDeleteSkillPanelButton;

	[SerializeField]
	private TextMeshProUGUI skillCountText;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private SkillDisplay skillCellPrefab;

	[SerializeField]
	private Transform allSkillAreaCellParent;

	[SerializeField]
	private IconObjectFilterPanel characterClassFilterPanel;

	[SerializeField]
	private GameObject characterClassFilterTextArea;

	[SerializeField]
	private TextMeshProUGUI characterClassFilterText;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[NonSerialized]
	private CellUIPrefabPool<SkillDisplay, GameSkill> allSkillPool;

	[NonSerialized]
	private SkillOverviewPanel overviewPanel;

	private SkillUpgradePanel skillUpgradePanel => UIBase.fortressState.uiManager.skill_GemPanel.skillUpgradePanel;

	private DeleteSkillPanel deleteSkillPanel => UIBase.fortressState.uiManager.skill_GemPanel.skillUpgradePanel.deleteSkillPanel;

	protected override void Init()
	{
		overviewPanel = parent as SkillOverviewPanel;
		InitUI(skillInfoPanel);
		GetPrefabPool(ref allSkillPool, skillCellPrefab, null, skillInfoPanel);
		CellUIPrefabPool<SkillDisplay, GameSkill> cellUIPrefabPool = allSkillPool;
		cellUIPrefabPool.onInit = (Action<SkillDisplay>)Delegate.Combine(cellUIPrefabPool.onInit, (Action<SkillDisplay>)delegate(SkillDisplay s)
		{
			s.GetComponent<SkillWordModule>().onClick = _OnClickPoolSkillCell;
		});
		toSkillUpgradePanelButton.onClick.AddListener(_OnToSkillUpgradePanelButtonClicked);
		toDeleteSkillPanelButton.onClick.AddListener(_OnToDeleteSkillPanelButtonClicked);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		sortDropDown.options = (from c in ProfileSkill.GetAllProfileSkillSortMethod()
			select new TMP_Dropdown.OptionData(c.Item1)).ToList();
		sortDropDown.onValueChanged.AddListener(_OnSortDropdownValueChanged);
		UpdateDisplay();
		InitUI(characterClassFilterPanel);
		characterClassFilterPanel.Sync(UIBase.applicationState.characterManager.GetAllCharacterClassIconFilterDatas(includeGeneral: true, ScriptableObjectSingleton<TextDataPreset>.instance.term_character_class_arg0_sub_skills));
	}

	protected override void Dispose()
	{
		base.Dispose();
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
	}

	protected override void UpdateDisplay()
	{
		List<ProfileSkill> list = UIBase.applicationState.globalSkillManager.allSkills.Where(CheckClass).ToList();
		list.Sort(ProfileSkill.GetAllProfileSkillSortMethod()[sortDropDown.value].Item2);
		List<GameSkill> elements = list.Select((ProfileSkill s) => s.GetGameSkill()).ToList();
		allSkillPool.UpdateSequenceDisplay(elements, layoutGroup);
		toSkillUpgradePanelButton.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillUpgrade));
		toDeleteSkillPanelButton.gameObject.SetActive(UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillDelete));
		skillCountText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_amount_arg0.FormatWith(list.Count);
		if (characterClassFilterPanel.selected == null)
		{
			characterClassFilterTextArea.gameObject.SetActive(value: false);
			return;
		}
		characterClassFilterTextArea.gameObject.SetActive(value: true);
		characterClassFilterText.text = characterClassFilterPanel.selected.setupData.title;
		bool CheckClass(ProfileSkill skill)
		{
			if (characterClassFilterPanel.selected == null)
			{
				return true;
			}
			return skill.def.attachCharacterClass == characterClassFilterPanel.selected.setupData.target;
		}
	}

	private void HideSkillInfoDisplay()
	{
		skillInfoPanel.HideAllCommonInfo();
	}

	public void _OnClickPoolSkillCell(SkillDisplay cell)
	{
		overviewPanel.SetDirty();
	}

	private void _OnEmptyAreaClicked()
	{
		HideSkillInfoDisplay();
	}

	private void _OnToSkillUpgradePanelButtonClicked()
	{
		HideSkillInfoDisplay();
		skillUpgradePanel.SetActiveAndDirtyWithFadeAnim(active: true);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "地下室-强化护符");
	}

	private void _OnToDeleteSkillPanelButtonClicked()
	{
		HideSkillInfoDisplay();
		deleteSkillPanel.ShowUI();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIBase.applicationState, "地下室-销毁护符");
	}

	private void _OnSortDropdownValueChanged(int value)
	{
		SetDirty();
	}
}
