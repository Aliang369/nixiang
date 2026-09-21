using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using DG.Tweening;
using UnityEngine;

public class SkillConfigPresetPanel : CellUI<PlayerCharacterState>
{
	private enum SkillSlotFeedbackKind
	{
		AddNew,
		Replace,
		Unequip
	}

	[SerializeField]
	public PresetSkillSlotArea skillPresetArea;

	[SerializeField]
	public SkillSelectArea skillSelectArea;

	[SerializeField]
	private CellUIPrefabPoolConfig<SkillPresetButtonCell, ProfileSkillPreset> skillPresetButtonPool;

	[NonSerialized]
	private CharacterGameInitConfig characterInitConfig;

	[NonSerialized]
	public ProfileSkillPresetSlotCell selectedCell;

	[NonSerialized]
	private PresetSkillSlot focusSlot;

	[NonSerialized]
	public ICommonInfoPanel infoPanel;

	[NonSerialized]
	private readonly Dictionary<RectTransform, Tween> slotFeedbackTweens = new Dictionary<RectTransform, Tween>();

	public PlayerCharacterState character => base.setupData;

	public ProfileSkillPreset skillPreset => character.skillPreset;

	protected override void Init()
	{
		InitUI(skillPresetArea);
		InitUI(skillSelectArea);
		Init(skillPresetButtonPool);
		PresetSkillSlotArea presetSkillSlotArea = skillPresetArea;
		presetSkillSlotArea.onSelect = (Action<ProfileSkillPresetSlotCell>)Delegate.Combine(presetSkillSlotArea.onSelect, new Action<ProfileSkillPresetSlotCell>(SelectSkillSlotCell));
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		CellUI<PlayerCharacterState>.applicationState.globalSkillManager.ValidateAllSkills();
		infoPanel = UIHelper.GetDefaultCommonInfoPanel();
		selectedCell = null;
		ClearSlotFeedbackTweens();
		skillPresetButtonPool.DespawnAll();
		foreach (ProfileSkillPreset skillPreset in character.skillPresets)
		{
			skillPresetButtonPool.Spawn(skillPreset).onclick = _OnSkillPresetButtonClicked;
		}
		characterInitConfig = character.GenerateCharacterConfigInit(CellUI<PlayerCharacterState>.applicationState);
		RefreshPresetSlots();
		skillSelectArea.Setup();
		base.gameObject.SetActive(value: true);
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(CellUI<PlayerCharacterState>.applicationState, "主仪式间-护符配置", character.def.characterName);
	}

