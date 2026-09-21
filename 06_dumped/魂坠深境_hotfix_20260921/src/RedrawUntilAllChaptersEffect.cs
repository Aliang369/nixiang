using System;
using System.Collections.Generic;

public class RedrawUntilAllChaptersEffect : Effect
{
	public ItemTagGenerator chapterTagGenerator;

	public ValueGenerator drawCountOffsetGenerator;

	public ValueGenerator maxRoundsGenerator;

	public DealDamage redrawDamageEffect;

	public AddChapter completionEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		ItemTag itemTag = chapterTagGenerator?.GenerateSingleItemTag(eve);
		int? num = drawCountOffsetGenerator?.GenerateValue(eve);
		int? num2 = maxRoundsGenerator?.GenerateValue(eve);
		int? num3 = redrawDamageEffect?.GetBaseDamage(eve);
		if (eventBFT == null || itemTag == null || !num.HasValue || !num2.HasValue || !num3.HasValue || completionEffect == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new RedrawUntilAllChaptersAction(eventBFT, eve.CasterItem, eve.spellInfo, itemTag, num.Value, Math.Max(1, num2.Value), num3.Value, redrawDamageEffect, completionEffect, 0));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { chapterTagGenerator, drawCountOffsetGenerator, maxRoundsGenerator, redrawDamageEffect, completionEffect };
	}
}
