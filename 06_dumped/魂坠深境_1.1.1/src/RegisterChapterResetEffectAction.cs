public class RegisterChapterResetEffectAction : ExecuteAction
{
	private readonly Item item;

	private readonly int maxCounterDelta;

	public RegisterChapterResetEffectAction(Item item, int maxCounterDelta)
	{
		this.item = item;
		this.maxCounterDelta = maxCounterDelta;
	}

	protected override void Update()
	{
		if (item != null)
		{
			item.pendingChapterResetMaxCounterDelta += maxCounterDelta;
		}
	}
}
