using System.Collections.Generic;

public class ShuffleContainer : Effect
{
	public GameEvent.EventBFTType targetBFT;

	public GameEvent.ContainerType targetContainerType;

	public TransformNFX3DConfig transformEffect3D;

	public HitNFX3DConfig hitEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer ownerContainer = eve.GetOwnerContainer(targetBFT, targetContainerType);
		transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eve.GetEventBFT(targetBFT));
		Effect.EnqueueAction(eve, new ShuffleContainerAction(ownerContainer));
		hitEffect3D?.Play(eve, eve.GetEventBFT(targetBFT));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
