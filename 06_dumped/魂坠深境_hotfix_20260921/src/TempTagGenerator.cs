using System;
using System.Collections.Generic;
using UnityEngine;

public class TempTagGenerator : ItemTagGenerator
{
	[SerializeField]
	private TempValue.Type type;

	[SerializeField]
	private string key;

	public override List<ItemTag> GenerateItemTags(GameEvent eve)
	{
		if (type == TempValue.Type.GameGlobal)
		{
			throw new Exception("Global tag is not supported.");
		}
		return GeneratorNormalTag(eve);
	}

	private List<ItemTag> GeneratorNormalTag(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue(key) is List<ItemTag> result)
		{
			return result;
		}
		return null;
	}
}
