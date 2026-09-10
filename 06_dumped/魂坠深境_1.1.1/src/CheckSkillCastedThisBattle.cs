public class CheckSkillCastedThisBattle : CheckNode
{
	public SkillSelector skillSelector;

	public override bool? Check(GameEvent eve)
	{
		GameSkill gameSkill = skillSelector?.GetSingleSkill(eve);
		if (gameSkill == null || eve.gameState == null)
		{
			return null;
		}
		return eve.gameState.recordManager.castedSkillsThisBattle.Contains(gameSkill);
	}
}
