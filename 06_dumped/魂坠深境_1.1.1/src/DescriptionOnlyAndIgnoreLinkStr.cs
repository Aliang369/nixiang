using System.Text.RegularExpressions;

public class DescriptionOnlyAndIgnoreLinkStr : KeywordDef.DisplayNode
{
	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		return AppendStrWithKeywordRefFormatAndIgnoreLinkStr(def, def.keywordName, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}
}
