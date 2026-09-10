using System.Collections.Generic;
using System.Linq;

public sealed class DancerCharmPlayedCardEffect : Effect
{
	public NewItemGenerator itemGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = eve?.spellInfo?.spellItem;
		BattleFieldTransform battleFieldTransform = item?.OwnerBft ?? eve?.CasterSkill?.OwnerBft;
		if (eve?.gameState == null || item == null || battleFieldTransform == null)
		{
			return NodeCastResult.Failure;
		}
		ItemContainer owner = item.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.Table)
		{
			Effect.EnqueueAction(eve, new ExileItemAction(item, ((object)eve.CasterSkill) ?? ((object)eve.CasterItem)));
		}
		ItemContainer drawPileItemContainer = battleFieldTransform.drawPileItemContainer;
		if (drawPileItemContainer != null)
		{
			Item item2 = itemGenerator?.GenerateNewItems(eve)?.FirstOrDefault((Item item3) => item3 != null);
			if (item2 == null && eve.gameState.generationManager != null)
			{
				item2 = eve.gameState.generationManager.GenerateCharacterPoolItems(1, eve.gameState.difficultyState.CurrentStage, QualityLevel.None, drawPileItemContainer)?.FirstOrDefault((Item item3) => item3 != null);
			}
			if (item2 != null)
			{
				Effect.EnqueueAction(eve, new SpawnItemAction(item2.Copy(), drawPileItemContainer, withAnim: false, eve.CasterItem, null, suppressAfterTriggerChapterEffectsUntilQueueClean: false, eve.spellInfo));
			}
			Effect.EnqueueAction(eve, new DrawFromPileAction(battleFieldTransform, eve.CasterItem));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> value = itemGenerator?.GenerateNewItems(eve);
		return new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>(value),
			times = new NodeDisplayInfo.Element<int?>(1)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemGenerator };
	}
}
