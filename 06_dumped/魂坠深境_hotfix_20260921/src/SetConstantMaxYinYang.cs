using System.Collections.Generic;
using UnityEngine;

public class SetConstantMaxYinYang : Effect
{
	[SerializeField]
	private ValueGenerator yangValueGenerator;

	[SerializeField]
	private ValueGenerator yinValueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		int? num = yinValueGenerator.GenerateValue(eve);
		int? num2 = yangValueGenerator.GenerateValue(eve);
		if (num2.HasValue)
		{
			eve.gameState.trigramsManager.constantMaxYangPower = num2.Value;
		}
		if (num.HasValue)
		{
			eve.gameState.trigramsManager.constantMaxYinPower = num.Value;
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { yinValueGenerator, yangValueGenerator };
	}
}
