public class CheckGameEventSkillPropertyType : CheckNode
{
	public SkillDef.SkillPropertyType type;

	public override bool? Check(GameEvent eve)
	{
		if (eve.skillPropertyType == type)
		{
			return true;
		}
		return false;
	}
}
