using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;

public class GemCombinePanel : UIBase, ISelectablePanel, IShowAsync, IHideAsync
{
	private enum NotCombineableReason
	{
		None,
		NotEnoughGem,
		DifferentLevel,
		NotEnoughMatter
	}

	[SerializeField]
	private CombineGemSlotDisplay combineGemSlotDisplay1;

	[SerializeField]
	private CombineGemSlotDisplay combineGemSlotDisplay2;

	[SerializeField]
	private CombineGemSlotDisplay combineGemSlotDisplay3;

	[SerializeField]
	private SkillGemDisplay skillGemDisplay;

	[SerializeField]
	private ScrollRectGridLayoutGroup layoutGroup;

	[SerializeField]
	private GameButton combineButton;

	[SerializeField]
	private SkillGemInfoPanel infoPanel;

	[SerializeField]
	private TextMeshProUGUI hint;

	[SerializeField]
	private MatterCell matterCell;

	[SerializeField]
	private TMP_Dropdown sortDropDown;

	[SerializeField]
	private Animator animator;

	[NonSerialized]
	private CellUIPrefabPool<SkillGemDisplay, SkillGem> pool;

	[NonSerialized]
	public CombineGemSlotDisplay current;

	[NonSerialized]
	private SkillGemDef selectedInventoryGemDef;

	private SkillGemDisplay slotGemDisplay1 => combineGemSlotDisplay1.skillGemDisplay;

	private SkillGemDisplay slotGemDisplay2 => combineGemSlotDisplay2.skillGemDisplay;

	private SkillGemDisplay slotGemDisplay3 => combineGemSlotDisplay3.skillGemDisplay;

	private SkillGem gem1 => slotGemDisplay1?.setupData;

	private SkillGem gem2 => slotGemDisplay2?.setupData;

	private SkillGem gem3 => slotGemDisplay3?.setupData;

