using System.Collections.Generic;
using UnityEngine;

public class ValueDisplay : Effect
{
	[SerializeField]
	private ValueGenerator valueGenerator;

	public int? GenerateValue(GameEvent eve)
	{
		return valueGenerator?.GenerateValue(eve);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(GenerateValue(eve))
		};
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
