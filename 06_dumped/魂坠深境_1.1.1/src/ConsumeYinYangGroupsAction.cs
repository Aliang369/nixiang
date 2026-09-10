internal sealed class ConsumeYinYangGroupsAction : ExecuteAction
{
	private readonly GameEvent castEvent;

	private readonly DealDamage damageEffect;

	private readonly int remainingGroups;

	private readonly ChangeEnchant temporaryDamageEffect;

	private readonly int yangPerGroup;

	private readonly int yinPerGroup;

	private readonly ValueGenerator temporaryDamageValueGenerator;

	private readonly bool upgraded;

	public ConsumeYinYangGroupsAction(GameEvent castEvent, int yinPerGroup, int yangPerGroup, int remainingGroups, DealDamage damageEffect, ChangeEnchant temporaryDamageEffect, ValueGenerator temporaryDamageValueGenerator, bool upgraded)
	{
		this.castEvent = castEvent;
		this.yinPerGroup = yinPerGroup;
		this.yangPerGroup = yangPerGroup;
		this.remainingGroups = remainingGroups;
		this.damageEffect = damageEffect;
		this.temporaryDamageEffect = temporaryDamageEffect;
		this.temporaryDamageValueGenerator = temporaryDamageValueGenerator;
		this.upgraded = upgraded;
	}

	protected override void Update()
	{
		TrigramsManager trigramsManager = gameState?.trigramsManager;
		if (trigramsManager != null && remainingGroups > 0 && (int)trigramsManager.yinPower >= yinPerGroup && (int)trigramsManager.yangPower >= yangPerGroup)
		{
			EnqueueAction(new TrigramsManager.ChangYinYangPowerAction(-yinPerGroup, -yangPerGroup, TrigramsManager.DurationType.Temp, castEvent.CasterItem));
			EnqueueAction(delegate
			{
				damageEffect.Cast(castEvent);
			});
			EnqueueAction(delegate
			{
				temporaryDamageEffect.deltaValueGenerator = temporaryDamageValueGenerator ?? new PredefinedValueGenerator
				{
					value = (upgraded ? 5 : 3)
				};
				temporaryDamageEffect.Cast(castEvent);
			});
			EnqueueAction(new ConsumeYinYangGroupsAction(castEvent, yinPerGroup, yangPerGroup, remainingGroups - 1, damageEffect, temporaryDamageEffect, temporaryDamageValueGenerator, upgraded));
		}
	}
}
