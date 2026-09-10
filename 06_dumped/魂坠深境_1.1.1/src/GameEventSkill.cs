using System.Collections.Generic;

public class GameEventSkill : SkillSelector
{
	public GameEvent.EveObjType type;

	public override List<GameSkill> GetTargetSkills(GameEvent eve)
	{
		GameSkill eveObj = eve.GetEveObj<GameSkill>(type);
		if (eveObj != null)
		{
			return new List<GameSkill> { eveObj };
		}
		return new List<GameSkill>();
	}
}
