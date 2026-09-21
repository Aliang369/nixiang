using System.Collections.Generic;

public class RemoveFromDeck : Effect
{
	public bool removeRealItem = true;

	public bool removeContainerItem;

	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (Item item in targetSelector.GetTargetItems(eve))
		{
			if (item == null || item.BlocksExternalExile(eve.CasterItem, ExileItemAction.ExileType.Permanent))
			{
				continue;
			}
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			if (removeRealItem)
			{
				Effect.EnqueueAction(eve, new ExileItemAction(item, eve.CasterItem, null, ExileItemAction.ExileType.Permanent));
			}
			if (removeContainerItem)
			{
				BattleFieldTransform ownerBft = item.OwnerBft;
				if (ownerBft != null)
				{
					ownerBft.tableContainer.GetAllItems().ForEach(delegate(Item i)
					{
						RemoveContainerItem(i, item);
					});
					ownerBft.drawPileItemContainer.GetAllItems().ForEach(delegate(Item i)
					{
						RemoveContainerItem(i, item);
					});
					ownerBft.wastePileItemContainer.GetAllItems().ForEach(delegate(Item i)
					{
						RemoveContainerItem(i, item);
					});
					ownerBft.companionContainer.GetAllItems().ForEach(delegate(Item i)
					{
						RemoveContainerItem(i, item);
					});
				}
			}
			Effect.EnqueueAction(eve, new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_permanentExile, item));
			if (item.OwnerBft != null && item.FromDeck && item.sourceDeckItem != null && item.sourceDeckItem.GetDef() == item.GetDef())
			{
				Effect.EnqueueAction(eve, new ExileItemAction(item.sourceDeckItem, eve.CasterItem, null, ExileItemAction.ExileType.Permanent));
			}
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
		void RemoveContainerItem(Item i, Item deckItem)
		{
			if (i.FromDeck && i.sourceDeckItem == deckItem)
			{
				Effect.EnqueueAction(eve, new ExileItemAction(i.sourceDeckItem, eve.CasterItem, null, ExileItemAction.ExileType.Permanent));
			}
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
