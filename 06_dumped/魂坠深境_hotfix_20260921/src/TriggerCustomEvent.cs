public class TriggerCustomEvent : Effect
{
	public string eventName;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new TriggerCustomEventAction(eventName, eve.Caster));
		return NodeCastResult.Success;
	}
}
