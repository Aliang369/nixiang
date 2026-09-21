using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class RandomActivatedSkillSelector : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		List<GameSkill> list = eve.gameState?.playerState?.ActivatedSkills?.ToList();
		if (list == null || list.Count == 0)
		{
			return new List<GameSkill>();
		}
		return new List<GameSkill> { list.SelectRandom(eve.gameState.randomManager.NextDouble) };
	}
}
