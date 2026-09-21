using System.Collections.Generic;

public sealed class EvilSkinBranchEffect : Effect
{
	public ItemSkinDef evilSkin;

	public Effect normalEffect;

	public Effect evilEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		return ((eve.CasterItem?.skin == evilSkin) ? evilEffect : normalEffect)?.Cast(eve) ?? NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { normalEffect, evilEffect };
	}
}
