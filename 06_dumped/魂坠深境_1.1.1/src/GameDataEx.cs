using System.Collections.Generic;
using UnityEngine;

public static class GameDataEx
{
	public static Color GetQualityBaseColor(this QualityLevel level)
	{
		return ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(level);
	}

	public static List<StanceDef> GetStanceDefsFromTags(this List<ItemTag> tags)
	{
		List<StanceDef> list = new List<StanceDef>();
		Dictionary<ItemTag, StanceDef> stanceTagsBindings = ScriptableObjectSingleton<GameDataManager>.instance.stanceTagsBindings;
		foreach (ItemTag tag in tags)
		{
			if (stanceTagsBindings.TryGetValue(tag, out var value))
			{
				list.Add(value);
			}
		}
		return list;
	}
}
