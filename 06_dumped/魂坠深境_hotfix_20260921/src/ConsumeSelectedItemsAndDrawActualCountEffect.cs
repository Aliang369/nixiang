using System.Collections.Generic;

public class ConsumeSelectedItemsAndDrawActualCountEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public TriggerChapter triggerChapterEffect;

	public DrawFromPile drawEffect;

	public bool consumeRightToLeft;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		if (consumeRightToLeft)
		{
			list.Reverse();
		}
		int discardedCount = 0;
		foreach (Item item in list)
		{
			if (item != null)
			{
				GameEvent gameEvent = eve.ShallowCopy();
				gameEvent.NodeTempItem = item;
				triggerChapterEffect?.Cast(gameEvent);
				Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect, null, triggerChapterEffect?.hitEffect, delegate
				{
					discardedCount++;
				}));
			}
		}
		Effect.EnqueueAction(eve, new ActionAction(delegate
		{
			BattleFieldTransform eventBFT = eve.GetEventBFT(drawEffect?.targetBFT ?? GameEvent.EventBFTType.Owner);
			if (eventBFT != null)
			{
				if (eventBFT.isPlayer)
				{
					drawEffect?.transformEffect?.Play(eve, eve.CasterPivot.GetPivot(eve.gameState), eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.DrawPileButton));
				}
				else if (eventBFT.isEnemy)
				{
					drawEffect?.transformEffect?.Play(eve, eve.CasterPivot.GetPivot(eve.gameState), eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.EnemyPivot));
				}
				drawEffect?.transformEffect3D?.Play(eve, eve.GetEventBFT(GameEvent.EventBFTType.Owner), eventBFT);
				for (int i = 0; i < discardedCount; i++)
				{
					int valueOrDefault = (drawEffect?.insertIndexGen?.GenerateValue(eve)).GetValueOrDefault();
					Effect.EnqueueAction(eve, new DrawFromPileAction(eventBFT, eve.CasterItem, valueOrDefault));
				}
				if (eventBFT.isPlayer)
				{
					drawEffect?.hitEffect?.Play(eve.gameState, eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.DrawPileButton));
				}
				else if (eventBFT.isEnemy)
				{
					drawEffect?.hitEffect?.Play(eve.gameState, eve.gameState.pivotManager.GetPivot(PivotManager.PivotType.EnemyPivot));
				}
				drawEffect?.hitEffect3D?.Play(eve, eventBFT);
			}
		}));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, triggerChapterEffect, drawEffect };
	}
}
