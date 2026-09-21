using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AssetCatalog : SerializedScriptableObject
{
	public AssetReference testMenuSceneRef;

	public AssetReference applicationSceneRef;

	public AssetReference mainMenuSceneRef;

	public AssetReference fortressManagerRef;

	public AssetReference gameManagerRef;

	public AssetDataCatalog<KeywordDef> keywordCatalog;

	public AssetDataCatalog<SkillDef> SkillDefCatalog;

	public AssetDataCatalog<ItemDefinition> itemDefCatalog;

	public AssetDataCatalog<EnchantDef> enchantDefCatalog;

	public AssetDataCatalog<RuneDef> runeDefCatalog;

	public AssetDataCatalog<ItemTag> itemTagDefCatalog;

	public AssetDataCatalog<CharacterClassDef> characterClassDefCatalog;

	public AssetDataCatalog<PlayerCharacterDef> characterDefCatalog;

	public AssetDataCatalog<WorldEvent> worldEventCatalog;

	public AssetDataCatalog<MatterDef> matterDefCatalog;

	public AssetDataCatalog<AchievementDef> achievementDefDefCatalog;

	public AssetDataCatalog<InfluenceDef> influenceCatalog;

	public AssetDataCatalog<Level> levelDefCatalog;

	public AssetDataCatalog<ContractDef> contractDefCatalog;

	public AssetDataCatalog<RoutineQuestDef> routineQuestCatalog;

	public AssetDataCatalog<SkillWordDef> skillWordCatalog;

	public AssetDataCatalog<SkillGemDef> skillGemCatalog;

	public AssetDataCatalog<DifficultyInfo> difficultyCatalog;

	public AssetDataCatalog<TalentDef> talentCatalog;

	public AssetDataCatalog<ClavisDef> clavisDefDefCatalog;

	public AssetDataCatalog<StoryPoint> storyPointCatalog;

	public AssetDataCatalog<EnvironmentInfluenceDef> environmentInfluenceCatalog;

	public AssetDataCatalog<LevelLineDef> levelLineDefCatalog;

	public AssetDataCatalog<SeasonDef> SeasonDefCatalog;

	public AssetDataCatalog<ActivityDef> activityCatalog;

	public AssetPreDataCatalog<DraftMapInfoData, DraftMap> draftMapCatalog;

	public AssetDataCatalog<ItemSkinDef> itemSkinCatalog;

	public AssetDataCatalog<ItemSkinBaseDef> itemSkinBaseCatalog;

	public AssetDataCatalog<ItemSkinSetDef> skinSetCatalog;

	public AssetDataCatalog<FestivalDef> festivalCatalog;

	public AssetDataCatalog<TalentTree> talentTreeCatalog;

	public AssetDataCatalog<Constellation> constellationCatalog;

	public AssetDataCatalog<ChallengeStarMap> starMapCatalog;

	public AssetDataCatalog<FortressNPC> fortressNPCCatalog;

	public AssetDataCatalog<StartGameBuffDef> startGameBuffCatalog;

	public AssetDataCatalog<CharacterMastery> characterMasteryCatalog;

	public AssetDataCatalog<GlobalRewardBundle> globalRewardBundleCatalog;

	public AssetDataCatalog<PurchaseGroupDef> purchaseGroupCatalog;

	public AssetDataCatalog<AvatarTalkData> avatarTalkDataCatalog;

	public AssetDataCatalog<RoomObjectDisplay> roomObjectDisplay;

	public AssetDataCatalog<GachaPoolDef> gachaPoolCatalog;

	public AssetDataCatalog<NFXSetup> nfxCatalog;

	public AssetDataCatalog<IdolDef> idolDefCatalog;

	public AssetPreDataCatalog<EnemyInfoData, CharacterAIComponent> enemyInfoDataCatalog;

	public AssetPreDataCatalog<CutsceneInfoData, Cutscene> cutsceneCatalog;

	public AssetPreDataCatalog<RoomInfoData, DungeonRoom> roomInfoDataCatalog;

	public List<ScriptableObject> singletons;

	[NonSerialized]
	private Dictionary<string, ICTAsset> assetIdDicCache;

	[NonSerialized]
	private List<SkillQualityIndexEntry> skillQualityIndexEntriesCache;

	[NonSerialized]
	private Dictionary<SkillDef, Dictionary<QualityLevel, SkillQualityIndexEntry>> skillQualityIndexEntryCache;

	public T GetSingleton<T>() where T : ScriptableObject
	{
		return singletons.Find((ScriptableObject t) => t is T) as T;
	}

	public ICollection<ICTAsset> GetAllResourceAssets()
	{
		if (assetIdDicCache == null)
		{
			assetIdDicCache = GenerateAssetIdDicCache();
		}
		return assetIdDicCache.Values;
	}

	public T FullIdToCTAsset<T>(string fullId) where T : class, ICTAsset
	{
		if (fullId.IsNullOrEmpty())
		{
			return null;
		}
		if (assetIdDicCache == null)
		{
			assetIdDicCache = GenerateAssetIdDicCache();
		}
		if (assetIdDicCache.TryGetValue(fullId, out var value))
		{
			return value as T;
		}
		return null;
	}

	public List<SkillQualityIndexEntry> GetSkillQualityIndexEntries()
	{
		EnsureSkillQualityIndexEntryCache();
		return skillQualityIndexEntriesCache;
	}

	public SkillQualityIndexEntry GetSkillQualityIndexEntry(SkillDef skillDef, int archetypeIndex)
	{
		if (skillDef?.skillArchetypes == null || archetypeIndex < 0 || archetypeIndex >= skillDef.skillArchetypes.Count)
		{
			return null;
		}
		return GetSkillQualityIndexEntry(skillDef, skillDef.skillArchetypes[archetypeIndex].qualityLevel);
	}

	public SkillQualityIndexEntry GetSkillQualityIndexEntry(SkillDef skillDef, QualityLevel qualityLevel)
	{
		if (skillDef == null)
		{
			return null;
		}
		EnsureSkillQualityIndexEntryCache();
		if (!skillQualityIndexEntryCache.TryGetValue(skillDef, out var value))
		{
			return null;
		}
		if (!value.TryGetValue(qualityLevel, out var value2))
		{
			return null;
		}
		return value2;
	}

	private void EnsureSkillQualityIndexEntryCache()
	{
		if (skillQualityIndexEntriesCache != null && skillQualityIndexEntryCache != null)
		{
			return;
		}
		skillQualityIndexEntriesCache = new List<SkillQualityIndexEntry>();
		skillQualityIndexEntryCache = new Dictionary<SkillDef, Dictionary<QualityLevel, SkillQualityIndexEntry>>();
		if (SkillDefCatalog?.ValidAssets == null)
		{
			return;
		}
		foreach (SkillDef validAsset in SkillDefCatalog.ValidAssets)
		{
			if (validAsset == null || validAsset.developingAssetFlag || validAsset.GetAttachCharacer(this) != null || validAsset.skillArchetypes.IsNullOrEmpty())
			{
				continue;
			}
			Dictionary<QualityLevel, SkillQualityIndexEntry> dictionary = new Dictionary<QualityLevel, SkillQualityIndexEntry>();
			for (int i = 0; i < validAsset.skillArchetypes.Count; i++)
			{
				SkillDef.SkillArchetype skillArchetype = validAsset.skillArchetypes[i];
				if (skillArchetype != null)
				{
					if (dictionary.ContainsKey(skillArchetype.qualityLevel))
					{
						TraceLog.Warn($"{validAsset.skillName} 存在重复护符品级 {skillArchetype.qualityLevel}，图鉴只使用第一个配置。");
						continue;
					}
					SkillQualityIndexEntry skillQualityIndexEntry = new SkillQualityIndexEntry(validAsset, i);
					dictionary.Add(skillArchetype.qualityLevel, skillQualityIndexEntry);
					skillQualityIndexEntriesCache.Add(skillQualityIndexEntry);
				}
			}
			if (dictionary.Count > 0)
			{
				skillQualityIndexEntryCache.Add(validAsset, dictionary);
			}
		}
	}

	private Dictionary<string, ICTAsset> GenerateAssetIdDicCache()
	{
		assetIdDicCache = new Dictionary<string, ICTAsset>();
		foreach (ICTAsset item in from t in GetAllDirectResourceAssetAssets()
			where t != null
			select t)
		{
			foreach (ICTAsset item2 in item.GetDescendantChild(includeself: true))
			{
				try
				{
					if (item2 != null && !assetIdDicCache.ContainsKey(item2.GetFullID()))
					{
						assetIdDicCache.Add(item2.GetFullID(), item2);
					}
				}
				catch
				{
				}
			}
		}
		return assetIdDicCache;
	}

	private List<ICTAsset> GetAllDirectResourceAssetAssets()
	{
		List<ICTAsset> list = new List<ICTAsset>();
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition().IsDerivedFromGenericParent(typeof(AssetDataCatalog<>)))
			{
				FieldInfo field = fieldInfo.FieldType.GetField("assets", BindingFlags.Instance | BindingFlags.Public);
				if (field == null)
				{
					continue;
				}
				try
				{
					foreach (object item in field.GetValue(fieldInfo.GetValue(this)) as IList)
					{
						if (item != null)
						{
							list.Add(item as ICTAsset);
						}
					}
				}
				catch
				{
				}
			}
			if (!fieldInfo.FieldType.IsDerivedFromGenericParent(typeof(AssetPreDataCatalog<, >)))
			{
				continue;
			}
			FieldInfo field2 = fieldInfo.FieldType.GetField("assets", BindingFlags.Instance | BindingFlags.Public);
			field2.GetValue(fieldInfo.GetValue(this));
			foreach (object item2 in field2.GetValue(fieldInfo.GetValue(this)) as IList)
			{
				if (item2 != null)
				{
					list.Add(item2 as ICTAsset);
				}
			}
		}
		foreach (ScriptableObject singleton in singletons)
		{
			if (singleton != null)
			{
				list.Add(singleton as ICTAsset);
			}
		}
		list.AddRange(GetSkillQualityIndexEntries());
		return list;
	}

	public List<T> GetTypeObjectsInCatalogs<T>() where T : class
	{
		List<T> list = new List<T>();
		foreach (ICTAsset allResourceAsset in GetAllResourceAssets())
		{
			if (allResourceAsset is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<object> GetTypeObjectsInCatalogs(Type t)
	{
		List<object> list = new List<object>();
		foreach (ICTAsset allResourceAsset in GetAllResourceAssets())
		{
			if (allResourceAsset.GetType() == t)
			{
				list.Add(allResourceAsset);
			}
		}
		return list;
	}
}
