using System.Text.RegularExpressions;

public class GetCoolDown : KeywordDef.DisplayNode
{
	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		int valueOrDefault = (eve.CasterItem?.CurrentSpell?.GetCoolDown(eve.CasterItem)).GetValueOrDefault();
		string str = ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_tilde_arg1.FormatWith(def.keywordName, valueOrDefault);
		return AppendStrWithKeywordRefFormat(def, str, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}

	public override KeywordInfo GenerateKeywordInfo(KeywordDef def, GroupCollection group, GameEvent eve)
	{
		KeywordInfo keywordInfo = base.GenerateKeywordInfo(def, group, eve);
		keywordInfo.value = (eve.CasterItem?.CurrentSpell?.GetCoolDown(eve.CasterItem)).GetValueOrDefault();
		return keywordInfo;
	}

	public override string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
	{
		return base.GetKeywordDescription(applicationState, info).FormatWith(info.value);
	}
}
