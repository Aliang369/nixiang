using System.Collections.Generic;

public sealed class ConsumeYinYangGroupsEffect : Effect
{
	public CheckNode upgradedCheck;

	public ValueGenerator yinPerGroup;

	public ValueGenerator yangPerGroup;

	public ValueGenerator maximumGroups;

	public DealDamage damageEffect;

	public ChangeEnchant temporaryDamageEffect;

	public ValueGenerator temporaryDamageValueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int valueOrDefault = (yinPerGroup?.GenerateValue(eve)).GetValueOrDefault();
		int valueOrDefault2 = (yangPerGroup?.GenerateValue(eve)).GetValueOrDefault();
		int num = maximumGroups?.GenerateValue(eve) ?? 30;
		if (valueOrDefault <= 0 || valueOrDefault2 <= 0 || num <= 0 || damageEffect == null || temporaryDamageEffect == null)
		{
			return NodeCastResult.Failure;
		}
		bool? flag = upgradedCheck?.Check(eve);
		int num2;
		if (!flag.HasValue)
		{
			Item casterItem = eve.CasterItem;
			num2 = ((casterItem != null && casterItem.upgradeLevel > 0) ? 1 : 0);
		}
		else
		{
			num2 = ((flag == true) ? 1 : 0);
		}
		bool upgraded = (byte)num2 != 0;
		Effect.EnqueueAction(eve, new ConsumeYinYangGroupsAction(eve.ShallowCopy(), valueOrDefault, valueOrDefault2, num, damageEffect, temporaryDamageEffect, temporaryDamageValueGenerator, upgraded));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { yinPerGroup, yangPerGroup, maximumGroups, damageEffect, temporaryDamageEffect, temporaryDamageValueGenerator, upgradedCheck };
	}
}
