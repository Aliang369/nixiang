public class CasterSkillWordStackGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.CasterSkillWord == null)
		{
			return null;
		}
		return eve.CasterSkillWord.stack;
	}
}
