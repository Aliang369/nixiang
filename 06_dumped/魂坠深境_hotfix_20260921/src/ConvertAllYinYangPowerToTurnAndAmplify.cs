using System.Collections.Generic;
using UnityEngine;

public class ConvertAllYinYangPowerToTurnAndAmplify : Effect
{
	[SerializeField]
	public ValueGenerator divisor;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int valueOrDefault = (divisor?.GenerateValue(eve)).GetValueOrDefault();
		if (valueOrDefault <= 0 || eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new TrigramsManager.ConvertAllPowerToTurnAndAmplifyAction(valueOrDefault, eve.Caster));
		Effect.EnqueueAction(eve, new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { divisor };
	}
}
