public class RecycleWastePile : Effect
{
	public GameEvent.EventBFTType target;

	public TransformNFX3DConfig transformEffect3D;

	public HitNFX3DConfig hitEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(target);
		if (eventBFT == null)
		{
			return NodeCastResult.Failure;
		}
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
		Effect.EnqueueAction(eve, new RecycleWastePileAction(eventBFT));
		hitEffect3D?.Play(eve, eventBFT);
		return NodeCastResult.Success;
	}
}
