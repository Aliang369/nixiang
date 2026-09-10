using System.Text.RegularExpressions;
using UnityEngine;

public class NoLinkColorDisplay : KeywordDef.DisplayNode
{
	public Color color = new Color(0.74f, 0.72f, 0.56f);

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		return def.keywordName.ColorText(color).BoldText().NoBreak();
	}
}
