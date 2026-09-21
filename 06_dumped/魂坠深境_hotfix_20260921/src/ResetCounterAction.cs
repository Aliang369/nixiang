public class ResetCounterAction : ExecuteAction
{
	public Item sourceItem;

	public Item targetItem;

	public ResetCounterAction(Item sourceItem, Item targetItem)
	{
		this.sourceItem = sourceItem;
		this.targetItem = targetItem;
	}

	protected override void Update()
	{
		if (targetItem != null)
		{
			ProtectedInt counter = targetItem.counter;
			ProtectedInt maxCounter = targetItem.maxCounter;
			bool usesDefenceCounterLimit = targetItem.UsesDefenceCounterLimit;
			int num = (usesDefenceCounterLimit ? CombatValueLimit.Clamp((int)targetItem.resetInitCounter) : ((int)targetItem.resetInitCounter));
			bool flag = targetItem.HasPendingChapterResetEffect && (int)counter == (int)maxCounter && num < (int)counter;
			targetItem.counter = num;
			int num2 = (usesDefenceCounterLimit ? CombatValueLimit.Clamp((int)targetItem.resetInitMaxCounter) : ((int)targetItem.resetInitMaxCounter));
			targetItem.maxCounter = num2;
			gameState.gameEventManager.TriggerCounterChange(targetItem, sourceItem, num - (int)counter, num);
			if (flag)
			{
				EnqueueAction(new ResolveChapterResetEffectAction(targetItem));
			}
		}
	}
}
