using System.Text.RegularExpressions;

public class DescriptionOnly : KeywordDef.DisplayNode
{
	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		return AppendStrWithKeywordRefFormat(def, def.keywordName, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}
}
