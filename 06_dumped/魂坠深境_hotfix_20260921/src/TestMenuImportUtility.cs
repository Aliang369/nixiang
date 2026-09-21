using System;
using System.Collections.Generic;
using UnityEngine;

public static class TestMenuImportUtility
{
	[Serializable]
	public class TestRunImportJson
	{
		public string playerCharacter;

		public List<DeckItemJson> deck_detail;

		public List<string> bless_list;

		public List<SkillDetailJson> skill_list_detail;

		public List<string> activated_potential_list;
	}

	[Serializable]
	public class DeckItemJson
	{
		public string id;

		public int upgrade_level;

		public int counter;

		public List<string> runes;
	}

	[Serializable]
	public class SkillDetailJson
	{
		public string id;

		public int level;

		public int durability;

		public List<string> words;

		public string archetype;
	}

	public static ImportedTestSetup ImportFromJson(string json, AssetCatalog assetCatalog)
	{
		if (string.IsNullOrWhiteSpace(json))
		{
			throw new ArgumentException("json 为空", "json");
		}
		TestRunImportJson testRunImportJson = JsonUtility.FromJson<TestRunImportJson>(json);
		if (testRunImportJson == null)
		{
			throw new Exception("JSON 解析失败：TestRunImportJson 为空。");
		}
		return new ImportedTestSetup
		{
			Character = BuildPlayerCharacter(testRunImportJson.playerCharacter, assetCatalog),
			DeckRecords = BuildDeckRecords(testRunImportJson.deck_detail, assetCatalog),
			Blesses = BuildBlesses(testRunImportJson.bless_list, assetCatalog),
			Skills = BuildSkills(testRunImportJson.skill_list_detail, assetCatalog),
			ActivatedTalents = BuildTalents(testRunImportJson.activated_potential_list, assetCatalog)
		};
	}

	private static List<GameRecord.ItemRecord> BuildDeckRecords(List<DeckItemJson> deckDetail, AssetCatalog assetCatalog)
	{
		List<GameRecord.ItemRecord> list = new List<GameRecord.ItemRecord>();
		if (deckDetail == null)
		{
			return list;
		}
		foreach (DeckItemJson item2 in deckDetail)
		{
			if (string.IsNullOrEmpty(item2.id))
			{
				continue;
			}
			ItemDefinition itemDefinition = FindByFullId(item2.id, assetCatalog.itemDefCatalog?.assets);
			if (itemDefinition == null)
			{
				Debug.LogWarning("[Import] 找不到 ItemDefinition, id=" + item2.id);
				continue;
			}
			Item item = new Item(itemDefinition);
			for (int i = 0; i < item2.upgrade_level; i++)
			{
				item.Upgrade();
			}
			item.counter = item2.counter;
			if (item2.runes != null)
			{
				foreach (string rune2 in item2.runes)
				{
					if (!string.IsNullOrEmpty(rune2))
					{
						RuneDef runeDef = FindByFullId(rune2, assetCatalog.runeDefCatalog?.assets);
						if (runeDef == null)
						{
							Debug.LogWarning("[Import] 找不到 RuneDef, id=" + rune2);
							continue;
						}
						Rune rune = new Rune(runeDef);
						item.AddRune(rune);
					}
				}
			}
			list.Add(new GameRecord.ItemRecord(item));
		}
		return list;
	}

	private static List<InfluenceDef> BuildBlesses(List<string> blessList, AssetCatalog assetCatalog)
	{
		List<InfluenceDef> list = new List<InfluenceDef>();
		if (blessList == null)
		{
			return list;
		}
		foreach (string bless in blessList)
		{
			if (!string.IsNullOrEmpty(bless))
			{
				InfluenceDef influenceDef = FindByFullId(bless, assetCatalog.influenceCatalog?.assets);
				if (influenceDef != null)
				{
					list.Add(influenceDef);
				}
				else
				{
					Debug.LogWarning("[Import] 找不到 InfluenceDef, id=" + bless);
				}
			}
		}
		return list;
	}

	private static List<GameSkill> BuildSkills(List<SkillDetailJson> skillDetailList, AssetCatalog assetCatalog)
	{
		List<GameSkill> list = new List<GameSkill>();
		if (skillDetailList == null)
		{
			return list;
		}
		foreach (SkillDetailJson skillDetail in skillDetailList)
		{
			if (string.IsNullOrEmpty(skillDetail.id))
			{
				continue;
			}
			SkillDef skillDef = FindByFullId(skillDetail.id, assetCatalog.SkillDefCatalog?.assets);
			if (skillDef == null)
			{
				Debug.LogWarning("[Import] 找不到 SkillDef, id=" + skillDetail.id);
				continue;
			}
			GameSkill gameSkill = new GameSkill(new ProfileSkill(skillDef)
			{
				archetypeIndex = skillDetail.level
			});
			gameSkill.durability = skillDetail.durability;
			if (skillDetail.words != null)
			{
				foreach (string word in skillDetail.words)
				{
					if (!string.IsNullOrEmpty(word))
					{
						SkillWordDef skillWordDef = FindByFullId(word, assetCatalog.skillWordCatalog?.assets);
						if (skillWordDef == null)
						{
							Debug.LogWarning("[Import] 找不到 SkillWordDef, id=" + word);
							continue;
						}
						SkillWord item = new SkillWord(skillWordDef, skillDetail.words.Count, gameSkill);
						gameSkill.skillWords.Add(item);
					}
				}
			}
			list.Add(gameSkill);
		}
		return list;
	}

	private static List<TalentDef> BuildTalents(List<string> potentialList, AssetCatalog assetCatalog)
	{
		List<TalentDef> list = new List<TalentDef>();
		if (potentialList == null)
		{
			return list;
		}
		foreach (string potential in potentialList)
		{
			if (!string.IsNullOrEmpty(potential))
			{
				TalentDef talentDef = FindByFullId(potential, assetCatalog.talentCatalog?.assets);
				if (talentDef != null)
				{
					list.Add(talentDef);
				}
				else
				{
					Debug.LogWarning("[Import] 找不到 TalentDef, id=" + potential);
				}
			}
		}
		return list;
	}

	private static PlayerCharacterDef BuildPlayerCharacter(string playerCharacter, AssetCatalog assetCatalog)
	{
		PlayerCharacterDef playerCharacterDef = FindByFullId(playerCharacter, assetCatalog.characterDefCatalog?.assets);
		if (playerCharacterDef != null)
		{
			return playerCharacterDef;
		}
		Debug.LogWarning("[Import] 找不到 PlayerCharacterDef, id=" + playerCharacter);
		return null;
	}

	private static T FindByFullId<T>(string fullId, IList<T> catalogAssets) where T : class, ICTAsset
	{
		if (catalogAssets == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(fullId))
		{
			return null;
		}
		string importIdAppend = ICTAsset.GetImportIdAppend(fullId);
		int importIdIndex = ICTAsset.GetImportIdIndex(fullId);
		for (int i = 0; i < catalogAssets.Count; i++)
		{
			T val = catalogAssets[i];
			if (val != null && val.CTAssetIDPrepend == importIdAppend && val.AssetIndex == importIdIndex)
			{
				return val;
			}
		}
		return null;
	}
}
