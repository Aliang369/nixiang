using System.Collections.Generic;
using UnityEngine;

public class ConvertYinYangPower : Effect
{
	[SerializeField]
	private TrigramsManager.YinYangType type;

	[SerializeField]
	private ValueGenerator valueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState?.trigramsManager == null)
		{
			return NodeCastResult.Failure;
		}
		int value = valueGenerator.GenerateValue(eve).Value;
		eve.gameState.actionManager.EnqueueAction(new TrigramsManager.ConvertYinYangPowerAction(value, type, eve.Caster));
		Effect.EnqueueAction(eve, new WaitUntilAction(() => !eve.gameState.UIManager.trigramsDisplay.HasAnim()));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator };
	}
}
