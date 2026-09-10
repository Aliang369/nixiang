using System.Collections.Generic;
using UnityEngine;

public class RandomCheck : CheckNode
{
	[SerializeField]
	private ValueGenerator baseThresholdGnerator;

	[SerializeField]
	private ValueGenerator deltaGenerator;

	[SerializeField]
	private bool dynamic;

	[SerializeField]
	private string globalVariableId;

	public override bool? Check(GameEvent eve)
	{
		GameRandomManager gameRandomManager = eve.gameState?.randomManager;
		InGameGlobalVariableStateInfo inGameGlobalVariableStateInfo = eve.gameState?.globalVariableStateInfo;
		if (gameRandomManager == null || inGameGlobalVariableStateInfo == null)
		{
			return null;
		}
		int? num = ((baseThresholdGnerator != null) ? baseThresholdGnerator.GenerateValue(eve) : new int?(0));
		if (!num.HasValue)
		{
			return null;
		}
		int num2 = gameRandomManager.Next(0, 100);
		if (dynamic)
		{
			int num3 = 0;
			if (inGameGlobalVariableStateInfo.WithVariable(globalVariableId))
			{
				num3 = Mathf.Max(num.Value, inGameGlobalVariableStateInfo.GetVariableValue(globalVariableId));
			}
			else
			{
				num3 = num.Value;
				inGameGlobalVariableStateInfo.SetVariableValue(globalVariableId, num3);
			}
			if (num2 < num3)
			{
				inGameGlobalVariableStateInfo.SetVariableValue(globalVariableId, num.Value);
				return true;
			}
			int? num4 = deltaGenerator.GenerateValue(eve);
			if (!num4.HasValue)
			{
				return null;
			}
			inGameGlobalVariableStateInfo.SetVariableValue(globalVariableId, num3 + num4.Value);
			return false;
		}
		if (num2 < num)
		{
			return true;
		}
		return false;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { baseThresholdGnerator };
	}
}