	public void SelectSkillSlotCell(ProfileSkillPresetSlotCell cell)
	{
		if (!cell.isInner && !cell.skillSlot.IsUnLocked(characterInitConfig))
		{
			return;
		}
		skillPresetArea.ForceUpdate();
		if (cell.isInner)
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(cell.skillSlot.skill, CommonInfoData.ControlType.Click, cell.RectTransform(), false));
			return;
		}
		selectedCell = cell;
		if (cell.skillSlot?.skill != null)
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(cell.skillSlot.skill, CommonInfoData.ControlType.Click, cell.RectTransform(), false, GetUnequipInfo(cell)));
		}
		skillSelectArea.RefreshSkills();
		SetDirty();
	}

	public void SelectSkill(ProfileSkill skill)
	{
		if (!CanEquip(skill))
		{
			infoPanel.ShowCommonInfo(new CommonInfoData(skill, CommonInfoData.ControlType.Click, skillSelectArea.selectedSkillDisplay.RectTransform(), false));
		}
		else if (skillSelectArea.selectedSkill != null)
		{
			ShowSkillInfo(skill, GetEquipInfo(skillSelectArea.selectedSkill));
		}
	}

	public void TryConfigSkill(ProfileSkill skill)
	{
		if (CanEquip(skill))
		{
			ProfileSkillPresetSlotCell profileSkillPresetSlotCell = selectedCell;
			PresetSkillSlot slot = profileSkillPresetSlotCell.skillSlot;
			SkillSlotFeedbackKind kind = ((slot.skill != null) ? SkillSlotFeedbackKind.Replace : SkillSlotFeedbackKind.AddNew);
			PlaySlotFeedback(profileSkillPresetSlotCell, kind, delegate
			{
				slot.SetSkill(skill);
			}, delegate
			{
				ThinkingAnalyticsExtends.TrackChangeCharacterSkillSetup(CellUI<PlayerCharacterState>.applicationState, slot, skillPreset);
				CellUI<PlayerCharacterState>.applicationState.eventManager.configSkillPresetEvent.TriggerEvent((slot, skill));
			});
		}
	}

	public bool CanEquip(ProfileSkill skill)
	{
		if (skill == null || selectedCell?.skillSlot == null || characterInitConfig == null)
		{
			return false;
		}
		if (!selectedCell.skillSlot.IsUnLocked(characterInitConfig) || selectedCell.isInner)
		{
			return false;
		}
		return character.CanConfigSkill(selectedCell.skillSlot, skill);
	}

	public void UnEquipSkillFromSlot(PresetSkillSlot slot)
	{
		if (slot != null && !slot.characterInnateSkillSlot && slot.IsUnLocked(characterInitConfig))
		{
			ProfileSkillPresetSlotCell profileSkillPresetSlotCell = skillPresetArea.skillPresetSlots.FirstOrDefault((ProfileSkillPresetSlotCell c) => c.skillSlot == slot);
			if (profileSkillPresetSlotCell != null)
			{
				selectedCell = profileSkillPresetSlotCell;
			}
			PlaySlotFeedback(profileSkillPresetSlotCell, SkillSlotFeedbackKind.Unequip, delegate
			{
				slot.SetSkill(null);
			}, delegate
			{
				ThinkingAnalyticsExtends.TrackChangeCharacterSkillSetup(CellUI<PlayerCharacterState>.applicationState, slot, skillPreset);
			});
		}
	}

	private void RefreshPresetSlots()
	{
		ClearSlotFeedbackTweens();
		skillPresetArea.Setup(skillPreset);
		skillPresetArea.ForceUpdate();
		selectedCell = skillPresetArea.GetDefaultSelectableSlotCell();
		skillSelectArea.RefreshSkills();
	}

	private void _OnSkillPresetButtonClicked(SkillPresetButtonCell cell)
	{
		character.skillPreset = cell.preset;
		RefreshPresetSlots();
		SetDirty();
	}

	private (object, object) GetUnequipInfo(ProfileSkillPresetSlotCell cell)
	{
		return (typeof(SkillEquipModule.Info), new SkillEquipModule.Info
		{
			useEquipSprite = false,
			buttonText = ScriptableObjectSingleton<TextDataPreset>.instance.term_unequip,
			onClick = delegate
			{
				UnEquipSkillFromSlot(cell.skillSlot);
				(infoPanel as UniversalCommonInfoPanel).HideAll();
			}
		});
	}

	private (object, object) GetEquipInfo(ProfileSkill skill)
	{
		string buttonText = ((selectedCell?.skillSlot?.skill != null) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_replace : ScriptableObjectSingleton<TextDataPreset>.instance.term_equip);
		return (typeof(SkillEquipModule.Info), new SkillEquipModule.Info
		{
			useEquipSprite = true,
			buttonText = buttonText,
			onClick = delegate
			{
				TryConfigSkill(skill);
				(infoPanel as UniversalCommonInfoPanel).HideAll();
			}
		});
	}

	private void ShowSkillInfo(ProfileSkill skill, (object key, object value) appendData)
	{
		infoPanel.ShowCommonInfo(new CommonInfoData(skill, CommonInfoData.ControlType.Click, skillSelectArea.selectedSkillDisplay.RectTransform(), false, appendData));
	}

	protected override void Dispose()
	{
		foreach (Tween item in slotFeedbackTweens.Values.ToList())
		{
			item?.Kill();
		}
		slotFeedbackTweens.Clear();
	}

	private void PlaySlotFeedback(ProfileSkillPresetSlotCell cell, SkillSlotFeedbackKind kind, Action applyChange, Action afterChange = null)
	{
		if (applyChange == null)
		{
			return;
		}
		if (cell == null)
		{
			applyChange();
			afterChange?.Invoke();
			SetDirty();
			skillPresetArea.ForceUpdate();
			return;
		}
		RectTransform root = cell.rectTransform;
		SkillDisplay skillCell = cell.skillCell;
		RectTransform rectTransform = ((skillCell != null) ? skillCell.rectTransform : null);
		if (root == null)
		{
			applyChange();
			afterChange?.Invoke();
			SetDirty();
			skillPresetArea.ForceUpdate();
			return;
		}
		KillSlotFeedback(root);
		root.DOKill();
		rectTransform?.DOKill();
		Vector3 localScale = root.localScale;
		Vector3 skillBaseScale = ((rectTransform != null) ? rectTransform.localScale : Vector3.one);
		Sequence sequence = DOTween.Sequence();
		sequence.SetTarget(root);
		sequence.OnComplete(delegate
		{
			RemoveSlotFeedbackTween(root, sequence);
		});
		sequence.OnKill(delegate
		{
			RemoveSlotFeedbackTween(root, sequence);
		});
		switch (kind)
		{
		case SkillSlotFeedbackKind.AddNew:
			applyChange();
			skillPresetArea.ForceUpdate();
			SetDirty();
			afterChange?.Invoke();
			PreparePopInState(root, rectTransform, localScale, skillBaseScale);
			AppendAddNewTween(sequence, root, rectTransform, localScale, skillBaseScale);
			break;
		case SkillSlotFeedbackKind.Replace:
			AppendReplaceTween(sequence, root, rectTransform, localScale, skillBaseScale, applyChange, afterChange);
			break;
		case SkillSlotFeedbackKind.Unequip:
			AppendUnequipTween(sequence, root, rectTransform, localScale, skillBaseScale, applyChange, afterChange);
			break;
		}
		slotFeedbackTweens[root] = sequence;
		sequence.Play();
	}

	private void AppendAddNewTween(Sequence sequence, RectTransform root, RectTransform skillDisplayRect, Vector3 rootBaseScale, Vector3 skillBaseScale)
	{
		sequence.Append(root.DOScale(rootBaseScale * 1.05f, 0.08f).SetEase(Ease.OutQuad));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale * 0.88f, 0.08f).SetEase(Ease.OutQuad));
		}
		sequence.Append(root.DOScale(rootBaseScale, 0.11f).SetEase(Ease.OutBack));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale, 0.12f).SetEase(Ease.OutBack));
		}
	}

	private void AppendReplaceTween(Sequence sequence, RectTransform root, RectTransform skillDisplayRect, Vector3 rootBaseScale, Vector3 skillBaseScale, Action applyChange, Action afterChange)
	{
		sequence.Append(root.DOScale(rootBaseScale * 0.985f, 0.05f).SetEase(Ease.OutQuad));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale * 0.82f, 0.05f).SetEase(Ease.OutQuad));
		}
		sequence.AppendCallback(delegate
		{
			applyChange();
			skillPresetArea.ForceUpdate();
			SetDirty();
			afterChange?.Invoke();
			PreparePopInState(root, skillDisplayRect, rootBaseScale, skillBaseScale);
		});
		sequence.Append(root.DOScale(rootBaseScale * 1.03f, 0.05f).SetEase(Ease.OutQuad));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale * 0.9f, 0.05f).SetEase(Ease.OutQuad));
		}
		sequence.Append(root.DOScale(rootBaseScale, 0.1f).SetEase(Ease.OutBack));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale, 0.11f).SetEase(Ease.OutBack));
		}
	}

	private void AppendUnequipTween(Sequence sequence, RectTransform root, RectTransform skillDisplayRect, Vector3 rootBaseScale, Vector3 skillBaseScale, Action applyChange, Action afterChange)
	{
		sequence.Append(root.DOScale(rootBaseScale * 0.985f, 0.05f).SetEase(Ease.OutQuad));
		if (skillDisplayRect != null)
		{
			sequence.Join(skillDisplayRect.DOScale(skillBaseScale * 0.8f, 0.05f).SetEase(Ease.OutQuad));
		}
		sequence.AppendCallback(delegate
		{
			applyChange();
			skillPresetArea.ForceUpdate();
			SetDirty();
			afterChange?.Invoke();
		});
		sequence.Append(root.DOScale(rootBaseScale, 0.1f).SetEase(Ease.OutBack));
	}

	private void PreparePopInState(RectTransform root, RectTransform skillDisplayRect, Vector3 rootBaseScale, Vector3 skillBaseScale)
	{
		root.localScale = rootBaseScale * 0.98f;
		if (skillDisplayRect != null)
		{
			skillDisplayRect.localScale = skillBaseScale * 0.84f;
		}
	}

	private void KillSlotFeedback(RectTransform root)
	{
		if (!(root == null) && slotFeedbackTweens.TryGetValue(root, out var value))
		{
			slotFeedbackTweens.Remove(root);
			value?.Kill();
		}
	}

	private void RemoveSlotFeedbackTween(RectTransform root, Tween tween)
	{
		if (!(root == null) && slotFeedbackTweens.TryGetValue(root, out var value) && value == tween)
		{
			slotFeedbackTweens.Remove(root);
		}
	}

	private void ClearSlotFeedbackTweens()
	{
		List<Tween> list = slotFeedbackTweens.Values.ToList();
		slotFeedbackTweens.Clear();
		foreach (Tween item in list)
		{
			item?.Kill();
		}
	}
}
