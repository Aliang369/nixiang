using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillGemDef : CTScriptableObject
{
	public enum GemType
	{
		Attack,
		Defence,
		Economy,
		Heal,
		Ability,
		AttackAssist
	}

	[LocalizeText("宝石名称")]
	[CTSOAssetName]
	public string skillGemName;

	public List<SkillWordStack> skillWordStacks;

	public HexShape hexShape;

	[HideInInspector]
	public GemType gemColorType;

	public override string CTAssetIDPrepend => "SKG";

	public Color GemColor => WordTypeInfo?.gemColor ?? Color.white;

	public int level => hexShape.points?.Count ?? 0;

	public SkillWordDef PrimarySkillWord => (from stack in skillWordStacks.OrEmptyIfNull()
		where stack?.skillWord != null && stack.wordStack > 0
		select stack.skillWord).FirstOrDefault();

	public SkillGemNamingConfig.WordTypeInfo WordTypeInfo => ScriptableObjectSingleton<SkillGemNamingConfig>.instance?.GetWordTypeInfo(PrimarySkillWord?.skillWordType ?? SkillWordDef.SkillWordType.None);

	public GemType GemColorType => WordTypeInfo?.gemType ?? gemColorType;

	public Sprite GemSprite => WordTypeInfo?.gemSprite;

	public static int SkillGemDefIndexComparer(SkillGemDef a, SkillGemDef b, ApplicationStateInfo applicationState = null)
	{
		int num = a.level.CompareTo(b.level);
		if (num != 0)
		{
			return num;
		}
		int num2 = a.GemColorType.CompareTo(b.GemColorType);
		if (num2 != 0)
		{
			return num2;
		}
		int num3 = a.SOindex.CompareTo(b.SOindex);
		if (num3 != 0)
		{
			return num3;
		}
		return string.CompareOrdinal(a.skillGemName, b.skillGemName);
	}

	public static QualityLevel GetLevelQuality(int level)
	{
		return level switch
		{
			2 => QualityLevel.Base, 
			3 => QualityLevel.Common, 
			4 => QualityLevel.Uncommon, 
			5 => QualityLevel.Rare, 
			_ => QualityLevel.Epic, 
		};
	}

	public string GenerateName()
	{
		return ScriptableObjectSingleton<SkillGemNamingConfig>.instance?.GetGemName(level, PrimarySkillWord);
	}

	public string GetDescription(ApplicationStateInfo applicationState, ProfileSkill profileSkill)
	{
		string text = "";
		text += $"lv.{level}".ColorText(GemColor);
		text += " ";
		foreach (var allWordDescription in GetAllWordDescriptions(applicationState, profileSkill))
		{
			text = text + allWordDescription.Item1 + " ";
		}
		return text;
	}

	public List<(string, TextDataGenerator)> GetAllWordDescriptions(ApplicationStateInfo applicationState, ProfileSkill profileSkill)
	{
		List<(string, TextDataGenerator)> list = new List<(string, TextDataGenerator)>();
		foreach (SkillWord gemSkillWord in GetGemSkillWords())
		{
			string formatDescription = gemSkillWord.GetFormatDescription(applicationState, profileSkill, out var generator);
			list.Add((formatDescription, generator));
		}
		return list;
	}

	public List<SkillWord> GetGemSkillWords()
	{
		List<SkillWord> list = new List<SkillWord>();
		if (skillWordStacks == null)
		{
			return list;
		}
		foreach (SkillWordStack skillWordStack in skillWordStacks)
		{
			if (!(skillWordStack.skillWord == null) && skillWordStack.wordStack > 0)
			{
				SkillWord item = new SkillWord(skillWordStack.skillWord, skillWordStack.wordStack, null);
				list.Add(item);
			}
		}
		return list;
	}
}
