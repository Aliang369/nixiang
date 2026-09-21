using System.Collections.Generic;
using System.Linq;

public class CoolDowningActivatedSkillSelctor : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		if (eve.gameState?.playerState == null)
		{
			return null;
		}
		return eve.gameState.playerState.ActivatedSkills.Where((GameSkill s) => !s.CoolDowned).ToList();
	}
}
