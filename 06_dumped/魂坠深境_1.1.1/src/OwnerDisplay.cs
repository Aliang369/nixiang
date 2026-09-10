using System.Text.RegularExpressions;
using UnityEngine;

public class OwnerDisplay : KeywordDef.DisplayNode
{
	public Color color = new Color(0.74f, 0.72f, 0.56f);

	public override string GenerateKeywordStr(KeywordDef def, GroupCollection group, GameEvent eve, int generateIndex)
	{
		string text = eve?.CasterItem?.indexOverrideOwnerName;
		if (text != null)
		{
			return text.ColorText(color);
		}
		CharacterAI characterAI = eve?.CasterOwner?.characterAI;
		if (characterAI == null)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_you;
		}
		return characterAI.EnemyName.ColorText(color);
	}
}
