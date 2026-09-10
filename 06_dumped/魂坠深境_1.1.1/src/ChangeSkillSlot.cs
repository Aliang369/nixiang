using System.Collections.Generic;
using UnityEngine;

public class ChangeSkillSlot : Effect
{
	[SerializeField]
	private ChangSkillSlotCountAction.Type type;

	[SerializeField]
	private ValueGenerator count;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new ChangSkillSlotCountAction(count.GetValue(eve) ?? 1, type));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { count };
	}
}
