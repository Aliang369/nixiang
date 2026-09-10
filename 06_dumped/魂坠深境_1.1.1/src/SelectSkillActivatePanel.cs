using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectSkillActivatePanel : CellUI<SelectSkillInfo>
{
	public class AiActivateSkillCandidate
	{
		public int Index;

		public ProfileSkillPresetSlotCell PresetCell;

		public SkillSlotDisplay TargetSlotDisplay;

		public GameSkill Skill => (PresetCell?.skillSlot as GameSkillSlot)?.gameSkill;

		public GameSkillSlot TargetSlot => TargetSlotDisplay?.skillSlot;
	}

	[SerializeField]
	public SkillReplaceConfirmPanel skillReplaceConfirmPanel;

	[SerializeField]
	public GameSkillPresetArea skillPresetArea;

	[SerializeField]
	private GameButton cancelButton;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private TextMeshProUGUI confirmText;

	[SerializeField]
	private Image characterIcon;

	[SerializeField]
	public SkillSlotPanel usingSkillSlotPanel;

	[NonSerialized]
	private bool activating;

	[NonSerialized]
	public GameSkillSlot selectingPresetSlot;

	[NonSerialized]
	private ProfileSkillPresetSlotCell selectingPresetSlotCell;

	[NonSerialized]
	public UniversalCommonInfoPanel infoPanel;

	public bool IsAiDecisionActive
	{
		get
		{
			if (base.gameObject.activeInHierarchy && selectInfo != null)
			{
				return !activating;
			}
			return false;
		}
	}

	public bool CanCancelForAiSession => IsAiDecisionActive;

	private SelectSkillInfo selectInfo => base.setupData;

	protected override void Init()
	{
		InitUI(skillPresetArea);
		InitUI(usingSkillSlotPanel);
		InitUI(skillReplaceConfirmPanel);
		cancelButton.onClick.AddListener(_OnCancelButtonClicked);
		helpButton.onClick.AddListener(_OnHelpButtonClicked);
		CellUI<SelectSkillInfo>.applicationState.eventManager.UIEmptyClickedEvent.AddListener(_OnEmptyAreaClicked);
		GameSkillPresetArea gameSkillPresetArea = skillPresetArea;
		gameSkillPresetArea.onSelect = (Action<ProfileSkillPresetSlotCell>)Delegate.Combine(gameSkillPresetArea.onSelect, new Action<ProfileSkillPresetSlotCell>(_OnSelectSkillPresetSlot));
		SkillSlotPanel skillSlotPanel = usingSkillSlotPanel;
		skillSlotPanel.onSelectedChanged = (Action<SkillSlotDisplay>)Delegate.Combine(skillSlotPanel.onSelectedChanged, new Action<SkillSlotDisplay>(_OnUsingSkillSlotSelectedChanged));
		infoPanel = UIHelper.GetDefaultCommonInfoPanel();
	}

	protected override void Dispose()
	{
		CellUI<SelectSkillInfo>.applicationState.eventManager.UIEmptyClickedEvent.RemoveListener(_OnEmptyAreaClicked);
		SkillSlotPanel skillSlotPanel = usingSkillSlotPanel;
		skillSlotPanel.onSelectedChanged = (Action<SkillSlotDisplay>)Delegate.Remove(skillSlotPanel.onSelectedChanged, new Action<SkillSlotDisplay>(_OnUsingSkillSlotSelectedChanged));
	}

	protected override void OnSetup(SelectSkillInfo setupData)
	{
		activating = false;
		selectingPresetSlot = null;
		selectingPresetSlotCell = null;
		skillPresetArea.Setup(CellUI<SelectSkillInfo>.gameState.playerState.prepareSkillsPreset, CellUI<SelectSkillInfo>.gameState.playerState.character);
		characterIcon.sprite = CellUI<SelectSkillInfo>.gameState.playerState.character.Avatar;
		base.gameObject.SetActive(value: true);
		ForceUpdate();
		usingSkillSlotPanel.ForceUpdate();
		usingSkillSlotPanel.SelectFirstEmptySlot();
		CellUI<SelectSkillInfo>.applicationState.eventManager.OpenSelectSkillPanelEvent?.TriggerEvent();
		SetDirty();
	}

	protected override void OnUnset()
	{
		selectingPresetSlot = null;
		selectingPresetSlotCell = null;
		base.gameObject.SetActive(value: false);
		UIHelper.HideAllInfo(base.transform);
		infoPanel.HideAll();
	}

	protected override void UpdateDisplay()
	{
		if (selectInfo == null)
		{
			return;
		}
		if (selectingPresetSlot?.gameSkill == null)
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_to_activate_skill;
		}
		else if (usingSkillSlotPanel.selected == null)
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_please_select_to_swap_skill;
		}
		else if (selectingPresetSlot?.gameSkill != null && !WithEnoughActivateCost(selectingPresetSlot.gameSkill))
		{
			if (selectingPresetSlot.gameSkill.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCoinCost)
			{
				confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_this_skill_activateable_cost_not_enough_coin_need_arg0_count.FormatWith(GetSkillFinalActivateCost(selectingPresetSlot.gameSkill));
			}
			else if (selectingPresetSlot.gameSkill.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCrystalCost)
			{
				confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_this_skill_activateable_cost_not_enough_crystal_need_arg0_count.FormatWith(GetSkillFinalActivateCost(selectingPresetSlot.gameSkill));
			}
		}
		else if (usingSkillSlotPanel.selected.skillSlot.gameSkill == null && selectingPresetSlot?.gameSkill != null && selectingPresetSlot.gameSkill.ReplaceToActivate)
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_info_need_another_replace_activate;
		}
		else if (usingSkillSlotPanel.selected.skillSlot.gameSkill == null && selectingPresetSlot?.gameSkill != null)
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_activate_skill_arg0.FormatWith(selectingPresetSlot.gameSkill.NameWithQualityAndUpgrade);
		}
		else if (usingSkillSlotPanel.selected.skillSlot.gameSkill != null && selectingPresetSlot?.gameSkill != null)
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_swap_skill_arg0_to_arg1.FormatWith(usingSkillSlotPanel.selected.skillSlot.gameSkill.NameWithQualityAndUpgrade, selectingPresetSlot.gameSkill.NameWithQualityAndUpgrade);
		}
		else
		{
			confirmText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_this_skill_is_not_activateable;
		}
		if (selectInfo.canCancel)
		{
			cancelButton.gameObject.SetActive(value: true);
		}
		else
		{
			cancelButton.gameObject.SetActive(value: false);
		}
	}

	private bool WithEnoughActivateCost(GameSkill skill)
	{
		int skillFinalActivateCost = GetSkillFinalActivateCost(skill);
		if (skill.ActivateCostType == SkillDef.SkillPropertyType.None)
		{
			return true;
		}
		if (skill.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCoinCost && (int)CellUI<SelectSkillInfo>.gameState.playerState.coinCount >= skillFinalActivateCost)
		{
			return true;
		}
		if (skill.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCrystalCost && (int)CellUI<SelectSkillInfo>.gameState.playerState.crystalCount >= skillFinalActivateCost)
		{
			return true;
		}
		return false;
	}

	private int GetSkillFinalActivateCost(GameSkill skill)
	{
		int num = skill.ActivateCost;
		if (skill.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCrystalCost && CellUI<SelectSkillInfo>.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NeedCrystalCostForUnlockSkill))
		{
			num += ScriptableObjectSingleton<GameDataManager>.instance.highDifficultyUnlockSkillCrystalCost;
		}
		return num;
	}

	private bool CanActivateSkill(GameSkill toActive, SkillSlotDisplay selectedSlot)
	{
		if (toActive == null || selectedSlot?.skillSlot == null)
		{
			return false;
		}
		if (!WithEnoughActivateCost(toActive))
		{
			return false;
		}
		if (selectedSlot.skillSlot.gameSkill == null && toActive.ReplaceToActivate)
		{
			return false;
		}
		return true;
	}

	private void ActiveSkill(GameSkill toActive, SkillSlotDisplay selectedSlot)
	{
		if (selectedSlot?.skillSlot != null)
		{
			activating = true;
			CellUI<SelectSkillInfo>.gameState.StartCoroutine(Cor);
		}
		IEnumerator Cor()
		{
			try
			{
				if (selectedSlot.skillSlot.gameSkill == null)
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new ActivateSkillAction(toActive, selectedSlot.skillSlot));
				}
				else
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new SwapSkillAction(selectedSlot.skillSlot, toActive));
				}
				CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_amulet_activate));
				if (toActive.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCoinCost)
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new ChangeCoinAction(-toActive.ActivateCost));
				}
				else if (toActive.ActivateCostType == SkillDef.SkillPropertyType.ActivateWithCrystalCost)
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new ChangeCrystalAction(-toActive.ActivateCost));
				}
				if (CellUI<SelectSkillInfo>.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NeedCrystalCostForUnlockSkill))
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new ChangeCrystalAction(-ScriptableObjectSingleton<GameDataManager>.instance.highDifficultyUnlockSkillCrystalCost));
				}
				CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(SetDirty);
				CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_UI_ActivateSkill_a, selectedSlot.transform.position, null));
				ProfileSkillPresetSlotCell profileSkillPresetSlotCell = selectingPresetSlotCell ?? skillPresetArea.GetSlot(selectingPresetSlot);
				if (profileSkillPresetSlotCell != null)
				{
					CellUI<SelectSkillInfo>.gameState.actionManager.EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_UI_ActivateSkill_b, profileSkillPresetSlotCell.transform.position, null, waitForEnd: true));
				}
				selectInfo.confirmCallback?.Invoke(toActive);
			}
			catch (Exception ex)
			{
				TraceLog.Error($"[SelectSkillActivatePanel] ActiveSkill() - [{selectedSlot == null}] [{selectedSlot?.skillSlot == null}] [{selectedSlot?.skillSlot?.gameSkill == null}] [{toActive == null}] - {ex}");
			}
			yield return new WaitUntil(() => CellUI<SelectSkillInfo>.gameState.actionManager.isEmpty);
			Unset();
		}
	}

	private void _OnSelectSkillPresetSlot(ProfileSkillPresetSlotCell cell)
	{
		selectingPresetSlot = cell.setupData.slot as GameSkillSlot;
		selectingPresetSlotCell = cell;
		if (selectingPresetSlot != null)
		{
			ShowSelectingSkillInfo();
			SetDirty();
		}
	}

	private void _OnUsingSkillSlotSelectedChanged(SkillSlotDisplay selected)
	{
		ShowSelectingSkillInfo();
		SetDirty();
	}

	private void ShowSelectingSkillInfo()
	{
		if (selectingPresetSlot?.gameSkill != null && !(selectingPresetSlotCell == null))
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(selectingPresetSlot.gameSkill, CommonInfoData.ControlType.Click, selectingPresetSlotCell.RectTransform(), showUncollectedStatus: false, GetActivateInfo(selectingPresetSlot.gameSkill, usingSkillSlotPanel.selected)));
		}
	}

	private void _OnEmptyAreaClicked()
	{
		selectingPresetSlot = null;
		selectingPresetSlotCell = null;
		infoPanel.HideAll();
		SetDirty();
	}

	private void _OnActiveSkillClicked(GameSkill toActive, SkillSlotDisplay selectedSlot)
	{
		if (activating || !CanActivateSkill(toActive, selectedSlot))
		{
			return;
		}
		infoPanel.HideAll();
		GameSkill gameSkill = selectedSlot?.skillSlot?.gameSkill;
		if (gameSkill == null || !CellUI<SelectSkillInfo>.gameState.showSkillReplaceConfirm)
		{
			ActiveSkill(toActive, selectedSlot);
			return;
		}
		skillReplaceConfirmPanel.Setup(gameSkill, toActive, delegate
		{
			ActiveSkill(toActive, selectedSlot);
		});
	}

	public List<AiActivateSkillCandidate> GetAiActivateSkillCandidates()
	{
		List<AiActivateSkillCandidate> list = new List<AiActivateSkillCandidate>();
		if (!IsAiDecisionActive)
		{
			return list;
		}
		ForceUpdate();
		skillPresetArea.ForceUpdate();
		usingSkillSlotPanel.ForceUpdate();
		foreach (ProfileSkillPresetSlotCell aiSelectableSkillCell in skillPresetArea.GetAiSelectableSkillCells())
		{
			GameSkill gameSkill = (aiSelectableSkillCell.skillSlot as GameSkillSlot)?.gameSkill;
			if (gameSkill == null)
			{
				continue;
			}
			foreach (SkillSlotDisplay aiSelectableSlotDisplay in usingSkillSlotPanel.GetAiSelectableSlotDisplays())
			{
				if (CanActivateSkill(gameSkill, aiSelectableSlotDisplay))
				{
					list.Add(new AiActivateSkillCandidate
					{
						Index = list.Count,
						PresetCell = aiSelectableSkillCell,
						TargetSlotDisplay = aiSelectableSlotDisplay
					});
				}
			}
		}
		return list;
	}

	public bool TryActivateSkillForAiSession(AiActivateSkillCandidate candidate)
	{
		if (!IsAiDecisionActive || candidate?.PresetCell == null || candidate.TargetSlotDisplay == null)
		{
			return false;
		}
		skillPresetArea.SelectSlotCell(candidate.PresetCell);
		usingSkillSlotPanel.SelectForAiSession(candidate.TargetSlotDisplay);
		ForceUpdate();
		if (selectingPresetSlot?.gameSkill == null || usingSkillSlotPanel.selected == null)
		{
			return false;
		}
		if (!CanActivateSkill(selectingPresetSlot.gameSkill, usingSkillSlotPanel.selected))
		{
			return false;
		}
		ActiveSkill(selectingPresetSlot.gameSkill, usingSkillSlotPanel.selected);
		return true;
	}

	public void CancelForAiSession()
	{
		if (CanCancelForAiSession)
		{
			_OnCancelButtonClicked();
		}
	}

	private void _OnCancelButtonClicked()
	{
		if (!activating)
		{
			if (selectInfo != null)
			{
				selectInfo.onCancel?.Invoke();
			}
			Unset();
		}
	}

	private void _OnHelpButtonClicked()
	{
		CellUI<SelectSkillInfo>.gameState.UIManager.guideUIPanel.triggerHint_activateSkill.Show();
	}

	private (object, object)[] GetActivateInfo(GameSkill toActive, SkillSlotDisplay selectedSlot)
	{
		if (!CanActivateSkill(toActive, selectedSlot))
		{
			return Array.Empty<(object, object)>();
		}
		return new(object, object)[1] { (typeof(SkillEquipModule.Info), new SkillEquipModule.Info
		{
			useEquipSprite = true,
			buttonText = ScriptableObjectSingleton<TextDataPreset>.instance.term_active,
			onClick = delegate
			{
				_OnActiveSkillClicked(toActive, selectedSlot);
			}
		}) };
	}
}
