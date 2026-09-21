using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress.SkillOverview;

public class GameSkillPresetArea : PresetSkillSlotArea
{
	public GameSkillPreset gameSkillPreset => base.info.gameSkillPreset;

	public void Setup(GameSkillPreset gameSkillPreset, PlayerCharacterState character)
	{
		Setup(new Info
		{
			gameSkillPreset = gameSkillPreset,
			character = character
		});
	}

	protected override void UpdateDisplay()
	{
		GameSkillSlot gameSkillSlot = gameSkillPreset?.gameSkillSlots.FirstOrDefault((GameSkillSlot slot) => slot.characterInnateSkillSlot);
		if (gameSkillSlot == null)
		{
			innateSkillSlot.gameObject.SetActive(value: false);
		}
		else
		{
			innateSkillSlot.gameObject.SetActive(value: true);
			innateSkillSlot.Setup(new ProfileSkillPresetSlotCell.Info
			{
				slot = gameSkillSlot,
				onClick = base.SelectSlotCell,
				character = base.setupData.character
			});
		}
		List<GameSkillSlot> list = gameSkillPreset?.gameSkillSlots.Where((GameSkillSlot slot) => !slot.characterInnateSkillSlot).ToList();
		for (int num = 0; num < skillPresetSlots.Length; num++)
		{
			if (list == null || num >= list.Count)
			{
				skillPresetSlots[num].gameObject.SetActive(value: false);
				continue;
			}
			skillPresetSlots[num].gameObject.SetActive(value: true);
			skillPresetSlots[num].Setup(new ProfileSkillPresetSlotCell.Info
			{
				slot = list[num],
				onClick = base.SelectSlotCell,
				character = base.setupData.character
			});
		}
	}

	public List<ProfileSkillPresetSlotCell> GetAiSelectableSkillCells()
	{
		return (from cell in AllSlots()
			where cell != null && cell.gameObject.activeSelf && cell.setupData != null && cell.skillSlot?.GetGameSkill() != null && (cell.isInner || cell.skillSlot.IsUnLocked(CellUI<Info>.gameState.playerState.characterGameConfig))
			select cell).ToList();
	}

	public ProfileSkillPresetSlotCell GetSlot(GameSkillSlot slot)
	{
		if (innateSkillSlot.skillSlot == slot)
		{
			return innateSkillSlot;
		}
		return skillPresetSlots.FirstOrDefault((ProfileSkillPresetSlotCell c) => c.skillSlot == slot);
	}
}
