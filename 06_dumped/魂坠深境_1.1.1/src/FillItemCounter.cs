public class FillItemCounter : Effect
{
	public GameEvent.EventItemType targetItem;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(targetItem);
		if (eventItem == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, eventItem);
		Effect.EnqueueAction(eve, new SetCounterAction(eventItem.maxCounter, eventItem, eve.CasterItem));
		hitEffect?.Play(eve, eventItem);
		return NodeCastResult.Success;
	}
}
