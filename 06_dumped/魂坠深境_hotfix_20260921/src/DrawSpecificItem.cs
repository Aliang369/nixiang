using System.Collections.Generic;

public class DrawSpecificItem : Effect
{
	public ItemTargetSelector drawPileItems;

	public GameEvent.EventBFTType targetBFT;

	public HitNFXConfig hitEffect;

	public HitNFX3DConfig hitEffect3D;

	public TransformNFXConfig transformVFXEffect;

	public TransformNFX3DConfig transformEffect3D;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer ownerContainer = eve.GetOwnerContainer(targetBFT, GameEvent.ContainerType.DrawPileContainer);
		BattleFieldTransform eventBFT = eve.GetEventBFT(targetBFT);
		List<Item> list = drawPileItems?.GetTargetItems(eve);
		if (eventBFT == null || ownerContainer == null || list.IsNullOrEmpty())
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			if (item.owner == ownerContainer)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
				Effect.EnqueueAction(eve, new DrawSpecificItemFromPileAction(eventBFT, item, eve.CasterItem));
				hitEffect?.Play(eve, item);
				hitEffect3D?.Play(eve, eventBFT);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawPileItems };
	}
}
