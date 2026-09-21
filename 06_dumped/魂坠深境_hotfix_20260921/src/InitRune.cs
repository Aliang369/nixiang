using System.Collections.Generic;

public class InitRune : Effect, IOutGameEffectNode, IEffectCastNode, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public RuneDef runeDef;

	public ValueGenerator deltaValueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		int num = deltaValueGenerator?.GenerateValue(eve) ?? 1;
		Effect.EnqueueAction(eve, new ChangeRuneAction(casterItem, runeDef, num, eve.CasterItem));
		Rune rune = casterItem.Runes.Find((Rune e) => e.def == runeDef);
		if (rune == null && num >= 0)
		{
			Rune rune2 = new Rune(runeDef, num);
			casterItem.AddRune(rune2);
		}
		else if (rune != null)
		{
			if (rune.value + num <= 0)
			{
				casterItem.RemoveRune(rune);
			}
			else if (rune.def.stacking)
			{
				casterItem.ChangeRuneValue(rune, rune.value + num);
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaValueGenerator };
	}
}
