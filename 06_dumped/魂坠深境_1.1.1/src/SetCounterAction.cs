public class SetCounterAction : ExecuteAction
{
	public int counter;

	public object source;

	public Item targetItem;

	private readonly bool countsAsChapterReset;

	public SetCounterAction(int counter, Item item, object source, bool countsAsChapterReset = false)
	{
		this.counter = counter;
		targetItem = item;
		this.source = source;
		this.countsAsChapterReset = countsAsChapterReset;
	}

	protected override void Update()
	{
		if (targetItem != null)
		{
			ProtectedInt protectedInt = targetItem.counter;
			int num = (targetItem.UsesDefenceCounterLimit ? CombatValueLimit.Clamp(counter) : counter);
			bool num2 = countsAsChapterReset && targetItem.HasPendingChapterResetEffect && (int)protectedInt == (int)targetItem.maxCounter && num < (int)protectedInt;
			targetItem.counter = num;
			gameState.gameEventManager.TriggerCounterChange(targetItem, source, (int)targetItem.counter - (int)protectedInt, num);
			if ((int)protectedInt != (int)targetItem.maxCounter && num == (int)targetItem.maxCounter)
			{
				gameState.gameEventManager.TriggerCounterReachMaxCounter(targetItem, source);
			}
			if (num2)
			{
				EnqueueAction(new ResolveChapterResetEffectAction(targetItem));
			}
		}
	}
}
