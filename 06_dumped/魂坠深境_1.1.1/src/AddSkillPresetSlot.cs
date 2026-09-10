using UnityEngine;

public class AddSkillPresetSlot : GameConfigEffect
{
	public int index;

	public Sprite wordSlotIcon;

	public Sprite noWordSlotIcon;

	public override void Prepare(CharacterGameInitConfig characterGameConfig = null)
	{
		if (characterGameConfig != null)
		{
			characterGameConfig.unlockedSkillPresetSlotCount++;
		}
	}

	public override string GenerateTalentDisplayName(PlayerCharacterState character, TalentDef def)
	{
		if (character == null)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_add_skill_preset_slot;
		}
		PresetSkillSlot talentUnlockSlot = GetTalentUnlockSlot(character);
		if (talentUnlockSlot != null)
		{
			if (talentUnlockSlot.slotWord != null)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_skill_preset_slot_with_word;
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_add_skill_preset_slot;
		}
		return "";
	}

	public override string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character, TalentDef def)
	{
		generator = null;
		if (character == null)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_character_skill_preset_slot_add_one;
		}
		PresetSkillSlot talentUnlockSlot = GetTalentUnlockSlot(character);
		if (talentUnlockSlot != null)
		{
			if (talentUnlockSlot.slotWord != null)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_skill_preset_slot_unlock_name_arg0_description_arg1.FormatWith(talentUnlockSlot.slotWord.wordName, talentUnlockSlot.slotWord.GetFormatDescription(base.applicationState, out generator));
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_character_skill_preset_slot_add_one;
		}
		return "";
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_talent_character_skill_preset_slot_add_one;
	}

	public override Sprite GenerateTalentDisplayIcon(PlayerCharacterState character, TalentDef def)
	{
		PresetSkillSlot talentUnlockSlot = GetTalentUnlockSlot(character);
		if (talentUnlockSlot != null)
		{
			if (talentUnlockSlot.slotWord != null)
			{
				return wordSlotIcon;
			}
			return noWordSlotIcon;
		}
		return null;
	}

	private PresetSkillSlot GetTalentUnlockSlot(PlayerCharacterState character)
	{
		return character.skillPreset.configSkillSlots.IndexOrNull(character.def.initSkillSlotCount + index);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(index)
		};
	}
}
