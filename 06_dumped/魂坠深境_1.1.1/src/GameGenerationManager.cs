using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class GameGenerationManager : InGameManager
{
	private const float ItemSkinRecommendationWeightMultiplier = 3f;

	[SaveField(null, null)]
	public List<ICTAsset> lockedContent;

	[SaveField(null, null)]
	private int cardRewardRareOffset = -4;

	[SaveField(null, null)]
	private int cardRewardRareOffsetStage;

	[SaveField(null, null)]
	private int cardRewardRareOffsetConfigVersion;

	private static readonly (int cost, float weight)[][] InitialBossBlessWillPowerWeights = new(int, float)[3][]
	{
		new(int, float)[1] { (2, 100f) },
		new(int, float)[1] { (3, 100f) },
		new(int, float)[1] { (5, 100f) }
	};

	private static readonly (int cost, float weight)[][] RefreshedBossBlessWillPowerWeights = new(int, float)[3][]
	{
		new(int, float)[2]
		{
			(1, 50f),
			(2, 50f)
		},
		new(int, float)[1] { (3, 100f) },
		new(int, float)[2]
		{
			(4, 50f),
			(5, 50f)
		}
	};

	public override void Init(GameStateInfo gameState)
	{
		if (lockedContent == null)
		{
			lockedContent = new List<ICTAsset>();
		}
	}

	private static T ListDequeue<T>(List<T> list) where T : class
	{
		if (list.Count == 0)
		{
			return null;
		}
		T result = list.FirstOrDefault();
		list.RemoveAt(0);
		return result;
	}

	public void LockContentGeneration(ICTAsset target)
	{
		if (!lockedContent.Contains(target))
		{
			lockedContent.Add(target);
		}
	}

	public bool IsLocked(ICTAsset target)
	{
		return lockedContent.Contains(target);
	}

	public List<ItemDefinition> GetAllGenerateAbleLoots()
	{
		List<ItemDefinition> list = base.gameState.applicationState.assetCatalog.itemDefCatalog.ValidAssets.FindAll((ItemDefinition t) => base.gameState.applicationState.globalUnlockManager.IsUnlocked(t) && !IsLocked(t) && (t.generationInfo?.isLoot ?? false));
		ContractDef contractDef = base.gameState.contractState?.selectedContract?.contractDef;
		if (contractDef != null)
		{
			list.RemoveAll((ItemDefinition itemDefinition) => !contractDef.CanGenerateLoot(base.gameState, itemDefinition));
		}
		return list;
	}

	public List<RuneDef> GetAllGenerateAbleRunes()
	{
		List<RuneDef> list = base.gameState.applicationState.assetCatalog.runeDefCatalog.ValidAssets.FindAll((RuneDef rune) => base.gameState.applicationState.globalUnlockManager.IsUnlocked(rune) && !IsLocked(rune) && rune.attachGenerationInfo != null);
		ContractDef contractDef = base.gameState.contractState?.selectedContract?.contractDef;
		if (contractDef != null)
		{
			list.RemoveAll((RuneDef runeDef) => !contractDef.CanGenerateRune(base.gameState, runeDef));
		}
		return list;
	}

	public List<InfluenceDef> GetAllGenerateAbleBless()
	{
		return base.gameState.applicationState.assetCatalog.influenceCatalog.ValidAssets.FindAll((InfluenceDef b) => b.randomGenerate && IsCharacterMatchedBless(b) && !IsLocked(b) && base.gameState.applicationState.globalUnlockManager.IsUnlocked(b));
	}

	public List<InfluenceDef> GenerateRandomBlessingsByQuality(int count, QualityLevel quality, IEnumerable<InfluenceDef> excluded = null, bool updateRecord = true)
	{
		if (count <= 0)
		{
			return new List<InfluenceDef>();
		}
		HashSet<InfluenceDef> excludedSet = new HashSet<InfluenceDef>(excluded ?? Enumerable.Empty<InfluenceDef>());
		List<InfluenceDef> list = (from blessing in GetAllGenerateAbleBless()
			where (quality == QualityLevel.None || blessing.quality == quality) && !excludedSet.Contains(blessing)
			select blessing).ToList();
		List<InfluenceDef> list2 = list.SelectRandomCountUnique(Mathf.Min(count, list.Count), base.gameState.randomManager.NextDouble);
		if (updateRecord)
		{
			list2.ForEach(delegate(InfluenceDef blessing)
			{
				base.gameState.recordManager.generatedInfluence.AddWithoutDuplicate(blessing);
			});
		}
		return list2;
	}

	private bool IsCharacterMatchedBless(InfluenceDef bless)
	{
		if (bless.attachCharacter != null)
		{
			return bless.attachCharacter == base.gameState?.playerState?.CharacterDef;
		}
		if (bless.attachCharacterClass != null)
		{
			return bless.attachCharacterClass == base.gameState?.playerState?.CharacterDef?.attachClass;
		}
		return true;
	}

	public List<Item> GenerateRandomLoot(int count)
	{
		List<Item> list = new List<Item>();
		List<ItemDefinition> allGenerateAbleLoots = GetAllGenerateAbleLoots();
		List<ItemDefinition> list2 = GrabFirstMaxXItemFromGuideListInPool(count, allGenerateAbleLoots, base.gameState.applicationState.guideManager.guideLoots);
		list.AddRange(list2.ConvertAll((ItemDefinition d) => new Item(d)));
		count -= list2.Count;
		list.AddRange(allGenerateAbleLoots.SelectRandomCountUnique(count, base.gameState.randomManager.NextDouble).ConvertAll((ItemDefinition itd) => new Item(itd)));
		return list;
	}

	public List<Item> GenerateCharacterPoolItems(int count = 1, int stageDifficulty = 1, QualityLevel filterQuality = QualityLevel.None, ItemContainer owner = null)
	{
		List<Item> list = new List<Item>();
		List<ItemDefinition> list2 = new List<ItemDefinition>();
		List<ItemDefinition> classPoolItem = base.gameState.playerState.CharacterDef.attachClass.GetPoolItems(base.gameState.applicationState);
		list2.AddRange(classPoolItem);
		List<ItemDefinition> collection = base.gameState.playerState.CharacterDef.secondaryClasses.OrEmptyIfNull().SelectMany((CharacterClassDef c) => c.GetPoolItems(base.gameState.applicationState)).ToList();
		list2.AddRange(collection);
		if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateItemFromAllCharacters))
		{
			list2 = new List<ItemDefinition>();
			foreach (CharacterClassState characterClassState3 in base.gameState.applicationState.characterManager.characterClassStates)
			{
				list2.AddRange(characterClassState3.classDef.GetPoolItems(base.gameState.applicationState));
			}
		}
		List<ItemDefinition> list3 = new List<ItemDefinition>();
		if (base.gameState.applicationState.characterManager != null)
		{
			if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateItemFromAllCharacters))
			{
				foreach (CharacterClassState characterClassState4 in base.gameState.applicationState.characterManager.characterClassStates)
				{
					if (characterClassState4 != null)
					{
						list3.AddRange(characterClassState4.GetForgetShrineItems());
					}
				}
			}
			else
			{
				CharacterClassState characterClassState = base.gameState.applicationState.characterManager.GetCharacterClassState(base.gameState.playerState.CharacterDef.attachClass);
				if (characterClassState != null)
				{
					foreach (ItemDefinition forgetShrineItem in characterClassState.GetForgetShrineItems())
					{
						list3.AddUnique(forgetShrineItem);
					}
				}
				foreach (CharacterClassDef item in base.gameState.playerState.CharacterDef.secondaryClasses.OrEmptyIfNull())
				{
					CharacterClassState characterClassState2 = base.gameState.applicationState.characterManager.GetCharacterClassState(item);
					if (characterClassState2 == null)
					{
						continue;
					}
					foreach (ItemDefinition forgetShrineItem2 in characterClassState2.GetForgetShrineItems())
					{
						list3.AddUnique(forgetShrineItem2);
					}
				}
			}
		}
		list2 = list2.Except(list3).ToList();
		if (filterQuality != QualityLevel.None)
		{
			list2 = list2.FindAll((ItemDefinition itd) => itd.qualityLevel == filterQuality);
		}
		List<ItemDefinition> list4 = GrabFirstMaxXItemFromGuideListInPool(count, list2, base.gameState.applicationState.guideManager.guideItems);
		count -= list4.Count;
		list2.RemoveAll(list4.Contains);
		list.AddRange(list2.SelectRandomWeightCountUnique(GetItemGenerateWeight, count, base.gameState.randomManager.NextDouble).ConvertAll((ItemDefinition itd) => new Item(itd)));
		list.AddRange(list4.ConvertAll((ItemDefinition itd) => new Item(itd)));
		HashSet<ItemDefinition> hashSet = list.Select((Item it) => it.GetDef()).ToHashSet();
		base.gameState.applicationState.guideManager.itemSkinRecommendedItems.RemoveAll(hashSet.Contains);
		list.ForEach(delegate(Item it)
		{
			it.owner = owner;
		});
		list.ForEach(delegate(Item it)
		{
			base.gameState.recordManager.generatedItems.AddWithoutDuplicate(it.GetDef());
		});
		return list;
		float GenerateQualityWeight(QualityLevel quality)
		{
			switch (quality)
			{
			case QualityLevel.None:
				return 0f;
			case QualityLevel.Base:
				return 0f;
			case QualityLevel.Common:
				return 100f;
			case QualityLevel.Uncommon:
				if (stageDifficulty == 1)
				{
					return 40f;
				}
				if (stageDifficulty == 2)
				{
					return 80f;
				}
				return 140f;
			case QualityLevel.Rare:
				if (stageDifficulty == 1)
				{
					return 20f;
				}
				if (stageDifficulty == 2)
				{
					return 80f;
				}
				if (stageDifficulty == 3)
				{
					return 120f;
				}
				return 200f;
			case QualityLevel.Epic:
				return 0f;
			default:
				return 0f;
			}
		}
		float GetClassPoolWeight(ItemDefinition itd)
		{
			if (classPoolItem.Contains(itd))
			{
				return 1f;
			}
			return 0.5f;
		}
		float GetItemGenerateWeight(ItemDefinition itd)
		{
			float num = GenerateQualityWeight(itd.qualityLevel) * GetItemGeneratedRecordWeight(itd) * GetClassPoolWeight(itd);
			if (base.gameState.applicationState.guideManager.itemSkinRecommendedItems.Contains(itd))
			{
				num *= 3f;
			}
			return num;
		}
		float GetItemGeneratedRecordWeight(ItemDefinition itd)
		{
			if (base.gameState.recordManager.generatedItems.Contains(itd))
			{
				return 0.3f;
			}
			return 1f;
		}
	}

	public List<InfluenceDef> GenerateRandomCommonBless(int count, bool updateRecord = true)
	{
		return GenerateRandomCommonBless(count, updateRecord, QualityLevel.None);
	}

	public List<Rune> GenerateMintageRunes(int count, List<RuneDef> excludeRunes = null, QualityLevel filterQuality = QualityLevel.None)
	{
		List<Rune> list = new List<Rune>();
		if (count <= 0)
		{
			return list;
		}
		bool withoutRuneAttachCountLimit = base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.WithoutRuneAttachCountLimit);
		List<Item> currentDeckItems = base.gameState.playerState.bft.deckContainer.GetAllItems();
		List<RuneDef> list2 = GetAllGenerateAbleRunes();
		if (filterQuality != QualityLevel.None)
		{
			list2 = list2.FindAll((RuneDef r) => r.quality == filterQuality);
		}
		if (excludeRunes != null)
		{
			list2.RemoveAll(excludeRunes.Contains);
		}
		List<RuneDef> list3 = list2.FindAll(CheckRuneAttachableNow);
		if (list3.Count < count)
		{
			list3 = list2;
		}
		List<RuneDef> list4 = GrabFirstMaxXItemFromGuideListInPool(count, list3, base.gameState.applicationState.guideManager.guideRunes);
		list.AddRange(list4.ConvertAll((RuneDef rd) => new Rune(rd, rd.attachGenerationInfo.stack)));
		count -= list4.Count;
		list3.RemoveAll(list4.Contains);
		bool boostCommonQuality = filterQuality == QualityLevel.None && base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MintageRuneCanGenerateNegative);
		while (count > 0 && list3.Count > 0)
		{
			RuneDef runeDef = SelectMintageRuneByRarity(list3, filterQuality, boostCommonQuality);
			if (runeDef == null)
			{
				break;
			}
			list.Add(new Rune(runeDef, runeDef.attachGenerationInfo.stack));
			list3.Remove(runeDef);
			count--;
		}
		return list;
		bool CheckAttachableTo(RuneDef rune, Item item)
		{
			if (item.Runes.Count > 0 && !withoutRuneAttachCountLimit)
			{
				return false;
			}
			if (rune.CheckCanAddRuneTo(item) != true)
			{
				return false;
			}
			return true;
		}
		bool CheckRuneAttachableNow(RuneDef rune)
		{
			foreach (Item item in currentDeckItems)
			{
				if (CheckAttachableTo(rune, item))
				{
					return true;
				}
			}
			return false;
		}
	}

	private RuneDef SelectMintageRuneByRarity(List<RuneDef> candidates, QualityLevel filterQuality, bool boostCommonQuality)
	{
		if (candidates.IsNullOrEmpty())
		{
			return null;
		}
		double num = base.gameState.randomManager.NextDouble();
		QualityLevel quality;
		double withinRarityValue;
		if (filterQuality != QualityLevel.None)
		{
			quality = filterQuality;
			withinRarityValue = num;
		}
		else
		{
			DungeonGenerationManager instance = ScriptableObjectSingleton<DungeonGenerationManager>.instance;
			DungeonGenerationManager.RuneRarityWeights runeRarityWeights = instance.runeRarityWeights ?? (instance.runeRarityWeights = new DungeonGenerationManager.RuneRarityWeights());
			int common = (boostCommonQuality ? (runeRarityWeights.common * 2) : runeRarityWeights.common);
			quality = QualityRarityRoller.Roll(common, runeRarityWeights.uncommon, runeRarityWeights.rare, candidates.Exists((RuneDef rune) => rune.quality == QualityLevel.Common), candidates.Exists((RuneDef rune) => rune.quality == QualityLevel.Uncommon), candidates.Exists((RuneDef rune) => rune.quality == QualityLevel.Rare), num, out withinRarityValue);
		}
		List<RuneDef> list = candidates.FindAll((RuneDef rune) => rune.quality == quality);
		if (list.IsNullOrEmpty())
		{
			return null;
		}
		return list.SelectRandomWeight((RuneDef rune) => rune.attachGenerationInfo.weight, () => withinRarityValue);
	}

	public List<Item> GenerateCardRewardItems(int count, int currentStage, CardRewardType rewardType, ItemContainer owner = null)
	{
		DungeonGenerationManager instance = ScriptableObjectSingleton<DungeonGenerationManager>.instance;
		DungeonGenerationManager.CardRewardRarityConfig cardRewardRarityConfig = instance.cardRewardRarityConfig ?? (instance.cardRewardRarityConfig = new DungeonGenerationManager.CardRewardRarityConfig());
		cardRewardRarityConfig.EnsureDefaults();
		List<Item> list = new List<Item>();
		List<ItemDefinition> list2 = BuildCardRewardPool(out var classPoolItems);
		QualityLevel forcedQuality = ((rewardType == CardRewardType.BossBattle || rewardType == CardRewardType.GuaranteedRare) ? QualityLevel.Rare : QualityLevel.None);
		bool flag = rewardType == CardRewardType.NormalBattle || rewardType == CardRewardType.EliteBattle;
		if (forcedQuality != QualityLevel.None)
		{
			list2 = list2.FindAll((ItemDefinition itd) => itd.qualityLevel == forcedQuality);
		}
		List<ItemDefinition> list3 = GrabFirstMaxXItemFromGuideListInPool(count, list2, base.gameState.applicationState.guideManager.guideItems);
		int num = count - list3.Count;
		list2.RemoveAll(list3.Contains);
		if (forcedQuality == QualityLevel.None)
		{
			list2.RemoveAll((ItemDefinition itd) => itd.qualityLevel != QualityLevel.Common && itd.qualityLevel != QualityLevel.Uncommon && itd.qualityLevel != QualityLevel.Rare);
		}
		DungeonGenerationManager.CardRewardRarityTierConfig tier = cardRewardRarityConfig.GetTier(currentStage);
		int num2 = cardRewardRarityConfig.ResolveRareOffset(currentStage, cardRewardRareOffsetStage, cardRewardRareOffset, cardRewardRareOffsetConfigVersion);
		DungeonGenerationManager.CardRewardRarityWeights cardRewardRarityWeights = ((rewardType == CardRewardType.EliteBattle || rewardType == CardRewardType.Event) ? tier.elite : tier.normal);
		for (int num3 = 0; num3 < num; num3++)
		{
			if (list2.IsNullOrEmpty())
			{
				TraceLog.Error($"卡牌奖励池数量不足，期望生成{count}张，实际生成{list.Count + list3.Count}张。");
				break;
			}
			double num4 = base.gameState.randomManager.NextDouble();
			QualityLevel quality;
			double withinRarityValue;
			if (forcedQuality != QualityLevel.None)
			{
				quality = forcedQuality;
				withinRarityValue = num4;
			}
			else
			{
				DungeonGenerationManager.CardRewardRarityWeights weights = cardRewardRarityWeights.ApplyRareOffset(flag ? num2 : 0);
				quality = QualityRarityRoller.Roll(weights, list2.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Common), list2.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Uncommon), list2.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Rare), num4, out withinRarityValue);
			}
			List<ItemDefinition> list4 = list2.FindAll((ItemDefinition itd) => itd.qualityLevel == quality);
			if (list4.IsNullOrEmpty())
			{
				TraceLog.Error($"卡牌奖励的{quality}卡池为空，期望生成{count}张，实际生成{list.Count + list3.Count}张。");
				break;
			}
			ItemDefinition itemDefinition = list4.SelectRandomWeight(GetItemGenerateWeight, () => withinRarityValue);
			list2.Remove(itemDefinition);
			list.Add(new Item(itemDefinition));
			if (flag)
			{
				if (quality == QualityLevel.Common)
				{
					num2 = Mathf.Min(cardRewardRarityConfig.rareOffsetMax, num2 + tier.rareOffsetCommonIncrease);
				}
				else if (quality == QualityLevel.Rare)
				{
					num2 = tier.rareOffsetInitial;
				}
			}
		}
		list.AddRange(list3.ConvertAll((ItemDefinition itd) => new Item(itd)));
		if (flag)
		{
			cardRewardRareOffset = num2;
			cardRewardRareOffsetStage = currentStage;
			cardRewardRareOffsetConfigVersion = 1;
		}
		HashSet<ItemDefinition> hashSet = list.Select((Item it) => it.GetDef()).ToHashSet();
		base.gameState.applicationState.guideManager.itemSkinRecommendedItems.RemoveAll(hashSet.Contains);
		list.ForEach(delegate(Item it)
		{
			it.owner = owner;
		});
		list.ForEach(delegate(Item it)
		{
			base.gameState.recordManager.generatedItems.AddWithoutDuplicate(it.GetDef());
		});
		return list;
		float GetItemGenerateWeight(ItemDefinition itd)
		{
			float num5 = (base.gameState.recordManager.generatedItems.Contains(itd) ? 0.3f : 1f);
			if (!classPoolItems.Contains(itd))
			{
				num5 *= 0.5f;
			}
			if (base.gameState.applicationState.guideManager.itemSkinRecommendedItems.Contains(itd))
			{
				num5 *= 3f;
			}
			return num5;
		}
	}

	public List<Item> GenerateStoreCardItems(int count, ItemContainer owner = null)
	{
		List<Item> list = new List<Item>();
		if (count <= 0)
		{
			return list;
		}
		List<ItemDefinition> originalPoolItems = BuildCardRewardPool(out var classPoolItems).Where(delegate(ItemDefinition itd)
		{
			QualityLevel qualityLevel = itd.qualityLevel;
			return qualityLevel == QualityLevel.Common || qualityLevel == QualityLevel.Uncommon || qualityLevel == QualityLevel.Rare;
		}).Distinct().ToList();
		List<ItemDefinition> allPoolItems = new List<ItemDefinition>(originalPoolItems);
		DungeonGenerationManager instance = ScriptableObjectSingleton<DungeonGenerationManager>.instance;
		DungeonGenerationManager.CardRewardRarityWeights normalRewardWeights = (instance.cardRewardRarityConfig ?? (instance.cardRewardRarityConfig = new DungeonGenerationManager.CardRewardRarityConfig())).GetTier(base.gameState.difficultyState.CurrentStage).normal;
		List<ItemDefinition> guideItems = base.gameState.applicationState.guideManager.guideItems;
		if (!guideItems.IsNullOrEmpty() && originalPoolItems.Exists(guideItems.Contains))
		{
			list.AddRange(GenerateCharacterPoolItems(count, base.gameState.difficultyState.CurrentStage, QualityLevel.None, owner));
			HashSet<ItemDefinition> hashSet = list.Select((Item item) => item.GetDef()).ToHashSet();
			allPoolItems.RemoveAll(hashSet.Contains);
			if (list.Count < count)
			{
				TraceLog.Warn($"教学商店商品不足，已按普通奖励概率补充{count - list.Count}张。");
			}
			while (list.Count < count)
			{
				ItemDefinition itemDefinition = SelectRewardRandomFallback();
				if (itemDefinition == null)
				{
					TraceLog.Error($"教学商店可生成卡池为空，期望生成{count}张，实际生成{list.Count}张。");
					break;
				}
				allPoolItems.Remove(itemDefinition);
				list.Add(new Item(itemDefinition));
			}
		}
		else
		{
			for (int num = 0; num < count; num++)
			{
				QualityLevel targetQuality = StoreCardQualitySequence.GetQualityAt(num);
				List<ItemDefinition> list2 = allPoolItems.FindAll((ItemDefinition itd) => itd.qualityLevel == targetQuality);
				ItemDefinition itemDefinition2;
				if (!list2.IsNullOrEmpty())
				{
					itemDefinition2 = list2.SelectRandomWeight(GetItemGenerateWeight, base.gameState.randomManager.NextDouble);
				}
				else
				{
					itemDefinition2 = SelectRewardRandomFallback();
					TraceLog.Warn($"商店的{targetQuality}卡池为空，已按普通奖励概率补充商品。");
				}
				if (itemDefinition2 == null)
				{
					TraceLog.Error($"商店可生成卡池为空，期望生成{count}张，实际生成{list.Count}张。");
					break;
				}
				allPoolItems.Remove(itemDefinition2);
				list.Add(new Item(itemDefinition2));
			}
		}
		HashSet<ItemDefinition> hashSet2 = list.Select((Item it) => it.GetDef()).ToHashSet();
		base.gameState.applicationState.guideManager.itemSkinRecommendedItems.RemoveAll(hashSet2.Contains);
		list.ForEach(delegate(Item it)
		{
			it.owner = owner;
		});
		list.ForEach(delegate(Item it)
		{
			base.gameState.recordManager.generatedItems.AddWithoutDuplicate(it.GetDef());
		});
		return list;
		float GetItemGenerateWeight(ItemDefinition itd)
		{
			float num2 = (base.gameState.recordManager.generatedItems.Contains(itd) ? 0.3f : 1f);
			if (!classPoolItems.Contains(itd))
			{
				num2 *= 0.5f;
			}
			if (base.gameState.applicationState.guideManager.itemSkinRecommendedItems.Contains(itd))
			{
				num2 *= 3f;
			}
			return num2;
		}
		ItemDefinition SelectRewardRandomFallback()
		{
			List<ItemDefinition> list3 = (allPoolItems.IsNullOrEmpty() ? originalPoolItems : allPoolItems);
			if (list3.IsNullOrEmpty())
			{
				return null;
			}
			double randomValue = base.gameState.randomManager.NextDouble();
			QualityLevel quality = QualityRarityRoller.Roll(normalRewardWeights, list3.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Common), list3.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Uncommon), list3.Exists((ItemDefinition itd) => itd.qualityLevel == QualityLevel.Rare), randomValue, out var withinRarityValue);
			return list3.FindAll((ItemDefinition itd) => itd.qualityLevel == quality).SelectRandomWeight(GetItemGenerateWeight, () => withinRarityValue);
		}
	}

	private List<ItemDefinition> BuildCardRewardPool(out List<ItemDefinition> classPoolItems)
	{
		classPoolItems = base.gameState.playerState.CharacterDef.attachClass.GetPoolItems(base.gameState.applicationState);
		List<ItemDefinition> list = new List<ItemDefinition>(classPoolItems);
		list.AddRange(base.gameState.playerState.CharacterDef.secondaryClasses.OrEmptyIfNull().SelectMany((CharacterClassDef c) => c.GetPoolItems(base.gameState.applicationState)));
		if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateItemFromAllCharacters))
		{
			list.Clear();
			foreach (CharacterClassState characterClassState3 in base.gameState.applicationState.characterManager.characterClassStates)
			{
				list.AddRange(characterClassState3.classDef.GetPoolItems(base.gameState.applicationState));
			}
		}
		List<ItemDefinition> list2 = new List<ItemDefinition>();
		if (base.gameState.applicationState.characterManager != null)
		{
			if (base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateItemFromAllCharacters))
			{
				foreach (CharacterClassState characterClassState4 in base.gameState.applicationState.characterManager.characterClassStates)
				{
					if (characterClassState4 != null)
					{
						list2.AddRange(characterClassState4.GetForgetShrineItems());
					}
				}
			}
			else
			{
				CharacterClassState characterClassState = base.gameState.applicationState.characterManager.GetCharacterClassState(base.gameState.playerState.CharacterDef.attachClass);
				if (characterClassState != null)
				{
					foreach (ItemDefinition forgetShrineItem in characterClassState.GetForgetShrineItems())
					{
						list2.AddUnique(forgetShrineItem);
					}
				}
				foreach (CharacterClassDef item in base.gameState.playerState.CharacterDef.secondaryClasses.OrEmptyIfNull())
				{
					CharacterClassState characterClassState2 = base.gameState.applicationState.characterManager.GetCharacterClassState(item);
					if (characterClassState2 == null)
					{
						continue;
					}
					foreach (ItemDefinition forgetShrineItem2 in characterClassState2.GetForgetShrineItems())
					{
						list2.AddUnique(forgetShrineItem2);
					}
				}
			}
		}
		return list.Except(list2).ToList();
	}

	public List<Rune> GenerateMintageRunePairForSameItem(List<RuneDef> excludeRunes = null, QualityLevel filterQuality = QualityLevel.None)
	{
		bool withoutRuneAttachCountLimit = base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.WithoutRuneAttachCountLimit);
		List<Item> currentDeckItems = base.gameState.playerState.bft.deckContainer.GetAllItems();
		List<RuneDef> candidates = GetAllGenerateAbleRunes();
		if (filterQuality != QualityLevel.None)
		{
			candidates = candidates.FindAll((RuneDef r) => r.quality == filterQuality);
		}
		if (excludeRunes != null)
		{
			candidates.RemoveAll(excludeRunes.Contains);
		}
		candidates = candidates.FindAll((RuneDef rune) => currentDeckItems.Any((Item item) => CheckAttachableTo(rune, item)));
		bool boostCommonQuality = filterQuality == QualityLevel.None && base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MintageRuneCanGenerateNegative);
		List<RuneDef> guideRunes = base.gameState.applicationState.guideManager.guideRunes;
		List<RuneDef> list = candidates.FindAll((RuneDef first) => candidates.Any((RuneDef second) => second != first && CheckAttachableToSameItem(first, second)));
		if (list.Count == 0)
		{
			if (candidates.Count == 0)
			{
				return GenerateMintageRunes(2, excludeRunes, filterQuality);
			}
			RuneDef runeDef = guideRunes?.FirstOrDefault(candidates.Contains) ?? SelectMintageRuneByRarity(candidates, filterQuality, boostCommonQuality);
			guideRunes?.Remove(runeDef);
			return new List<Rune>
			{
				new Rune(runeDef, runeDef.attachGenerationInfo.stack)
			};
		}
		RuneDef firstDef = guideRunes?.FirstOrDefault(list.Contains) ?? SelectMintageRuneByRarity(list, filterQuality, boostCommonQuality);
		List<RuneDef> list2 = candidates.FindAll((RuneDef second) => second != firstDef && CheckAttachableToSameItem(firstDef, second));
		RuneDef runeDef2 = guideRunes?.FirstOrDefault(list2.Contains) ?? SelectMintageRuneByRarity(list2, filterQuality, boostCommonQuality);
		guideRunes?.Remove(firstDef);
		guideRunes?.Remove(runeDef2);
		return new List<Rune>
		{
			new Rune(firstDef, firstDef.attachGenerationInfo.stack),
			new Rune(runeDef2, runeDef2.attachGenerationInfo.stack)
		};
		bool CheckAttachableTo(RuneDef rune, Item item)
		{
			if (item.Runes.Count > 0 && !withoutRuneAttachCountLimit)
			{
				return false;
			}
			return rune.CheckCanAddRuneTo(item) == true;
		}
		bool CheckAttachableToSameItem(RuneDef first, RuneDef second)
		{
			return currentDeckItems.Any((Item item) => CheckAttachableTo(first, item) && CheckAttachableTo(second, item));
		}
	}

	public (EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) GenerateEnemy(GameStateInfo gameState, EnemyEncounterType encounterType, bool spawnEliteVariant = false, IReadOnlyCollection<EnemyInfoData> excludedEnemies = null)
	{
		if (!TrySelectEnemy(gameState, encounterType, spawnEliteVariant, excludedEnemies, out var selectedEnemy))
		{
			throw new InvalidOperationException($"没有可生成的敌人，遭遇类型：{encounterType}");
		}
		return ApplyEnemyHealthPercentDelta(gameState, selectedEnemy);
	}

	public bool TryPreselectEnemyForEncounter(GameStateInfo gameState, EncounterSetupInfo setupInfo, PathLocation location)
	{
		if (setupInfo == null)
		{
			return false;
		}
		if (setupInfo.fixedEnemyInfoData != null)
		{
			return true;
		}
		if (setupInfo.preselectedEnemyInfoData != null && setupInfo.PreselectedEnemySetup != null)
		{
			return true;
		}
		if (setupInfo.enemySpawnType == EnemyEncounterType.None)
		{
			return false;
		}
		bool spawnEliteVariant = location != null && location.variantType == PathLocation.LocationVariantType.EliteVariant;
		HashSet<EnemyInfoData> excludedEnemies = (from enemyRef in setupInfo.excludedEnemyRefs.OrEmptyIfNull()
			where enemyRef?.IsValidRef(gameState.applicationState.assetCatalog) ?? false
			select enemyRef.GetAssetPreData(gameState.applicationState.assetCatalog)).ToHashSet();
		if (!TrySelectEnemy(gameState, setupInfo.enemySpawnType, spawnEliteVariant, excludedEnemies, out var selectedEnemy))
		{
			return false;
		}
		int num = selectedEnemy.Item1?.setups?.IndexOf(selectedEnemy.Item2) ?? (-1);
		if (num < 0)
		{
			return false;
		}
		setupInfo.preselectedEnemyInfoData = selectedEnemy.Item1;
		setupInfo.preselectedEnemySetupIndex = num;
		return setupInfo.PreselectedEnemySetup != null;
	}

	public (EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) ApplyEnemyHealthPercentDelta(GameStateInfo gameState, (EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) setupInfo)
	{
		int variableValue = gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.EnemyHealthPercentDelta);
		if (variableValue == 0 || setupInfo.Item2?.difficultyInfo == null)
		{
			return setupInfo;
		}
		float num = Mathf.Max(0.01f, (100f + (float)variableValue) / 100f);
		CharacterAIComponent.DiffcultyInfo difficultyInfo = setupInfo.Item2.difficultyInfo;
		CharacterAIComponent.EnemySpawnSetup item = new CharacterAIComponent.EnemySpawnSetup
		{
			difficultyInfo = new CharacterAIComponent.DiffcultyInfo
			{
				allowEncounterTypes = difficultyInfo.allowEncounterTypes,
				healthPercent = difficultyInfo.healthPercent * num,
				intentSlotCount = difficultyInfo.intentSlotCount
			},
			itemGenerations = setupInfo.Item2.itemGenerations
		};
		return (setupInfo.Item1, item);
	}

	private bool TrySelectEnemy(GameStateInfo gameState, EnemyEncounterType encounterType, bool spawnEliteVariant, IReadOnlyCollection<EnemyInfoData> excludedEnemies, out (EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) selectedEnemy)
	{
		selectedEnemy = default((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup));
		Level currentLevel = gameState.contractState.currentLevel;
		List<EnemyInfoData> encounteredEnemies = gameState.recordManager.encounteredEnemies;
		List<(EnemyInfoData, CharacterAIComponent.EnemySpawnSetup)> list = new List<(EnemyInfoData, CharacterAIComponent.EnemySpawnSetup)>();
		List<EnemyInfoData> validLevelEnemies = currentLevel.GetValidLevelEnemies(gameState.applicationState.assetCatalog);
		validLevelEnemies.AddRange(gameState.applicationState.assetCatalog.enemyInfoDataCatalog.ValidAssets.FindAll((EnemyInfoData e) => e.globalSpawn));
		if (excludedEnemies != null && excludedEnemies.Count > 0)
		{
			validLevelEnemies.RemoveAll(((IEnumerable<EnemyInfoData>)excludedEnemies).Contains<EnemyInfoData>);
		}
		if (spawnEliteVariant)
		{
			validLevelEnemies.RemoveAll((EnemyInfoData e) => !e.isEliteVariant);
		}
		else
		{
			validLevelEnemies.RemoveAll((EnemyInfoData e) => e.isEliteVariant);
		}
		foreach (EnemyInfoData item2 in validLevelEnemies)
		{
			if (base.gameState.difficultyState.contractDifficulty < item2.unlockDifficulty || !gameState.applicationState.globalUnlockManager.IsUnlocked(item2))
			{
				continue;
			}
			if (gameState.applicationState.globalUnlockManager.CheckNewUnlockAndTryReset(item2))
			{
				list.Clear();
				foreach (CharacterAIComponent.EnemySpawnSetup setup in item2.setups)
				{
					if (setup.difficultyInfo.allowEncounterTypes.Contains(encounterType))
					{
						list.Add((item2, setup));
					}
				}
				break;
			}
			foreach (CharacterAIComponent.EnemySpawnSetup setup2 in item2.setups)
			{
				if (setup2.difficultyInfo.allowEncounterTypes.Contains(encounterType))
				{
					list.Add((item2, setup2));
				}
			}
		}
		if (list.Count == 0)
		{
			return false;
		}
		List<EnemyInfoData> pool = list.Select(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) t) => t.Item1).Distinct().ToList();
		List<EnemyInfoData> guideItems = gameState.applicationState.guideManager.guideEnemies.ConvertAll((EnemyAssetRef e) => e.GetAssetPreData(gameState.applicationState.assetCatalog));
		List<EnemyInfoData> list2 = GrabFirstMaxXItemFromGuideListInPool(1, pool, guideItems);
		foreach (EnemyInfoData info in list2)
		{
			EnemyAssetRef item = gameState.applicationState.guideManager.guideEnemies.FirstOrDefault((EnemyAssetRef e) => e.GetAssetPreData(gameState.applicationState.assetCatalog) == info);
			gameState.applicationState.guideManager.guideEnemies.Remove(item);
		}
		if (list2.Count > 0)
		{
			EnemyInfoData guidedEnemyInfo = list2[0];
			selectedEnemy = list.First(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) t) => t.Item1 == guidedEnemyInfo);
			return true;
		}
		List<(EnemyInfoData, CharacterAIComponent.EnemySpawnSetup)> list3 = list.FindAll(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) e) => !encounteredEnemies.Contains(e.Item1));
		if (list3.Count == 0)
		{
			TraceLog.Error("没有足够的敌人可以按照规则生成");
			list3 = list;
		}
		if (!ScriptableObjectSingleton<GameDataManager>.instance.finalBossRandomGenerateClaivs.isUnlocked)
		{
			if (!ScriptableObjectSingleton<GameDataManager>.instance.finalBossFixGenerateBeastFixClavis.isUnlocked)
			{
				int num = list3.FindIndex(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) b) => b.Item1.FullId == ScriptableObjectSingleton<GameDataManager>.instance.knightfinalBoss.CTAssetId);
				if (num > 0)
				{
					selectedEnemy = list3[num];
					return true;
				}
			}
			if (ScriptableObjectSingleton<GameDataManager>.instance.finalBossFixGenerateBeastFixClavis.isUnlocked && !ScriptableObjectSingleton<GameDataManager>.instance.finalBossRandomGenerateClaivs.isUnlocked)
			{
				int num2 = list3.FindIndex(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) b) => b.Item1.FullId == ScriptableObjectSingleton<GameDataManager>.instance.beastfinalBoss.CTAssetId);
				if (num2 > 0)
				{
					selectedEnemy = list3[num2];
					return true;
				}
			}
		}
		selectedEnemy = list3.SelectRandomWeight(((EnemyInfoData, CharacterAIComponent.EnemySpawnSetup) e) => (!e.Item1.globalSpawn) ? 1f : 0.2f, gameState.randomManager.NextDouble);
		return true;
	}

	public EnvironmentInfluenceDef GenerateEnvironmentInfluenceDef(Level level, PathLocation loc)
	{
		List<Level.EnvironmentInfluenceSpawnSetup> list = level.environmentInfluenceSpawnSetups.FindAll((Level.EnvironmentInfluenceSpawnSetup s) => s.def.IsValidAsset() && base.gameState.applicationState.globalUnlockManager.IsUnlocked(s.def));
		bool variableFlag = base.gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.EnableHardLevelEnvironmentInfluence);
		if (!variableFlag)
		{
			list.RemoveAll((Level.EnvironmentInfluenceSpawnSetup e) => e.def.type == EnvironmentInfluenceDef.EnvironmentInfluenceType.Negative);
		}
		if (!list.IsNullOrEmpty())
		{
			return list.SelectRandomWeight((Level.EnvironmentInfluenceSpawnSetup s) => s.weight, base.gameState.randomManager.NextDouble).def;
		}
		List<EnvironmentInfluenceDef> list2 = ScriptableObjectSingleton<DungeonGenerationManager>.instance.defaultEnvironmentInfluences;
		if (!variableFlag)
		{
			list2 = list2.FindAll((EnvironmentInfluenceDef e) => e != null && e.type != EnvironmentInfluenceDef.EnvironmentInfluenceType.Negative);
		}
		return list2.SelectRandom();
	}

	public List<T> GrabFirstMaxXItemFromGuideListInPool<T>(int maxCount, List<T> pool, List<T> guideItems)
	{
		List<T> list = new List<T>();
		if (guideItems.IsNullOrEmpty() || pool.IsNullOrEmpty())
		{
			return list;
		}
		int num = 0;
		while (num < guideItems.Count && list.Count < maxCount)
		{
			T item = guideItems[num];
			if (pool.Contains(item) && !list.Contains(item))
			{
				list.Add(item);
				guideItems.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
		return list;
	}

	public WorldEvent GenerateRandomWorldEventTemplate()
	{
		List<WorldEvent> recordEvents = base.gameState.recordManager.generatedWorldEventTemplates;
		List<WorldEvent> list = base.gameState.applicationState.assetCatalog.worldEventCatalog.ValidAssets.FindAll((WorldEvent e) => e.CheckWorldEventGenerateAble(base.gameState) && base.gameState.applicationState.globalUnlockManager.IsUnlocked(e));
		foreach (WorldEvent item in list)
		{
			if (base.gameState.applicationState.globalUnlockManager.CheckNewUnlockAndTryReset(item))
			{
				return item;
			}
		}
		List<WorldEvent> list2 = list.FindAll((WorldEvent g) => g.globalGenerate);
		List<WorldEvent> list3 = list.FindAll((WorldEvent g) => base.gameState.contractState.currentLevel.worldEventPool.Contains(g));
		WorldEvent worldEvent = null;
		worldEvent = ((list2.Count > 0 && base.gameState.randomManager.NextFloat(1f) < 0.2f) ? list2.SelectRandomWeight(SelectWeightGenerator, base.gameState.randomManager.NextDouble) : ((list3.Count <= 0) ? list.SelectRandomWeight(SelectWeightGenerator, base.gameState.randomManager.NextDouble) : list3.SelectRandomWeight(SelectWeightGenerator, base.gameState.randomManager.NextDouble)));
		if (worldEvent != null)
		{
			base.gameState.recordManager.generatedWorldEventTemplates.AddWithoutDuplicate(worldEvent);
		}
		return worldEvent;
		float SelectWeightGenerator(WorldEvent item)
		{
			if (recordEvents.Contains(item))
			{
				return 0f;
			}
			return 100f;
		}
	}

	public List<Influence> GenerateRandomBossBless(bool updateRecord, bool isRefresh = false)
	{
		(int cost, float weight)[][] obj = (isRefresh ? RefreshedBossBlessWillPowerWeights : InitialBossBlessWillPowerWeights);
		List<Influence> randomOrder = GenerateRootBlessOptions(blessCount: obj.Length, poolFilter: (InfluenceDef bless) => bless.randomGenerate, updateRecord: updateRecord, bossBlessMatcher: IsCharacterMatchedBless);
		List<int> list = obj.Select(SelectBossBlessWillPowerCost).ToList();
		randomOrder = SortBossBlessBySnapshotWeight(randomOrder);
		for (int num = 0; num < randomOrder.Count; num++)
		{
			randomOrder[num].willPowerCost = list[num];
		}
		return randomOrder;
	}

	private List<Influence> SortBossBlessBySnapshotWeight(List<Influence> randomOrder)
	{
		ContractRuleDef obj = base.gameState.contractState?.selectedContract?.contractDef?.contractRule;
		if ((object)obj != null && !obj.CanUseBossBlessAutoBalance(base.gameState))
		{
			return randomOrder;
		}
		BossBlessWeightSnapshot bossBlessWeightSnapshot = base.gameState.newGameConfigData?.startGameInfo?.bossBlessWeightSnapshot;
		if (bossBlessWeightSnapshot == null || !bossBlessWeightSnapshot.HasWeights || randomOrder == null || randomOrder.Count == 0)
		{
			return randomOrder;
		}
		List<(Influence, double, int)> list = new List<(Influence, double, int)>(randomOrder.Count);
		for (int i = 0; i < randomOrder.Count; i++)
		{
			Influence influence = randomOrder[i];
			string text = influence?.def?.FullID;
			if (string.IsNullOrEmpty(text) || !bossBlessWeightSnapshot.Weights.TryGetValue(text, out var value) || double.IsNaN(value) || double.IsInfinity(value))
			{
				return randomOrder;
			}
			list.Add((influence, value, i));
		}
		return (from item in list
			orderby item.weight, item.index
			select item.influence).ToList();
	}

	private int SelectBossBlessWillPowerCost(IList<(int cost, float weight)> weights)
	{
		return weights.SelectRandomWeight(((int cost, float weight) option) => option.weight, base.gameState.randomManager.NextDouble).cost;
	}

	public List<Influence> GenerateRootBonfireBlessOptions(IReadOnlyList<int> willPowerCosts, bool updateRecord)
	{
		int blessCount = willPowerCosts?.Count ?? 0;
		List<Influence> list = GenerateRootBlessOptions((InfluenceDef bless) => bless.rootBonfireBlessPool, blessCount, updateRecord);
		for (int num = 0; num < list.Count; num++)
		{
			list[num].willPowerCost = willPowerCosts[num];
		}
		return list;
	}

	private List<Influence> GenerateRootBlessOptions(Func<InfluenceDef, bool> poolFilter, int blessCount, bool updateRecord, Func<InfluenceDef, bool> bossBlessMatcher = null)
	{
		if (bossBlessMatcher == null)
		{
			bossBlessMatcher = IsCharacterMatchedBless;
		}
		List<Influence> list = base.gameState.applicationState.assetCatalog.influenceCatalog.ValidAssets.FindAll(poolFilter.Invoke).FindAll(bossBlessMatcher.Invoke).FindAll((InfluenceDef b) => base.gameState.applicationState.globalUnlockManager.IsUnlocked(b))
			.FindAll((InfluenceDef b) => b.quality == QualityLevel.Epic)
			.SelectRandomWeightCountUnique(BlessWeightGenerator, blessCount, base.gameState.randomManager.NextDouble)
			.ConvertAll((InfluenceDef def) => new Influence(def));
		if (updateRecord)
		{
			list.ForEach(delegate(Influence inf)
			{
				base.gameState.recordManager.generatedInfluence.AddWithoutDuplicate(inf.def);
			});
		}
		return list;
		float BlessWeightGenerator(InfluenceDef bless)
		{
			float num = 1f;
			if (base.gameState.recordManager.generatedInfluence.Contains(bless))
			{
				num *= 0.5f;
			}
			if (base.gameState.playerState.bft.influences.Find((Influence t) => t.def == bless) != null)
			{
				num = 0f;
			}
			return num;
		}
	}

	public List<InfluenceDef> GenerateRandomCommonBless(int count, bool updateRecord, QualityLevel filterQuality)
	{
		List<InfluenceDef> list = new List<InfluenceDef>();
		List<InfluenceDef> allGenerateAbleBless = GetAllGenerateAbleBless();
		List<InfluenceDef> list2 = GrabFirstMaxXItemFromGuideListInPool(count, allGenerateAbleBless, base.gameState.applicationState.guideManager.guideBless);
		count -= list2.Count;
		List<InfluenceDef> pool = allGenerateAbleBless.Except(list2).ToList();
		list.AddRange(pool.SelectRandomWeightCountUnique(BlessWeightGenerator, count, base.gameState.randomManager.NextDouble));
		if (updateRecord)
		{
			list.ForEach(delegate(InfluenceDef inf)
			{
				base.gameState.recordManager.generatedInfluence.AddWithoutDuplicate(inf);
			});
		}
		list.AddRange(list2);
		return list;
		float BlessWeightGenerator(InfluenceDef bless)
		{
			float num = 0f;
			switch (bless.quality)
			{
			case QualityLevel.Common:
				num = 30f;
				break;
			case QualityLevel.Uncommon:
				num = 30f;
				break;
			case QualityLevel.Rare:
				num = 20f;
				break;
			}
			if (filterQuality != QualityLevel.None && bless.quality != filterQuality)
			{
				num = 0f;
			}
			if (base.gameState.recordManager.generatedInfluence.Contains(bless))
			{
				num *= 0.5f;
			}
			if (base.gameState.playerState.bft.influences.Find((Influence t) => t.def == bless) != null)
			{
				num = 0f;
			}
			return num;
		}
	}
}
