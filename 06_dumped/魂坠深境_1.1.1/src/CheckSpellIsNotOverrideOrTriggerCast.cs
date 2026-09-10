public class CheckSpellIsNotOverrideOrTriggerCast : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.spellInfo == null)
		{
			return false;
		}
		if (eve.spellInfo.GetCastingOverrideSpellEnchant() != null)
		{
			return false;
		}
		RealSpellCastInfo obj = eve.spellInfo as RealSpellCastInfo;
		return obj == null || !obj.isTriggerCast;
	}
}
