using System.Collections.Generic;

public class AllPrepareSkillsPresetSelector : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		if (eve.gameState?.playerState?.prepareSkillsPreset == null)
		{
			return null;
		}
		return eve.gameState.playerState.prepareSkillsPreset.Skills;
	}
}
