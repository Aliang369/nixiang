public class DealFireControllerSkyFireDamage : FireControllerAttackEffect
{
	public int baseDamage = 10;

	public int upgradedBaseDamage = 15;

	public int skinBaseDamage = 30;

	public int times = 4;

	public int skinMinTimes = 1;

	public int skinMaxTimes = 6;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFXConfig transformVFXEffect;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 10), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBaseDamage, 15), FireControllerExileEffect.UseConfiguredOrDefault(skinBaseDamage, 30));
		int num = FireControllerExileEffect.UseConfiguredOrDefault(times, 4);
		int min = FireControllerExileEffect.UseConfiguredOrDefault(skinMinTimes, 1);
		int num2 = FireControllerExileEffect.UseConfiguredOrDefault(skinMaxTimes, 6);
		int baseTimes = (FireControllerExileEffect.IsSkinBranch(eve) ? eve.gameState.randomManager.Next(min, num2 + 1) : num);
		FireControllerExileEffect.DealDamageToOpponent(eve, new DamageCommand
		{
			baseDamage = tierValue,
			source = eve.CasterItem,
			targetBFT = eve.OpponentBFT,
			overflow = true,
			spellCastInfo = eve.spellInfo,
			transformSetup = transformVFXEffect,
			transform3DSetup = transformEffect3D,
			hitSetup = hitEffect,
			hit3DSetup = hitEffect3D
		}, baseTimes);
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(baseDamage, 10), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBaseDamage, 15), FireControllerExileEffect.UseConfiguredOrDefault(skinBaseDamage, 30));
		bool flag = FireControllerExileEffect.IsSkinBranch(eve);
		NodeDisplayInfo nodeDisplayInfo = FireControllerExileEffect.DamageDisplay(eve, tierValue, flag ? 1 : FireControllerExileEffect.UseConfiguredOrDefault(times, 4));
		if (flag)
		{
			int num = FireControllerExileEffect.UseConfiguredOrDefault(skinMinTimes, 1);
			int num2 = FireControllerExileEffect.UseConfiguredOrDefault(skinMaxTimes, 6);
			string value = ((num == num2) ? num.ToString() : ScriptableObjectSingleton<TextDataPreset>.instance.term_range_arg0_arg1.FormatWith(num, num2));
			nodeDisplayInfo.str = new NodeDisplayInfo.Element<string>(value);
		}
		return nodeDisplayInfo;
	}
}
