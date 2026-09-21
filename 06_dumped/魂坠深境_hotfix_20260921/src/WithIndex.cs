using System.Text.RegularExpressions;

public class WithIndex : KeywordDef.DisplayNode
{
	private const string indexMatch = "index";

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string value = group["index"].Value;
		return AppendStrWithKeywordRefFormat(def, def.keywordName + value, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}

	public override KeywordInfo GenerateKeywordInfo(KeywordDef def, GroupCollection group, GameEvent eve)
	{
		KeywordInfo keywordInfo = base.GenerateKeywordInfo(def, group, eve);
		keywordInfo.value = int.Parse(group["index"].Value);
		return keywordInfo;
	}

	public override string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
	{
		return base.GetKeywordDescription(applicationState, info).FormatWith(info.value);
	}
}
