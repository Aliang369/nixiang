using System.Text.RegularExpressions;

public class ContentDisplay : KeywordDef.DisplayNode
{
	private const string content = "content";

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string value = group["content"].Value;
		string text = TextDataPreset.FormatEffectObjectDescription(eve.CasterItem, value, eve, out var _);
		return AppendStrWithKeywordRefFormat(def, def.keywordName + text, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}
}
