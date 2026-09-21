using System.Collections.Generic;
using UnityEngine;

public class ChangeMRuneStack : Effect
{
	[SerializeField]
	private ValueGenerator deltaValueGenerator;

	[SerializeField]
	private bool sourceDeckChange;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Rune casterRune = eve.CasterRune;
		if (casterRune == null)
		{
			return NodeCastResult.Failure;
		}
		int value = deltaValueGenerator.GetValue(eve, 1).Value;
		if (eve.CasterAttachObject is Item target)
		{
			Effect.EnqueueAction(eve, new ChangeRuneAction(target, casterRune.def, value, null, sourceDeckChange));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { deltaValueGenerator };
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int value = deltaValueGenerator.GetValue(eve, 1).Value;
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(value)
		};
	}
}
