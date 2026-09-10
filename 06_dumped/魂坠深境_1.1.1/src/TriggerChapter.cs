using System.Collections.Generic;

public class TriggerChapter : Effect
{
	public ItemTargetSelector targetSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (Item item in (targetSelector?.GetTargetItems(eve)).OrEmptyIfNull())
		{
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new TriggerChapterAction(item));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
