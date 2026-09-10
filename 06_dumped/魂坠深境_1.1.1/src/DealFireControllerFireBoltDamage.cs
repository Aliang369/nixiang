public class DealFireControllerFireBoltDamage : FireControllerAttackEffect
{
	public int baseDamage = 4;

	public int upgradedBaseDamage = 6;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		FireControllerExileEffect.DealDamageToOpponent(eve, FireControllerExileEffect.GetTierValue(eve, baseDamage, upgradedBaseDamage));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return FireControllerExileEffect.DamageDisplay(eve, FireControllerExileEffect.GetTierValue(eve, baseDamage, upgradedBaseDamage));
	}
}
