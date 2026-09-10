public class CheckGameEventWithSpellInfo : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return eve.spellInfo != null;
	}
}
