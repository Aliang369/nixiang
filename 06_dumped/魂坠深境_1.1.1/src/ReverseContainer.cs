using System.Collections.Generic;

public class ReverseContainer : Effect
{
	public GameEvent.EventBFTType targetBFT;

	public GameEvent.ContainerType targetContainerType;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer ownerContainer = eve.GetOwnerContainer(targetBFT, targetContainerType);
		Effect.EnqueueAction(eve, new ReverseContainerAction(ownerContainer));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
