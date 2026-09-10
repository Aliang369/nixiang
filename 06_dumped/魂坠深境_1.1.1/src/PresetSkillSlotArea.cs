using System;
using System.Collections.Generic;
using System.Linq;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using UnityEngine;

public class PresetSkillSlotArea : CellUI<PresetSkillSlotArea.Info>
{
	public class Info
	{
		public ProfileSkillPreset profileSkillPreset;

		public GameSkillPreset gameSkillPreset;

		public PlayerCharacterState character;
	}

	[SerializeField]
	public ProfileSkillPresetSlotCell innateSkillSlot;

	[SerializeField]
	public ProfileSkillPresetSlotCell[] skillPresetSlots;

	public Action<ProfileSkillPresetSlotCell> onSelect;

	protected Info info => base.setupData;

	private ProfileSkillPreset skillPreset => info.profileSkillPreset;

	protected override void Init()
	{
		InitUI(innateSkillSlot);
		ProfileSkillPresetSlotCell[] array = skillPresetSlots;
		foreach (ProfileSkillPresetSlotCell child in array)
		{
			InitUI(child);
		}
		ConfigureInfoPanels();
	}

	private void ConfigureInfoPanels()
	{
		UniversalCommonInfoPanel defaultCommonInfoPanel = UIHelper.GetDefaultCommonInfoPanel();
		innateSkillSlot.SetInfoPanel(defaultCommonInfoPanel);
		ProfileSkillPresetSlotCell[] array = skillPresetSlots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetInfoPanel(defaultCommonInfoPanel);
		}
	}

	protected override void UpdateDisplay()
	{
		ConfigureInfoPanels();
		if (skillPreset.innateSkillSlots.Count >= 1)
		{
			innateSkillSlot.gameObject.SetActive(value: true);
			innateSkillSlot.Setup(new ProfileSkillPresetSlotCell.Info
			{
				slot = skillPreset.innateSkillSlots[0],
				onClick = SelectSlotCell,
				character = info.character
			});
		}
		else
		{
			innateSkillSlot.gameObject.SetActive(value: false);
		}
		for (int i = 0; i < skillPreset.configSkillSlots.Count && i < skillPresetSlots.Length; i++)
		{
			skillPresetSlots[i].Setup(new ProfileSkillPresetSlotCell.Info
			{
				slot = skillPreset.configSkillSlots[i],
				onClick = SelectSlotCell,
				character = info.character
			});
		}
	}

	public void Setup(ProfileSkillPreset profileSkillPreset)
	{
		Setup(new Info
		{
			profileSkillPreset = profileSkillPreset,
			character = profileSkillPreset.character
		});
	}

	public void SelectSlotCell(ProfileSkillPresetSlotCell cell)
	{
		onSelect?.Invoke(cell);
	}

	public ProfileSkillPresetSlotCell GetDefaultSelectableSlotCell()
	{
		ProfileSkillPresetSlotCell profileSkillPresetSlotCell = skillPresetSlots.FirstOrDefault((ProfileSkillPresetSlotCell cell) => !cell.isInner && cell.skillSlot?.skill == null);
		if (profileSkillPresetSlotCell != null)
		{
			return profileSkillPresetSlotCell;
		}
		return skillPresetSlots.FirstOrDefault((ProfileSkillPresetSlotCell cell) => cell.skillSlot != null && !cell.isInner);
	}

	public IEnumerable<ProfileSkillPresetSlotCell> AllSlots()
	{
		yield return innateSkillSlot;
		ProfileSkillPresetSlotCell[] array = skillPresetSlots;
		for (int i = 0; i < array.Length; i++)
		{
			yield return array[i];
		}
	}
}
