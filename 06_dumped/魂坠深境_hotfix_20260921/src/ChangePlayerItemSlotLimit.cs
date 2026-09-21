public class ChangePlayerItemSlotLimit : Effect
{
	public int delta = -1;

	public bool temp;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(delta)
		};
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer itemContainer = eve?.gameState?.playerState?.bft?.tableContainer;
		if (itemContainer == null || delta == 0)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new ChangeSlotCountAction(itemContainer, delta, temp));
		return NodeCastResult.Success;
	}
}
