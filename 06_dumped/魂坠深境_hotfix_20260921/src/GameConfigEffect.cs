using System.Collections.Generic;
using UnityEngine;

public abstract class GameConfigEffect : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	protected ApplicationStateInfo applicationState => Bootstrap.applicationState;

	public virtual void Prepare(CharacterGameInitConfig characterGameConfig)
	{
	}

	public virtual string GenerateTalentDisplayName(PlayerCharacterState character, TalentDef def)
	{
		return def.talentDisplayName;
	}

	public virtual Sprite GenerateTalentDisplayIcon(PlayerCharacterState character, TalentDef def)
	{
		return def.icon;
	}

	public virtual string GenerateTalentDisplayDescription(out TextDataGenerator generator, PlayerCharacterState character = null, TalentDef def = null)
	{
		return TextDataPreset.FormatTalentDefEffectDescription(def, applicationState, out generator);
	}

	public virtual string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig)
	{
		return "";
	}

	public virtual string GenerateObsessionDisplayDescription(PlayerCharacterState character, CharacterObsessionLevelConfig obsessionConfig, TextDataGenerator generator)
	{
		return GenerateObsessionDisplayDescription(character, obsessionConfig);
	}

	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public virtual List<FeatureInfoData> GenerateFeatureInfoData(PlayerCharacterState character)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
