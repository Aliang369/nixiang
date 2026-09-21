public class FirepowerSourcePowerTargetAction : ExecuteAction
{
	private readonly FirepowerSourcePowerCastPlan castPlan;

	public FirepowerSourcePowerTargetAction(FirepowerSourcePowerCastPlan castPlan)
	{
		this.castPlan = castPlan;
	}

	protected override void Update()
	{
		if (!castPlan.CanResolveSwap())
		{
			castPlan.Cancel();
		}
		else
		{
			if (!castPlan.CanPrepareTrigger() || !castPlan.costPrepared)
			{
				return;
			}
			DisplaySpellCastInfo castInfo = castPlan.CreateDisplayCastInfo(gameState);
			if (!castPlan.ApplyCosts(castInfo, gameState))
			{
				castPlan.SkipTrigger();
				return;
			}
			if (!castInfo.NeedSpellTargetItem)
			{
				castPlan.targetPrepared = true;
				return;
			}
			PlayerControlInfo.EnqueueSpellSelectItemAsTargetAction(castPlan.targetItem, castPlan.targetSpell, delegate(Item target)
			{
				castPlan.spellTargetItem = target;
				castPlan.targetPrepared = target != null;
				if (!castPlan.targetPrepared)
				{
					castPlan.SkipTrigger();
				}
			}, gameState, (Item target) => target != castPlan.targetItem && castInfo.CheckItemAsTargetAble(target), castPlan.SkipTrigger);
		}
	}
}
