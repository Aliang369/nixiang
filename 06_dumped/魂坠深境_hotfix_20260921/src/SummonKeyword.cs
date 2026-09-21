using System;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using UnityEngine;

public class SummonKeyword : KeywordDef.DisplayNode
{
	private const string desMatch = "des";

	public Color greyColor = new Color(0.8f, 0.8f, 0.8f, 1f);

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string value = group["des"].Value;
		if (value.IsNullOrWhitespace())
		{
			return AppendStrWithKeywordRefFormat(def, def.keywordName, ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor, generateIndex);
		}
		if (eve.CasterItem?.owner != null && eve.CasterItem.owner.type == ItemContainer.ContainerType.Companion)
		{
			return value;
		}
		return AppendStrWithKeywordRefFormat(def, def.keywordName, ScriptableObjectSingleton<TextDataPreset>.instance.linkItemColor, generateIndex) + Environment.NewLine + value.ConvertTextColorTag((Color color) => color * greyColor).ColorText(greyColor);
	}
}
