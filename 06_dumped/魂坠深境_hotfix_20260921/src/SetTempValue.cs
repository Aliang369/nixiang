using System;
using System.Collections.Generic;
using UnityEngine;

public class SetTempValue : SetTempKey
{
	protected enum Operator
	{
		Set,
		Add,
		Sub
	}

	[SerializeField]
	protected Operator @operator;

	public ValueGenerator vg;

	public ValueGenerator defaultValueGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int? num = vg?.GenerateValue(eve);
		if (!num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		bool flag = true;
		if (!((type != TempValue.Type.GameGlobal) ? SetNormalTempValue(eve, num.Value) : SetGlobalTempValue(eve, num.Value)))
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	private bool SetNormalTempValue(GameEvent eve, int value)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		dictionary.TryAdd(key, 0);
		int? defaultValue = defaultValueGenerator?.GenerateValue(eve);
		int? tempValueInDic = TempValueGenerator.GetTempValueInDic(eve, key, dictionary, defaultValue);
		switch (@operator)
		{
		case Operator.Set:
			dictionary[key] = value;
			break;
		case Operator.Add:
			if (!tempValueInDic.HasValue)
			{
				TraceLog.Error("tempvalues的key值" + key + "不存在");
			}
			dictionary[key] = tempValueInDic.GetValueOrDefault() + value;
			break;
		case Operator.Sub:
			if (!tempValueInDic.HasValue)
			{
				TraceLog.Error("tempvalues的key值" + key + "不存在");
			}
			dictionary[key] = tempValueInDic.GetValueOrDefault() - value;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return true;
	}

	private bool SetGlobalTempValue(GameEvent eve, int value)
	{
		switch (@operator)
		{
		case Operator.Set:
			eve.gameState.globalVariableStateInfo.SetVariableValue(key, value);
			break;
		case Operator.Add:
			eve.gameState.globalVariableStateInfo.SetVariableValue(key, eve.gameState.globalVariableStateInfo.GetVariableValue(key) + value);
			break;
		case Operator.Sub:
			eve.gameState.globalVariableStateInfo.SetVariableValue(key, eve.gameState.globalVariableStateInfo.GetVariableValue(key) - value);
			break;
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { vg };
	}
}
