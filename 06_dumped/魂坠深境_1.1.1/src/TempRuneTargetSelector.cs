using System.Collections.Generic;
using UnityEngine;

public class TempRuneTargetSelector : RuneGenerator
{
	[SerializeField]
	private TempValue.Type type;

	[SerializeField]
	private string key;

	private List<Rune> GeneratorNormalRunes(GameEvent eve)
	{
		Dictionary<string, object> dictionary = TempValue.GetDictionary(eve, type);
		if (dictionary == null)
		{
			return null;
		}
		if (dictionary.TryGetValue(key) is List<Rune> result)
		{
			return result;
		}
		return null;
	}

	public override List<Rune> GenerateRunes(GameEvent eve)
	{
		if (type == TempValue.Type.GameGlobal || type == TempValue.Type.ApplicationGlobal)
		{
			return null;
		}
		return GeneratorNormalRunes(eve);
	}
}
