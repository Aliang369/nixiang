using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "角色试玩数据", menuName = "Singleton/CharacterTrialConfig")]
public class CharacterTrialConfig : ScriptableObjectSingleton<CharacterTrialConfig>
{
	[Serializable]
	public class TrialRuneConfig
	{
		public RuneDef def;

		public int value = 1;

		public Rune ToRune()
		{
			if (def == null)
			{
				throw new NullReferenceException("Character trial deck rune config is missing RuneDef.");
			}
			return new Rune(def, value);
		}
	}

	[Serializable]
	public class TrialDeckItem
	{
		public ItemDefinition itemDef;

		public int upgradeLevel;

		public ItemSkinDef skinDef;

		public bool overrideCounter;

		public int counter;

		public int maxCounter;

		public List<TrialRuneConfig> runes = new List<TrialRuneConfig>();

		public Item ToItem()
		{
			if (itemDef == null)
			{
				throw new NullReferenceException("Character trial deck item config is missing ItemDefinition.");
			}
			Item item = new Item(itemDef);
			if (skinDef != null)
			{
				item.SetSkin(skinDef);
			}
			for (int i = 0; i < upgradeLevel; i++)
			{
				item.Upgrade();
			}
			foreach (TrialRuneConfig rune in runes)
			{
				item.AddRune(rune.ToRune());
			}
			if (overrideCounter)
			{
				item.counter = counter;
				item.maxCounter = maxCounter;
				item.resetInitCounter = counter;
				item.resetInitMaxCounter = maxCounter;
			}
			return item;
		}

		public GameRecord.ItemRecord ToItemRecord()
		{
			return new GameRecord.ItemRecord(ToItem());
		}
	}

	public class TrialEntry
	{
		public PlayerCharacterDef character;

		public CharacterMastery mastery;

		public DifficultyLevel difficulty = DifficultyLevel.Normal;

		public List<TrialDeckItem> deck = new List<TrialDeckItem>();

		public AssetReference tutorialGuideHintPanelSetPrefab;

		public List<SkillDef> skills = new List<SkillDef>();

		public bool upgradeSkillsToMaxQuality = true;

		public List<InfluenceDef> influences = new List<InfluenceDef>();

		public List<GlobalRewardSetup> completionRewards = new List<GlobalRewardSetup>();

		public RoomRef room;

		public EnemyAssetRef enemy;

		public int enemySetupIndex;

		public int enemyLevel = 1;

		public List<GameRecord.ItemRecord> ToDeckRecords()
		{
			if (deck.Count == 0)
			{
				throw new InvalidOperationException("Character trial deck config is empty.");
			}
			return deck.Select((TrialDeckItem item) => item.ToItemRecord()).ToList();
		}
	}

	public ContractDef trialContract;

	[Tooltip("每个角色的试玩配置。未配置会直接抛错。")]
	public List<TrialEntry> entries = new List<TrialEntry>();

	public TrialEntry TryGetEntry(PlayerCharacterDef character)
	{
		if (character == null)
		{
			return null;
		}
		return entries?.FirstOrDefault((TrialEntry e) => e != null && e.character == character);
	}

	public TrialEntry GetEntry(PlayerCharacterDef character)
	{
		return TryGetEntry(character) ?? throw new InvalidOperationException("Character trial config is missing for '" + (character?.name ?? "null") + "'.");
	}

	public Contract GenerateTrialContract(AssetCatalog assetCatalog, DifficultyLevel difficulty)
	{
		return trialContract.GenerateContract(assetCatalog, difficulty);
	}
}
