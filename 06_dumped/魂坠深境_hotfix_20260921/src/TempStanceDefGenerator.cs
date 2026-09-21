using System.Collections.Generic;
using UnityEngine;

public class TempStanceDefGenerator : StanceDefSelector
{
	[SerializeField]
	private TempValue.Type type;

	private string key;

	private List<StanceDef> GeneratorStanceDefs(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue(key) is List<StanceDef> result)
		{
			return result;
		}
		return null;
	}

	public override List<StanceDef> GenerateStanceDefs(GameEvent eve)
	{
		return GeneratorStanceDefs(eve);
	}

	public override StanceDef GenerateSingleStanceDef(GameEvent eve)
	{
		return GetSingleStanceDef(eve);
	}
}
