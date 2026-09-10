using System.Text.RegularExpressions;
using UnityEngine;

public class TreasureKeyword : KeywordDef.DisplayNode
{
	public Color coinColor;

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		if (eve.CasterItem == null)
		{
			return "";
		}
		return AppendStrWithKeywordRefFormat(def, def.keywordName + eve.CasterItem.treasureValue, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}

	public override KeywordInfo GenerateKeywordInfo(KeywordDef def, GroupCollection group, GameEvent eve)
	{
		KeywordInfo keywordInfo = base.GenerateKeywordInfo(def, group, eve);
		keywordInfo.value = eve.CasterItem.treasureValue;
		return keywordInfo;
	}

	public override string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
	{
		return base.GetKeywordDescription(applicationState, info).FormatWith(info.value.ToString().ColorText(coinColor));
	}
}
