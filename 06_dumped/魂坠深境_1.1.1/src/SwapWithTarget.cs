public class SwapWithTarget : Effect
{
	public GameEvent.EventItemType target;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, eventItem);
		Effect.EnqueueAction(eve, new SwapSlotItemAction(eve.CasterItem, eventItem, eve.CasterItem));
		hitEffect?.Play(eve, eventItem);
		return NodeCastResult.Success;
	}
}
