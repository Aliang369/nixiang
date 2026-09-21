using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChapterMark : KeywordDef.DisplayNode
{
	private const string CounterMatch = "counter";

	private const string desMatch = "des";

	public Color activeColor;

	public Color inactiveColor;

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		if (eve.CasterItem == null)
		{
			return "";
		}
		int chapter = (int)eve.CasterItem.counter + 1;
		int result = 0;
		List<int> list = new List<int>();
		foreach (object capture in group["counter"].Captures)
		{
			if (int.TryParse(capture.ToString(), out result))
			{
				list.Add(result);
			}
		}
		string str = list.ConvertAll((int t) => (def.keywordName + t).ColorText((t == chapter) ? activeColor : inactiveColor) ?? "").Aggregate((string a, string b) => a + "," + b);
		string value = group["des"].Value;
		Color color = (list.Contains(chapter) ? activeColor : inactiveColor);
		return (AppendStrWithKeywordRefFormat(def, str, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex) + "：" + value).ColorText(color);
	}
}
