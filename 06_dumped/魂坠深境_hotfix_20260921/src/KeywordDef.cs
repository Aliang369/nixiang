using System.Collections.Generic;
using System.Text.RegularExpressions;
using OdinEx;
using UnityEngine;

public class KeywordDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public abstract class DisplayNode
	{
		public abstract string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generatorIndex);

		public virtual KeywordInfo GenerateKeywordInfo(KeywordDef def, GroupCollection group, GameEvent eve)
		{
			return new KeywordInfo(def, eve);
		}

		public virtual string GetKeywordDescription(ApplicationStateInfo applicationState, KeywordInfo info)
		{
			TextDataGenerator generator;
			return TextDataPreset.FormatKeywordDescription(applicationState, info.keywordDef, out generator);
		}

		protected string AppendStrWithKeywordRefFormat(KeywordDef def, string str, Color color, int generatorIndex)
		{
			return str.BoldText().LinkReferenceObject(generatorIndex).ViewText()
				.ColorText(color)
				.NoBreak();
		}

		protected string AppendStrWithKeywordRefFormatAndIgnoreLinkStr(KeywordDef def, string str, Color color, int generatorIndex)
		{
			return "".LinkReferenceObject(generatorIndex).ColorText(color).NoBreak();
		}
	}

	[CTSOAssetName]
	public string textId;

	[LocalizeText("关键字名字（应当保证大写开头）")]
	public string keywordName;

	[LocalizeText("关键字描述")]
	[TextAreaProp(1, 3, false, 0)]
	public string keyworkdDescription;

	public string customeRegex;

	private Regex _cachedRegex;

	public DisplayNode displayNode;

	public override string CTAssetIDPrepend => "KW";

	public Regex cachedRegex
	{
		get
		{
			if (_cachedRegex == null)
			{
				string pattern = "(?<!.)(?<id>" + textId + ")(?!.)";
				if (!string.IsNullOrEmpty(customeRegex))
				{
					pattern = "(?<!.)" + customeRegex + "(?!.)";
				}
				_cachedRegex = new Regex(pattern, RegexOptions.Compiled);
			}
			return _cachedRegex;
		}
	}

	private void ResetCachedRegex()
	{
		_cachedRegex = null;
	}

	NodeDisplayInfo ITextDisplay.GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		return new List<IEffectNode>();
	}

	public string GetKeywordStr(GroupCollection group, GameEvent eve, int generatorIndex)
	{
		return displayNode.GenerateKeywordStr(this, group, eve, generatorIndex);
	}
}
