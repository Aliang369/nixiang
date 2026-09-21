using System.Collections.Generic;
using UnityEngine;

public class ChangeCounter : Effect
{
	[SerializeField]
	public ItemTargetSelector target;

	[SerializeField]
	private ValueGenerator deltaGenerator;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	public bool consumeDefenceWhenDepleted;

	public bool showChangedItems;

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
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			Effect.EnqueueAction(eve, new ChangeCounterAction(num, item, eve.CasterItem, withPopupText: true, consumeDefenceWhenDepleted));
			hitEffect?.Play(eve, item);
		}
		if (showChangedItems && num != 0)
		{
			Effect.EnqueueAction(eve, new NotifyItemModificationsAction(targetItems));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target, deltaGenerator };
	}
}
