using System.Linq;

public class FirepowerSourcePowerAction : ExecuteAction
{
	private readonly bool consumeTargetCostWhenTriggered;

	private readonly EnchantDef sourcePowerEnchant;

	private readonly Item sourceItem;

	private readonly Item targetItem;

	public FirepowerSourcePowerAction(Item sourceItem, Item targetItem, EnchantDef sourcePowerEnchant, bool consumeTargetCostWhenTriggered)
	{
		this.sourceItem = sourceItem;
		this.targetItem = targetItem;
		this.sourcePowerEnchant = sourcePowerEnchant;
		this.consumeTargetCostWhenTriggered = consumeTargetCostWhenTriggered;
	}

	protected override void Update()
	{
		if (CanResolveSourcePower())
		{
			ItemSpell baseSpell = targetItem.BaseSpell;
			if (baseSpell != null)
			{
				FirepowerSourcePowerCastPlan castPlan = new FirepowerSourcePowerCastPlan(sourceItem, targetItem, baseSpell, sourcePowerEnchant, consumeTargetCostWhenTriggered);
				EnqueueAction(new FirepowerSourcePowerCostAction(castPlan));
				EnqueueAction(new FirepowerSourcePowerTargetAction(castPlan));
				EnqueueAction(new FirepowerSourcePowerRequirementAction(castPlan));
				EnqueueAction(new FirepowerSourcePowerResolveAction(castPlan, consumeTargetCostWhenTriggered));
			}
		}
	}

	private bool CanResolveSourcePower()
	{
		if (sourceItem == null || targetItem == null || sourcePowerEnchant == null || sourceItem.owner != targetItem.owner)
		{
			return false;
		}
		return sourceItem.Enchants.Any((Enchant enchant) => enchant.def == sourcePowerEnchant && (int)enchant.value > 0);
	}
}
