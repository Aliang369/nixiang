using System.Collections.Generic;
using UnityEngine;

public class TempValueCheckNode : CheckNode
{
	[SerializeField]
	private TempValue.Type type;

	[SerializeField]
	private string key;

	[SerializeField]
	private BoolCheckNode defaultValue;

	private List<string> defaultKeys => TempValue.defaultKeys;

	public override bool? Check(GameEvent eve)
	{
		if (key.IsNullOrEmpty())
		{
			return null;
		}
		if (type == TempValue.Type.GameGlobal)
		{
			return GlobalCheckNode(eve);
		}
		return NormalCheckNode(eve);
	}

	private bool? NormalCheckNode(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		object obj = dictionary.TryGetValue(key);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (obj == null && defaultValue != null)
		{
			bool? flag = defaultValue.Check(eve);
			if (!flag.HasValue)
			{
				return null;
			}
			dictionary[key] = flag;
			return flag;
		}
		return null;
	}

	private bool? GlobalCheckNode(GameEvent eve)
	{
		if (eve.gameState?.globalVariableStateInfo == null)
		{
			return null;
		}
		return eve.gameState.globalVariableStateInfo.GetVariableValue(key) > 0;
	}
}
