using System.Collections.Generic;

public class AddChapter : Effect
{
	public ItemTargetSelector targetSelector;

	public ValueGenerator addChapterGenerator;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> source = targetSelector?.GetTargetItems(eve);
		int num = addChapterGenerator?.GenerateValue(eve) ?? 1;
		foreach (Item item in source.OrEmptyIfNull())
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerAddChapterResolve(item, eve.Caster, GameEvent.TriggerType.Real);
			if (!resolveInfo.reject)
			{
				Effect.EnqueueAction(eve, new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_addChapter, item));
				Effect.EnqueueAction(eve, new ChangeCounterAction(resolveInfo.targetDelta + num, item, eve.Caster, withPopupText: true));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, addChapterGenerator };
	}
}
