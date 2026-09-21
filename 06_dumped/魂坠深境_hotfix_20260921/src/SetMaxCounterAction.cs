public class SetMaxCounterAction : ExecuteAction
{
	public Item item;

	public object source;

	public int maxCounter;

	public SetMaxCounterAction(int counter, Item item, object source)
	{
		maxCounter = counter;
		this.item = item;
		this.source = source;
	}

	protected override void Update()
	{
		if (item != null)
		{
			int num = item.counter;
			int num2 = item.maxCounter;
			int num3;
			int num4;
			if (item.UsesDefenceCounterLimit)
			{
				num3 = CombatValueLimit.Clamp(maxCounter);
				num4 = CombatValueLimit.Clamp(num);
			}
			else
			{
				num3 = maxCounter;
				num4 = num;
			}
			if (num4 > num3)
			{
				num4 = num3;
			}
			item.maxCounter = num3;
			item.counter = num4;
			if (num != num2 && num4 == num3)
			{
				gameState.gameEventManager.TriggerCounterReachMaxCounter(item, source);
			}
		}
	}
}
