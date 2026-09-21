using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "宝石命名配置", menuName = "Singleton/SkillGemNamingConfig")]
public class SkillGemNamingConfig : ScriptableObjectSingleton<SkillGemNamingConfig>
{
	[Serializable]
	public class LevelNameInfo
	{
		public int gridCount;

		public string prefix;
	}

	[Serializable]
	public class WordTypeInfo
	{
		public SkillWordDef.SkillWordType wordType;

		public SkillGemDef.GemType gemType;

		public string gemSuffix;

		public string displayName;

		public Sprite wordIcon;

		public Sprite gemSprite;

		public Color gemColor;
	}

	public List<LevelNameInfo> levelNames = new List<LevelNameInfo>();

	public List<WordTypeInfo> wordTypeInfos = new List<WordTypeInfo>();

	public LevelNameInfo GetLevelNameInfo(int gridCount)
	{
		return levelNames.FirstOrDefault((LevelNameInfo info) => info.gridCount == gridCount);
	}

	public WordTypeInfo GetWordTypeInfo(SkillWordDef.SkillWordType wordType)
	{
		return wordTypeInfos.FirstOrDefault((WordTypeInfo info) => info.wordType == wordType);
	}

	public string GetGemName(int gridCount, SkillWordDef skillWord)
	{
		if (skillWord == null)
		{
			return null;
		}
		LevelNameInfo levelNameInfo = GetLevelNameInfo(gridCount);
		WordTypeInfo wordTypeInfo = GetWordTypeInfo(skillWord.skillWordType);
		if (levelNameInfo == null || wordTypeInfo == null)
		{
			return null;
		}
		return levelNameInfo.prefix + skillWord.wordName + wordTypeInfo.gemSuffix;
	}

	public bool TryValidate(out string error)
	{
		int[] expectedKeys = Enumerable.Range(2, 6).ToArray();
		SkillWordDef.SkillWordType[] expectedKeys2 = (from SkillWordDef.SkillWordType type in Enum.GetValues(typeof(SkillWordDef.SkillWordType))
			where type != SkillWordDef.SkillWordType.None
			select type).ToArray();
		List<string> list = new List<string>();
		ValidateUniqueEntries(levelNames, expectedKeys, (LevelNameInfo info) => info.gridCount, "格数", list);
		ValidateUniqueEntries(wordTypeInfos, expectedKeys2, (WordTypeInfo info) => info.wordType, "词条类型", list);
		foreach (LevelNameInfo levelName in levelNames)
		{
			if (string.IsNullOrWhiteSpace(levelName.prefix))
			{
				list.Add($"格数 {levelName.gridCount} 的名称前缀为空");
			}
		}
		foreach (WordTypeInfo wordTypeInfo in wordTypeInfos)
		{
			if (string.IsNullOrWhiteSpace(wordTypeInfo.gemSuffix))
			{
				list.Add($"{wordTypeInfo.wordType} 的宝石后缀为空");
			}
			if (string.IsNullOrWhiteSpace(wordTypeInfo.displayName))
			{
				list.Add($"{wordTypeInfo.wordType} 的显示类型名为空");
			}
			if (wordTypeInfo.wordIcon == null)
			{
				list.Add($"{wordTypeInfo.wordType} 的词条图标为空");
			}
			if (wordTypeInfo.gemSprite == null)
			{
				list.Add($"{wordTypeInfo.wordType} 的宝石图标为空");
			}
		}
		foreach (IGrouping<string, WordTypeInfo> item in from info in wordTypeInfos
			group info by info.gemSuffix into @group
			where @group.Count() > 1
			select @group)
		{
			list.Add("宝石后缀 " + item.Key + " 被重复配置");
		}
		foreach (IGrouping<SkillGemDef.GemType, WordTypeInfo> item2 in from info in wordTypeInfos
			group info by info.gemType into @group
			where @group.Count() > 1
			select @group)
		{
			list.Add($"宝石类型 {item2.Key} 被重复配置");
		}
		error = string.Join("\n", list);
		return list.Count == 0;
	}

	private static void ValidateUniqueEntries<TEntry, TKey>(IEnumerable<TEntry> entries, IEnumerable<TKey> expectedKeys, Func<TEntry, TKey> getKey, string label, ICollection<string> errors)
	{
		List<TEntry> source = entries.ToList();
		foreach (TKey key in expectedKeys)
		{
			int num = source.Count((TEntry entry) => EqualityComparer<TKey>.Default.Equals(getKey(entry), key));
			if (num != 1)
			{
				errors.Add($"{label} {key} 应配置一次，当前为 {num} 次");
			}
		}
	}
}
