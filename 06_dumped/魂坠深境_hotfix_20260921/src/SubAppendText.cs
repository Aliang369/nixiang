using System.Text.RegularExpressions;

public class SubAppendText : KeywordDef.DisplayNode
{
	private const string desMatch = "des";

	public bool removeColorTextTagInWord;

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string text = group["des"].Value;
		if (removeColorTextTagInWord)
		{
			text = text.RemoveColorRichTextTag();
		}
		string str = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_tilde_arg1.FormatWith(def.keywordName, text);
		return AppendStrWithKeywordRefFormat(def, str, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}

	public override KeywordInfo GenerateKeywordInfo(KeywordDef def, GroupCollection group, GameEvent eve)
	{
		KeywordInfo keywordInfo = base.GenerateKeywordInfo(def, group, eve);
		keywordInfo.subText = group["des"].Value;
		return keywordInfo;
	}

	public override string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
	{
		return base.GetKeywordDescription(applicationState, info).FormatWith(info.subText);
	}
}
