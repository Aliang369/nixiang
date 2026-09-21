using System.Collections.Generic;
using UnityEngine;

public class SetTempBool : SetTempKey
{
	[SerializeField]
	private CheckNode checkNode;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		bool value = checkNode.Check(eve).Value;
		bool flag = true;
		if (!((type != TempValue.Type.GameGlobal) ? SetNormalTempValue(eve, value) : SetGlobalTempValue(eve, value)))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempValue(GameEvent eve, bool value)
	{
		TempValue.GetDictionary(eve, type)[key] = value;
		return true;
	}

	private bool SetGlobalTempValue(GameEvent eve, bool value)
	{
		eve.gameState.globalVariableStateInfo.SetVariableValue(key, value ? 1 : 0);
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { checkNode };
	}
}
