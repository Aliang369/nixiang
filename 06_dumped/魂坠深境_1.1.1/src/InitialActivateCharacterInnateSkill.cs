using System.Collections.Generic;
using System.Linq;

public class InitialActivateCharacterInnateSkill : GameConfigEffect
{
	public override List<FeatureInfoData> GenerateFeatureInfoData(PlayerCharacterState character)
	{
		return GetDisplayInnateSkills(character).Select(delegate(GameSkill skill)
		{
			FeatureDisplay.DisplayInfo obj = new FeatureDisplay.DisplayInfo
			{
				target = skill,
				icon = skill.skillDef.Icon,
				name = skill.NameWithQualityColor,
				hideTypeName = true
			};
			PlayerCharacterState playerCharacterState = character;
			obj.statusText = ((playerCharacterState != null && playerCharacterState.CharacterInnateSkillUnlocked) ? string.Empty : ScriptableObjectSingleton<TextDataPreset>.instance.term_character_innate_skill_need_unlock_talent);
			return new FeatureInfoData(obj);
		}).ToList();
	}

	public override string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig, TextDataGenerator generator)
	{
		List<GameSkill> displayInnateSkills = GetDisplayInnateSkills(character);
		string text = (displayInnateSkills.IsNullOrEmpty() ? ScriptableObjectSingleton<TextDataPreset>.instance.term_skill : generator.GetSkillsText(displayInnateSkills));
		string text2 = character?.Name ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_character;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_character_innate_skill_initial_activate_arg0_arg1.FormatWith(text2, text);
	}

	private List<GameSkill> GetDisplayInnateSkills(PlayerCharacterState character)
	{
		return character?.GetCharacterCurrentInnateProfileSkillsOfUpgradeCount(character.CharacterTalentInnateSkillUpgradeCount, showLocked: true).Select((ProfileSkill skill) => new GameSkill(skill)).ToList() ?? new List<GameSkill>();
	}
}
