using System.Collections.Generic;

public class SwapItemRunes : Effect
{
	public ItemTargetSelector selectorA;

	public ItemTargetSelector selectorB;

	private bool sourceDeckChange;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = selectorA?.GetSingleItem(eve);
		Item item2 = selectorB?.GetSingleItem(eve);
		if (item == null || item2 == null)
		{
			return NodeCastResult.Failure;
		}
		List<Rune> list = new List<Rune>(item.Runes);
		List<Rune> list2 = new List<Rune>(item2.Runes);
		transformVFXEffect?.Play(eve, eve.CasterPivot, item);
		transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
		foreach (Rune item3 in list)
		{
			Effect.EnqueueAction(eve, new ChangeRuneAction(item, item3.def, -999, eve.CasterItem, sourceDeckChange));
		}
		foreach (Rune item4 in list2)
		{
			Effect.EnqueueAction(eve, new ChangeRuneAction(item2, item4.def, -999, eve.CasterItem, sourceDeckChange));
		}
		foreach (Rune item5 in list)
		{
			Effect.EnqueueAction(eve, new ChangeRuneAction(item2, item5, eve.CasterItem, sourceDeckChange));
		}
		foreach (Rune item6 in list2)
		{
			Effect.EnqueueAction(eve, new ChangeRuneAction(item, item6, eve.CasterItem, sourceDeckChange));
		}
		hitEffect?.Play(eve, item);
		hitEffect?.Play(eve, item2);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { selectorA, selectorB };
	}
}
