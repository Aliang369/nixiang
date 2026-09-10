using System.Collections.Generic;
using UnityEngine;

public class TempItemTargetSelector : ItemTargetSelector
{
	[SerializeField]
	private TempValue.Type type;

	[SerializeField]
	private string key;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		if (type == TempValue.Type.GameGlobal || type == TempValue.Type.ApplicationGlobal)
		{
			return null;
		}
		return GeneratorNormalItems(eve);
	}

	private List<Item> GeneratorNormalItems(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue(key) is List<Item> result)
		{
			return result;
		}
		return null;
	}
}