	private bool enableGemCombine => UIBase.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillGemCombine);

	private bool CanCombine => GetNotCombineReason == NotCombineableReason.None;

	private NotCombineableReason GetNotCombineReason
	{
		get
		{
			if (gem1 == null || gem2 == null || gem3 == null)
			{
				return NotCombineableReason.NotEnoughGem;
			}
			if (gem1.def.level != gem2.def.level || gem2.def.level != gem3.def.level)
			{
				return NotCombineableReason.DifferentLevel;
			}
			if (!UIBase.applicationState.matterManager.HasEnoughMatter(cost))
			{
				return NotCombineableReason.NotEnoughMatter;
			}
			return NotCombineableReason.None;
		}
	}

	private MatterInfo cost
	{
		get
		{
			if (gem1 == null && gem2 == null && gem3 == null)
			{
				return null;
			}
			return UIBase.applicationState.skillGemManager.GetCombineCost(Level());
			int Level()
			{
				if (gem1 != null)
				{
					return gem1.def.level;
				}
				if (gem2 != null)
				{
					return gem2.def.level;
				}
				if (gem3 != null)
				{
					return gem3.def.level;
				}
				return -1;
			}
		}
	}

	private string hintText
	{
		get
		{
			if (gem1 == null && gem2 == null && gem3 == null)
			{
				return null;
			}
			try
			{
				int num = Level();
				Dictionary<int, int> dictionary = ScriptableObjectSingleton<FortressDataManager>.instance.gemCombineProbability.TryGetValue(num);
				if (dictionary.Count == 1)
				{
					return ScriptableObjectSingleton<TextDataPreset>.instance.term_combine_gem_cost_hint1.FormatWith(num);
				}
				int num2 = dictionary[num];
				int num3 = dictionary[num + 1];
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_combine_gem_cost_hint.FormatWith((float)num2 / (float)(num2 + num3) * 100f, num, (float)num3 / (float)(num2 + num3) * 100f, num + 1);
			}
			catch
			{
				return "";
			}
			int Level()
			{
				if (gem1 != null)
				{
					return gem1.def.level;
				}
				if (gem2 != null)
				{
					return gem2.def.level;
				}
				if (gem3 != null)
				{
					return gem3.def.level;
				}
				return -1;
			}
		}
	}

	protected override void Init()
	{
		InitUI(matterCell);
		InitUI(infoPanel);
		InitUI(combineGemSlotDisplay1);
		InitUI(combineGemSlotDisplay2);
		InitUI(combineGemSlotDisplay3);
		GetPrefabPool(ref pool, skillGemDisplay, null, infoPanel);
		combineButton.onClick.AddListener(OnCombine);
		combineButton.onNotInteractableClick.AddListener(OnNotCombineAbleClick);
		sortDropDown.options = UIBase.applicationState.skillGemManager.GetSortOptions();
		sortDropDown.value = 0;
		sortDropDown.onValueChanged.AddListener(delegate
		{
			SetDirty();
		});
		SelectNextEmptySlot();
		async void OnCombine()
		{
			if (!enableGemCombine)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillGemCombine));
			}
			else if (CanCombine)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_menu_gem_combine);
				await UIBase.applicationState.skillGemManager.Combine(gem1, gem2, gem3);
				Setup();
				manager.SetDirty();
				UIBase.applicationState.savingManager.SetProfileDirtyToSave();
			}
		}
		void OnNotCombineAbleClick()
		{
			if (!enableGemCombine)
			{
				UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillGemCombine));
			}
			else
			{
				switch (GetNotCombineReason)
				{
				case NotCombineableReason.NotEnoughGem:
					UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_need_three_same_level_gem_to_combine_gem);
					break;
				case NotCombineableReason.DifferentLevel:
					UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_three_gem_level_not_same_not_combine_able);
					break;
				case NotCombineableReason.NotEnoughMatter:
					UIBase.fortressState.uiManager.quickHintPanel.HintText(ScriptableObjectSingleton<TextDataPreset>.instance.term_not_enough_matter);
					break;
				case NotCombineableReason.None:
					break;
				}
			}
		}
	}

	public void Setup()
	{
		if (!enableGemCombine)
		{
			UIBase.fortressState.uiManager.quickHintPanel.HintText(UIBase.applicationState.clavisManager.GetGlobalVariableUnlockHint(ApplicationGlobalVariableManager.EnableSkillGemCombine));
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		slotGemDisplay1.Unset();
		slotGemDisplay2.Unset();
		slotGemDisplay3.Unset();
		selectedInventoryGemDef = null;
		infoPanel.HideAllCommonInfo();
		SelectNextEmptySlot();
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		matterCell.gameObject.SetActive(value: false);
		hint.gameObject.SetActive(value: false);
		pool.UpdateSequenceDisplay(UIBase.applicationState.skillGemManager.sortMethods[sortDropDown.value].Item2(UIBase.applicationState.skillGemManager.allSkillGems), layoutGroup);
		combineButton.interactable = enableGemCombine && CanCombine;
		if (cost != null)
		{
			matterCell.gameObject.SetActive(value: true);
			matterCell.Setup(cost);
		}
		UpdateHintText();
	}

	public void SelectNextEmptySlot()
	{
		if (slotGemDisplay1.setupData == null)
		{
			current = combineGemSlotDisplay1;
		}
		else if (slotGemDisplay2.setupData == null)
		{
			current = combineGemSlotDisplay2;
		}
		else if (slotGemDisplay3.setupData == null)
		{
			current = combineGemSlotDisplay3;
		}
		else
		{
			current = combineGemSlotDisplay1;
		}
	}

	public void OnCombineSlotClicked(CombineGemSlotDisplay slot)
	{
		if (!(slot == null))
		{
			if (current == slot && slot.skillGemDisplay.setupData != null)
			{
				ClearSlot(slot);
				return;
			}
			current = slot;
			ShowSlotGemInfo(slot);
			SetDirty();
		}
	}

	public bool IsInventoryGemSelected(SkillGem skillGem)
	{
		if (skillGem != null)
		{
			return selectedInventoryGemDef == skillGem.def;
		}
		return false;
	}

	private void ClearSlot(CombineGemSlotDisplay slot)
	{
		if (!(slot?.skillGemDisplay == null))
		{
			slot.skillGemDisplay.Unset();
			current = slot;
			infoPanel.HideAllCommonInfo();
			SetDirty();
		}
	}

	private void ShowSlotGemInfo(CombineGemSlotDisplay slot)
	{
		infoPanel.HideAllCommonInfo();
		SkillGem skillGem = slot?.skillGemDisplay?.setupData;
		if (skillGem != null)
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(skillGem, CommonInfoData.ControlType.Click, slot.skillGemDisplay.rectTransform, false));
		}
	}

	private void ShowInventoryGemInfo(SkillGemDisplay display)
	{
		infoPanel.HideAllCommonInfo();
		if (display?.setupData != null)
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(display.setupData, CommonInfoData.ControlType.Click, display.rectTransform, false));
		}
	}

	private bool TryPutInventoryGem(SelectableModuleBase module, bool playNegativeSound)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (current == null)
		{
			SelectNextEmptySlot();
		}
		if (current?.skillGemDisplay == null)
		{
			return false;
		}
		SkillGemDisplay skillGemDisplay = module.parent as SkillGemDisplay;
		SkillGem skillGem = skillGemDisplay?.setupData;
		if (skillGem == null)
		{
			return false;
		}
		selectedInventoryGemDef = skillGem.def;
		ShowInventoryGemInfo(skillGemDisplay);
		if (!CanPutGemInCurrentSlot(skillGem))
		{
			if (playNegativeSound)
			{
				Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_click_nagative);
			}
			SetDirty();
			return false;
		}
		CombineGemSlotDisplay combineGemSlotDisplay = current;
		bool wasEmpty = combineGemSlotDisplay.skillGemDisplay.setupData == null;
		SkillGem setupData = new SkillGem(skillGem.def);
		combineGemSlotDisplay.skillGemDisplay.Setup(setupData);
		combineGemSlotDisplay.skillGemDisplay.SetDirty();
		SelectSlotAfterPutGem(combineGemSlotDisplay, wasEmpty);
		SetDirty();
		return true;
	}

	private void SelectSlotAfterPutGem(CombineGemSlotDisplay editedSlot, bool wasEmpty)
	{
		if (wasEmpty && TryGetNextEmptySlot(editedSlot, out var nextEmptySlot))
		{
			current = nextEmptySlot;
		}
		else
		{
			current = editedSlot;
		}
	}

	private bool TryGetNextEmptySlot(CombineGemSlotDisplay editedSlot, out CombineGemSlotDisplay nextEmptySlot)
	{
		CombineGemSlotDisplay[] array = new CombineGemSlotDisplay[3] { combineGemSlotDisplay1, combineGemSlotDisplay2, combineGemSlotDisplay3 };
		int num = Array.IndexOf(array, editedSlot);
		if (num < 0)
		{
			num = 0;
		}
		for (int i = 1; i <= array.Length; i++)
		{
			CombineGemSlotDisplay combineGemSlotDisplay = array[(num + i) % array.Length];
			if (combineGemSlotDisplay.skillGemDisplay.setupData == null)
			{
				nextEmptySlot = combineGemSlotDisplay;
				return true;
			}
		}
		nextEmptySlot = null;
		return false;
	}

	private void UpdateHintText()
	{
		hint.gameObject.SetActive(value: true);
		if (gem1 == null && gem2 == null && gem3 == null)
		{
			hint.text = base.textData.term_put_in_three_same_level_gem_to_combine_gem;
			return;
		}
		if (gem1 != null && gem2 != null && gem3 != null)
		{
			if (!UIBase.applicationState.matterManager.HasEnoughMatter(cost))
			{
				hint.text = base.textData.term_matter_arg0_is_not_enough.FormatWith(cost.matterDef.DisplayMatterName);
				return;
			}
			if (CanCombine)
			{
				hint.text = hintText;
				return;
			}
		}
		int num = Level();
		hint.text = base.textData.term_put_in_more_gem.FormatWith(num);
		int Level()
		{
			if (gem1 != null)
			{
				return gem1.def.level;
			}
			if (gem2 != null)
			{
				return gem2.def.level;
			}
			if (gem3 != null)
			{
				return gem3.def.level;
			}
			return 1;
		}
	}

	void ISelectablePanel.Select(SelectableModuleBase module)
	{
		TryPutInventoryGem(module, playNegativeSound: false);
	}

	void ISelectablePanel.Unselect(SelectableModuleBase module)
	{
		TryPutInventoryGem(module, playNegativeSound: true);
	}

	bool ISelectablePanel.IsSelected(SelectableModuleBase module)
	{
		SkillGem skillGem = (module.parent as SkillGemDisplay)?.setupData;
		return IsInventoryGemSelected(skillGem);
	}

	bool ISelectablePanel.CanSelect(SelectableModuleBase module)
	{
		SkillGem skillGem = (module.parent as SkillGemDisplay)?.setupData;
		return CanPutGemInCurrentSlot(skillGem);
	}

	private bool CanPutGemInCurrentSlot(SkillGem skillGem)
	{
		if (skillGem == null)
		{
			return false;
		}
		if (current == null)
		{
			SelectNextEmptySlot();
		}
		if (current == null)
		{
			return false;
		}
		if (!CheckLevelValid(slotGemDisplay1))
		{
			return false;
		}
		if (!CheckLevelValid(slotGemDisplay2))
		{
			return false;
		}
		if (!CheckLevelValid(slotGemDisplay3))
		{
			return false;
		}
		return HasEnoughGem(skillGem);
		bool CheckLevelValid(SkillGemDisplay skillGemDisplay)
		{
			if (skillGemDisplay.setupData != null && skillGemDisplay.setupData.def.level != skillGem.def.level)
			{
				return false;
			}
			return true;
		}
		bool HasEnoughGem(SkillGem selectedGem)
		{
			SkillGem skillGem2 = gem1;
			SkillGem skillGem3 = gem2;
			SkillGem skillGem4 = gem3;
			if (current == combineGemSlotDisplay1)
			{
				skillGem2 = selectedGem;
			}
			else if (current == combineGemSlotDisplay2)
			{
				skillGem3 = selectedGem;
			}
			else if (current == combineGemSlotDisplay3)
			{
				skillGem4 = selectedGem;
			}
			List<SkillGem> list = new List<SkillGem>();
			if (skillGem2 != null)
			{
				list.Add(skillGem2);
			}
			if (skillGem3 != null)
			{
				list.Add(skillGem3);
			}
			if (skillGem4 != null)
			{
				list.Add(skillGem4);
			}
			List<SkillGem> allSkillGems = UIBase.applicationState.skillGemManager.allSkillGems;
			foreach (KeyValuePair<string, int> gemCount in (from g in list
				group g by g.def.id).ToDictionary((IGrouping<string, SkillGem> g) => g.Key, (IGrouping<string, SkillGem> g) => g.Count()))
			{
				SkillGem skillGem5 = allSkillGems.Find((SkillGem g) => g.def.id == gemCount.Key);
				if (skillGem5 == null || gemCount.Value > skillGem5.amount)
				{
					return false;
				}
			}
			return true;
		}
	}

	void IShowAsync.OnShowStart()
	{
		Setup();
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
