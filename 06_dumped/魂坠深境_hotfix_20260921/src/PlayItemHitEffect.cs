using System.Collections.Generic;

public class PlayItemHitEffect : Effect
{
	public ItemTargetSelector itemTargetSelector;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		foreach (Item targetItem in itemTargetSelector.GetTargetItems(eve))
		{
			hitEffect?.Play(eve, targetItem);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector };
	}
}
