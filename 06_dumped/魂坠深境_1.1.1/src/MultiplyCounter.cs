using System.Collections.Generic;
using UnityEngine;

public class MultiplyCounter : Effect
{
	[SerializeField]
	private ItemTargetSelector target;

	[SerializeField]
	private ValueGenerator deltaGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = target.GetTargetItems(eve);
		int num = ((deltaGenerator != null) ? deltaGenerator.GenerateValue(eve).Value : 0);
		if (targetItems == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in targetItems)
		{
			int delta = (item.UsesDefenceCounterLimit ? CombatValueLimit.SaturateToInt((int)item.counter * ((long)num - 1L)) : ((int)item.counter * (num - 1)));
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new ChangeCounterAction(delta, item, eve.CasterItem, withPopupText: true));
			hitEffect?.Play(eve, item);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, deltaGenerator };
	}
}
