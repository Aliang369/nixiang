public class CheckSpellIsTriggerCast : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		return (eve.spellInfo as RealSpellCastInfo)?.isTriggerCast ?? false;
	}
}
