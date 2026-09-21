public class MoveSlotTowardAction : ExecuteAction
{
	private readonly int delta;

	private readonly Item pivot;

	private readonly Item source;

	private readonly Item target;

	public MoveSlotTowardAction(Item target, Item pivot, Item source, int delta)
	{
		this.target = target;
		this.pivot = pivot;
		this.source = source;
		this.delta = delta;
	}

	protected override void Update()
	{
		if (target != null && pivot != null && target.owner == pivot.owner && delta != 0 && !target.BlocksExternalMove(source, gameState) && !pivot.IsEternal())
		{
			ItemContainer owner = source.owner;
			int rawIndex = target.owner.GetRawIndex(target);
			int num = pivot.owner.GetRawIndex(pivot) - rawIndex;
			int num2 = ((delta <= 0) ? 1 : (-1));
			owner.MoveItem(deltaPos: (num * delta < 0) ? (num + delta) : (num + delta + num2), item: target, source: source);
		}
	}
}
