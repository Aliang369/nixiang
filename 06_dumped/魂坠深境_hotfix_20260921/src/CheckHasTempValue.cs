using System.Collections.Generic;
using UnityEngine;

public class CheckHasTempValue : CheckNode
{
	[SerializeField]
	private TempValue.Type type;

	[SerializeField]
	private string key;

	private List<string> defaultKeys => TempValue.defaultKeys;

	public override bool? Check(GameEvent eve)
	{
		if (key == null)
		{
			return null;
		}
		if (type == TempValue.Type.GameGlobal)
		{
			return null;
		}
		return CheckNormal(eve);
	}

	private bool? CheckNormal(GameEvent eve)
	{
		return TempValue.GetDictionary(eve, type)?.ContainsKey(key);
	}
}
