using UnityEngine;

public class ResolveChapterResetEffectAction : ExecuteAction
{
	private readonly Item item;

	public ResolveChapterResetEffectAction(Item item)
	{
		this.item = item;
	}

	protected override void Update()
	{
		if (item != null)
		{
			int pendingChapterResetMaxCounterDelta = item.pendingChapterResetMaxCounterDelta;
			item.pendingChapterResetMaxCounterDelta = 0;
			if (pendingChapterResetMaxCounterDelta != 0)
			{
				int counter = Mathf.Max(1, (int)item.maxCounter + pendingChapterResetMaxCounterDelta);
				item.resetInitMaxCounter = Mathf.Max(1, item.resetInitMaxCounter + pendingChapterResetMaxCounterDelta);
				EnqueueAction(new SetMaxCounterAction(counter, item, item));
			}
		}
	}
}
