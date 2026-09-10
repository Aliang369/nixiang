using System.Text.RegularExpressions;

public class MusicSpiritKeyword : KeywordDef.DisplayNode
{
	public ItemTag tag;

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string text = def.keywordName;
		ItemTagInstance itemTagInstance = eve.CasterItem?.tagInstances.Find((ItemTagInstance t) => t.tag == tag);
		if (itemTagInstance != null)
		{
			int value = GetTagTempValueInt(itemTagInstance, "音灵升级进度").Value;
			int value2 = GetTagTempValueInt(itemTagInstance, "音灵升级最大进度").Value;
			text += ScriptableObjectSingleton<TextDataPreset>.instance.term_arg0_per_arg1.FormatWith(value, value2);
		}
		return AppendStrWithKeywordRefFormat(def, text, ScriptableObjectSingleton<TextDataPreset>.instance.keywordColor, generateIndex);
	}

	public override string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
	{
		string text = base.GetKeywordDescription(applicationState, info);
		ItemTagInstance itemTagInstance = info.displayGameEvent.CasterItem?.tagInstances.Find((ItemTagInstance t) => t.tag == tag);
		if (itemTagInstance != null)
		{
			int value = GetTagTempValueInt(itemTagInstance, "音灵升级进度").Value;
			int value2 = GetTagTempValueInt(itemTagInstance, "音灵升级最大进度").Value;
			text = text.FormatWith(value, value2);
		}
		return text;
	}

	private int? GetTagTempValueInt(ItemTagInstance tag, string key)
	{
		object obj = tag.tempValues[key];
		if (obj is int)
		{
			return (int)obj;
		}
		if (obj is long num)
		{
			return (int)num;
		}
		return null;
	}
}
