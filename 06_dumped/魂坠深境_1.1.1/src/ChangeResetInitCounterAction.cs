public class ChangeResetInitCounterAction : ExecuteAction
{
	public int counter;

	public Item targetItem;

	public ChangeResetInitCounterAction(int counter, Item targetItem)
	{
		this.counter = counter;
		this.targetItem = targetItem;
	}

	protected override void Update()
	{
		if (targetItem != null)
		{
			targetItem.resetInitCounter = (targetItem.UsesDefenceCounterLimit ? CombatValueLimit.Clamp(counter) : counter);
		}
	}
}
