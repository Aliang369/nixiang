using System.Text.RegularExpressions;

public class GetConsumeRequirement : KeywordDef.DisplayNode
{
	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		return AppendStrWithKeywordRefFormat(def, string.Format("{0}{1}", def.keywordName, group["str"]), ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}
}
