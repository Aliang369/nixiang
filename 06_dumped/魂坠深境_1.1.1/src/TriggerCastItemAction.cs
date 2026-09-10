public class TriggerCastItemAction : ExecuteAction
{
	private readonly Item item;

	private Item source;

	private Item target;

	private readonly TriggerTargetSpell.TargetingType targetingType;

	private readonly bool autoTargetForPlayer;

	private readonly bool useSourceAsTriggerAnchor;

	private readonly bool suppressRemoveAfterCast;

	private readonly bool triggerFromProtectedExile;

	private readonly EnchantDefGenerator negativeEnchantReplacementGenerator;

	private readonly bool suppressChangeEnchantAndSpawnItemEffects;

	public TriggerCastItemAction(Item item, Item source, TriggerTargetSpell.TargetingType targetingType, bool autoTargetForPlayer = false, bool useSourceAsTriggerAnchor = false, bool suppressRemoveAfterCast = false, bool triggerFromProtectedExile = false, EnchantDefGenerator negativeEnchantReplacementGenerator = null)
		: this(item, source, targetingType, autoTargetForPlayer, useSourceAsTriggerAnchor, suppressRemoveAfterCast, triggerFromProtectedExile, negativeEnchantReplacementGenerator, suppressChangeEnchantAndSpawnItemEffects: false)
	{
	}

	public TriggerCastItemAction(Item item, Item source, TriggerTargetSpell.TargetingType targetingType, bool autoTargetForPlayer, bool useSourceAsTriggerAnchor, bool suppressRemoveAfterCast, bool triggerFromProtectedExile, EnchantDefGenerator negativeEnchantReplacementGenerator, bool suppressChangeEnchantAndSpawnItemEffects)
	{
		this.item = item;
		this.source = source;
		this.targetingType = targetingType;
		this.autoTargetForPlayer = autoTargetForPlayer;
		this.useSourceAsTriggerAnchor = useSourceAsTriggerAnchor;
		this.suppressRemoveAfterCast = suppressRemoveAfterCast;
		this.triggerFromProtectedExile = triggerFromProtectedExile;
		this.negativeEnchantReplacementGenerator = negativeEnchantReplacementGenerator;
		this.suppressChangeEnchantAndSpawnItemEffects = suppressChangeEnchantAndSpawnItemEffects;
	}

	protected override void Update()
	{
		Item obj = this.item;
		if (obj == null || !obj.HasSpell)
		{
			return;
		}
		ItemSpell currentSpell = this.item.CurrentSpell;
		ItemSpell triggerSpell = this.item.BaseSpell;
		if (triggerSpell == null)
		{
			return;
		}
		ItemRequirement requirement = triggerSpell.requirement;
		if (requirement != null && requirement.NeedTarget)
		{
			BattleFieldTransform ownerBft = this.item.OwnerBft;
			if (ownerBft != null && ownerBft.isPlayer && !autoTargetForPlayer)
			{
				PlayerControlInfo.EnqueueSpellSelectItemAsTargetAction(this.item, triggerSpell, delegate(Item t)
				{
					target = t;
				}, gameState);
			}
			else
			{
				Item item = null;
				switch (targetingType)
				{
				case TriggerTargetSpell.TargetingType.Default:
					item = triggerSpell.requirement.GetDefaultTargetItem(this.item, gameState);
					break;
				case TriggerTargetSpell.TargetingType.Random:
					item = triggerSpell.requirement.GetRandomTargetItem(this.item, gameState);
					break;
				}
				target = item;
			}
		}
		BattleFieldTransform ownerBft2 = this.item.OwnerBft;
		if (ownerBft2 != null && ownerBft2.isEnemy)
		{
			gameState.actionManager.EnqueueAction(new PlayPerformanceAction(this.item, this.item.OwnerBft, EnemyAnimationType.none, waitForCompletion: true));
		}
		EnqueueAction(new ActionAction(delegate
		{
			GameStateInfo gameStateInfo = gameState;
			Item spellItem = this.item;
			Item targetItem = target;
			ItemSpell forcedSpell = ((triggerSpell == currentSpell) ? null : triggerSpell);
			Item triggerAnchorItem = (useSourceAsTriggerAnchor ? source : null);
			bool num = suppressRemoveAfterCast;
			bool num2 = triggerFromProtectedExile;
			EnchantDefGenerator enchantDefGenerator = negativeEnchantReplacementGenerator;
			bool num3 = suppressChangeEnchantAndSpawnItemEffects;
			BattleFieldTransform ownerBft3 = this.item.OwnerBft;
			RealSpellCastInfo.TriggerSpellCast(gameStateInfo, spellItem, targetItem, SpellCastInfo.CasterType.Effect, triggerCast: true, null, forcedSpell, triggerAnchorItem, num, consumeCostWhenTriggered: false, num2, null, enchantDefGenerator, num3, ownerBft3 != null && ownerBft3.isPlayer && !autoTargetForPlayer);
		}));
	}
}
