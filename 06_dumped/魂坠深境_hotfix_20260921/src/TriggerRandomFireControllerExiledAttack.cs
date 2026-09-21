public class TriggerRandomFireControllerExiledAttack : FireControllerAttackEffect
{
	public int baseDamage = 6;

	public int upgradedBaseDamage = 9;

	public int damagePerExiledItem = 3;

	public int upgradedDamagePerExiledItem = 4;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 6), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBaseDamage, 9));
		int num = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(damagePerExiledItem, 3), FireControllerExileEffect.UseConfiguredOrDefault(upgradedDamagePerExiledItem, 4)) * FireControllerExileEffect.GetExilePileItems(eve).Count;
		FireControllerExileEffect.DealDamageToOpponent(eve, tierValue + num);
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 6), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBaseDamage, 9));
		int tierValue2 = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(damagePerExiledItem, 3), FireControllerExileEffect.UseConfiguredOrDefault(upgradedDamagePerExiledItem, 4));
		tierValue += tierValue2 * FireControllerExileEffect.GetExilePileItems(eve).Count;
		NodeDisplayInfo nodeDisplayInfo = FireControllerExileEffect.DamageDisplay(eve, tierValue);
		nodeDisplayInfo.value = new NodeDisplayInfo.Element<int?>(tierValue2);
		return nodeDisplayInfo;
	}
}
