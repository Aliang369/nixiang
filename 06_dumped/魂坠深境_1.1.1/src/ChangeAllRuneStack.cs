using System.Collections.Generic;

public class ChangeAllRuneStack : Effect
{
	public ValueGenerator deltaValueGenerator;

	public ItemTargetSelector targetSelector;

	public bool sourceDeckChange;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		int delta = ((deltaValueGenerator == null) ? 1 : deltaValueGenerator.GenerateValue(eve).Value);
		foreach (Item target in list)
		{
			Effect.EnqueueAction(eve, new ActionAction(delegate
			{
				foreach (Rune rune in target.Runes)
				{
					transformVFXEffect?.Play(eve, eve.CasterPivot, target);
					Effect.EnqueueAction(eve, new ChangeRuneAction(target, rune.def, delta, eve.CasterItem, sourceDeckChange));
					hitEffect?.Play(eve, target);
				}
			}));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaValueGenerator, targetSelector };
	}
}
