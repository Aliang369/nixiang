using System.Collections.Generic;
using System.Linq;

public class AdvanceChapterAndDrawIfFinalEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public AddChapter advanceEffect;

	public DrawFromPile drawEffect;

	public ValueGenerator upgradedDrawCountGenerator;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item target = targetSelector?.GetTargetItems(eve)?.FirstOrDefault();
		if (target == null || advanceEffect == null)
		{
			return NodeCastResult.Failure;
		}
		GameEvent gameEvent = eve.ShallowCopy();
		gameEvent.NodeTempItem = target;
		bool upgraded = upgradedCheck?.Check(eve) ?? eve.CasterItem?.upgraded ?? false;
		int num = advanceEffect.addChapterGenerator?.GenerateValue(gameEvent) ?? 1;
		GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerAddChapterResolve(target, eve.Caster, GameEvent.TriggerType.Real);
		if (resolveInfo.reject)
		{
			return NodeCastResult.Success;
		}
		advanceEffect.transformVFXEffect?.Play(gameEvent, gameEvent.CasterPivot, target);
		Effect.EnqueueAction(eve, new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_addChapter, target));
		Effect.EnqueueAction(eve, new ChangeCounterAction(resolveInfo.targetDelta + num, target, eve.Caster, withPopupText: true, consumeDefenceWhenDepleted: false, delegate(int before, int actualDelta, int after, int maximum)
		{
			if (actualDelta > 0 && maximum > 0 && before < maximum && after >= maximum)
			{
				GameEvent gameEvent2 = eve.ShallowCopy();
				gameEvent2.NodeTempItem = target;
				if (upgraded && upgradedDrawCountGenerator != null)
				{
					int valueOrDefault = upgradedDrawCountGenerator.GenerateValue(gameEvent2).GetValueOrDefault();
					BattleFieldTransform eventBFT = gameEvent2.GetEventBFT(GameEvent.EventBFTType.Owner);
					for (int i = 0; i < valueOrDefault; i++)
					{
						Effect.EnqueueAction(gameEvent2, new DrawFromPileAction(eventBFT, eve.CasterItem));
					}
				}
				else if (upgraded)
				{
					BattleFieldTransform eventBFT2 = gameEvent2.GetEventBFT(GameEvent.EventBFTType.Owner);
					for (int j = 0; j < 3; j++)
					{
						Effect.EnqueueAction(gameEvent2, new DrawFromPileAction(eventBFT2, eve.CasterItem));
					}
				}
				else
				{
					drawEffect?.Cast(gameEvent2);
				}
			}
		}));
		advanceEffect.hitEffect?.Play(gameEvent, target);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, advanceEffect, drawEffect, upgradedDrawCountGenerator, upgradedCheck };
	}
}
