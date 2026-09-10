using System.Collections.Generic;
using System.Linq;

public class AllOtherSkillsSelector : SkillSelector
{
	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		if (eve.gameState?.playerState == null)
		{
			return null;
		}
		List<GameSkill> list = eve.gameState.playerState.ActivatedSkills.ToList();
		list.Remove(eve.GetEveObj<GameSkill>(GameEvent.EveObjType.Caster));
		return list;
	}
}
