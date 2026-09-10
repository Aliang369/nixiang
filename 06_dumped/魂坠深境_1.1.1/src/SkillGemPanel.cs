using System;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class SkillGemPanel : UIBase
{
	[SerializeField]
	private SkillGemDisplay skillGemDisplay;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private TextMeshProUGUI numText;

	[SerializeField]
	private SkillGemInfoPanel infoPanel;

	[SerializeField]
	private GameButton combineButton;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[NonSerialized]
	private CellUIPrefabPool<SkillGemDisplay, SkillGem> skillGemDisplayPool;

	private GemCombinePanel gemCombinePanel => UIBase.fortressState.uiManager.skill_GemPanel.gemCombinePanel;

	protected override void Init()
	{
		InitUI(infoPanel);
		GetPrefabPool(ref skillGemDisplayPool, skillGemDisplay, null, infoPanel);
		skillGemDisplay.gameObject.SetActive(value: false);
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyClicked);
		sortDropDown.options = UIBase.applicationState.skillGemManager.GetSortOptions();
		sortDropDown.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
	}

	protected override void Dispose()
	{
		UIBase.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyClicked);
	}

	protected override void UpdateDisplay()
	{
		combineButton.interactable = UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillGemCombine);
		skillGemDisplayPool.UpdateSequenceDisplay(UIBase.applicationState.skillGemManager.sortMethods[sortDropDown.value].Item2(UIBase.applicationState.skillGemManager.allSkillGems).ToList(), layoutGroup);
	}

	private void _OnEmptyClicked()
	{
		SetDirty();
	}
}
